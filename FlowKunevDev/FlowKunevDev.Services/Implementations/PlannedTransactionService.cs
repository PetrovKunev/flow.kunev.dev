using FlowKunevDev.Common;
using FlowKunevDev.Data;
using FlowKunevDev.Data.Models;
using FlowKunevDev.Services.DTOs;
using FlowKunevDev.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace FlowKunevDev.Services.Implementations
{
    public class PlannedTransactionService : IPlannedTransactionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICategoryService _categoryService;
        private readonly ITransactionService _transactionService;
        private readonly IMapper _mapper;

        public PlannedTransactionService(
            ApplicationDbContext context,
            ICategoryService categoryService,
            ITransactionService transactionService,
            IMapper mapper)
        {
            _context = context;
            _categoryService = categoryService;
            _transactionService = transactionService;
            _mapper = mapper;
        }

        public async Task<PlannedTransactionDto?> GetByIdAsync(int id, string userId)
        {
            var planned = await _context.PlannedTransactions
                .Include(pt => pt.Category)
                .Include(pt => pt.Account)
                .Include(pt => pt.ExecutedTransaction)
                .FirstOrDefaultAsync(pt => pt.Id == id && pt.UserId == userId);

            return planned == null ? null : _mapper.Map<PlannedTransactionDto>(planned);
        }

        public async Task<IEnumerable<PlannedTransactionDto>> GetAllAsync(string userId, PlannedTransactionFilterDto? filter = null)
        {
            var query = BuildFilterQuery(userId, filter);

            var planned = await query
                .Include(pt => pt.Category)
                .Include(pt => pt.Account)
                .OrderBy(pt => pt.PlannedDate)
                .ThenBy(pt => pt.Id)
                .ToListAsync();

            return planned.Select(pt => _mapper.Map<PlannedTransactionDto>(pt));
        }

        public async Task<(IEnumerable<PlannedTransactionDto> Items, int TotalCount)> GetPagedAsync(PlannedTransactionFilterDto filter, string userId)
        {
            var query = BuildFilterQuery(userId, filter);

            var totalCount = await query.CountAsync();

            var planned = await query
                .Include(pt => pt.Category)
                .Include(pt => pt.Account)
                .OrderBy(pt => pt.PlannedDate)
                .ThenBy(pt => pt.Id)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return (planned.Select(pt => _mapper.Map<PlannedTransactionDto>(pt)), totalCount);
        }

        public async Task<PlannedTransactionDto> CreateAsync(CreatePlannedTransactionDto createDto, string userId)
        {
            if (!await CanCreateAsync(createDto, userId))
            {
                throw new InvalidOperationException("Не може да се създаде планираната транзакция.");
            }

            var planned = _mapper.Map<PlannedTransaction>(createDto);
            planned.UserId = userId;

            _context.PlannedTransactions.Add(planned);
            await _context.SaveChangesAsync();

            await _context.Entry(planned)
                .Reference(pt => pt.Category)
                .LoadAsync();
            await _context.Entry(planned)
                .Reference(pt => pt.Account)
                .LoadAsync();

            return _mapper.Map<PlannedTransactionDto>(planned);
        }

        public async Task<PlannedTransactionDto?> UpdateAsync(UpdatePlannedTransactionDto updateDto, string userId)
        {
            var planned = await _context.PlannedTransactions
                .FirstOrDefaultAsync(pt => pt.Id == updateDto.Id && pt.UserId == userId);

            if (planned == null) return null;

            if (!await CanUpdateAsync(updateDto, userId))
            {
                throw new InvalidOperationException("Не може да се обнови планираната транзакция.");
            }

            _mapper.Map(updateDto, planned);

            await _context.SaveChangesAsync();

            return await GetByIdAsync(planned.Id, userId);
        }

        public async Task<bool> DeleteAsync(int id, string userId)
        {
            var planned = await _context.PlannedTransactions
                .FirstOrDefaultAsync(pt => pt.Id == id && pt.UserId == userId);

            if (planned == null || !await CanDeleteAsync(id, userId))
                return false;

            _context.PlannedTransactions.Remove(planned);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelAsync(int id, string userId)
        {
            var planned = await _context.PlannedTransactions
                .FirstOrDefaultAsync(pt => pt.Id == id && pt.UserId == userId);

            if (planned == null || planned.Status != PlannedTransactionStatus.Planned)
                return false;

            planned.Status = PlannedTransactionStatus.Cancelled;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> ExecuteAsync(int id, string userId)
        {
            return await ExecuteAsync(id, userId, null, null, null);
        }

        public async Task<int> ExecuteAsync(int id, string userId, decimal? actualAmount = null, DateTime? actualDate = null, string? actualNotes = null)
        {
            var planned = await _context.PlannedTransactions
                .FirstOrDefaultAsync(pt => pt.Id == id && pt.UserId == userId);

            if (planned == null || !await CanExecuteAsync(id, userId))
            {
                throw new InvalidOperationException("Не може да се изпълни планираната транзакция.");
            }

            // Създаваме реалната транзакция
            var createTransactionDto = new CreateTransactionDto
            {
                Description = planned.Description,
                Amount = actualAmount ?? planned.PlannedAmount,
                Date = actualDate ?? planned.PlannedDate,
                CategoryId = planned.CategoryId,
                AccountId = planned.AccountId,
                Type = planned.Type,
                Notes = actualNotes ?? planned.Notes ?? $"Автоматично изпълнена планирана транзакция (ID: {planned.Id})"
            };

            var executedTransaction = await _transactionService.CreateAsync(createTransactionDto, userId);

            // Обновяваме планираната транзакция
            planned.Status = PlannedTransactionStatus.Executed;
            planned.ExecutedTransactionId = executedTransaction.Id;

            // Ако е повтаряща се, създаваме следващата
            if (planned.IsRecurring && planned.RecurrenceType.HasValue)
            {
                await CreateNextRecurrenceAsync(planned);
            }

            await _context.SaveChangesAsync();
            return executedTransaction.Id;
        }

        public async Task<IEnumerable<PlannedTransactionDto>> GetDueForExecutionAsync(DateTime? targetDate = null)
        {
            var date = targetDate ?? DateTime.Now.Date;

            var dueTransactions = await _context.PlannedTransactions
                .Include(pt => pt.Category)
                .Include(pt => pt.Account)
                .Where(pt => pt.Status == PlannedTransactionStatus.Planned && pt.PlannedDate.Date <= date)
                .OrderBy(pt => pt.PlannedDate)
                .ToListAsync();

            return dueTransactions.Select(pt => _mapper.Map<PlannedTransactionDto>(pt));
        }

        public async Task<int> ExecuteDueTransactionsAsync(DateTime? targetDate = null)
        {
            var dueTransactions = await GetDueForExecutionAsync(targetDate);
            int executedCount = 0;

            foreach (var planned in dueTransactions)
            {
                try
                {
                    await ExecuteAsync(planned.Id, planned.UserId);
                    executedCount++;
                }
                catch
                {
                    // Логираме грешката, но продължаваме с останалите
                    continue;
                }
            }

            return executedCount;
        }

        public async Task<int> ProcessRecurringTransactionsAsync()
        {
            var recurringTransactions = await _context.PlannedTransactions
                .Where(pt => pt.IsRecurring && pt.Status == PlannedTransactionStatus.Executed && pt.RecurrenceType.HasValue)
                .ToListAsync();

            int processedCount = 0;

            foreach (var planned in recurringTransactions)
            {
                try
                {
                    // Проверяваме дали вече е създадена следващата итерация
                    var nextDate = CalculateNextRecurrenceDate(planned.PlannedDate, planned.RecurrenceType ?? throw new InvalidOperationException("RecurrenceType cannot be null."));
                    var existsNext = await _context.PlannedTransactions
                        .AnyAsync(pt => pt.UserId == planned.UserId &&
                                       pt.Description == planned.Description &&
                                       pt.PlannedDate.Date == nextDate.Date &&
                                       pt.Status == PlannedTransactionStatus.Planned);

                    if (!existsNext)
                    {
                        await CreateNextRecurrenceAsync(planned);
                        processedCount++;
                    }
                }
                catch
                {
                    continue;
                }
            }

            return processedCount;
        }

        public async Task<IEnumerable<PlannedTransactionSummaryDto>> GetUpcomingAsync(string userId, int days = 30)
        {
            var endDate = DateTime.Now.AddDays(days);

            var upcoming = await _context.PlannedTransactions
                .Include(pt => pt.Category)
                .Include(pt => pt.Account)
                .Where(pt => pt.UserId == userId &&
                           pt.Status == PlannedTransactionStatus.Planned &&
                           pt.PlannedDate.Date >= DateTime.Now.Date &&
                           pt.PlannedDate.Date <= endDate)
                .OrderBy(pt => pt.PlannedDate)
                .ToListAsync();

            return upcoming.Select(pt => _mapper.Map<PlannedTransactionSummaryDto>(pt));
        }

        public async Task<IEnumerable<PlannedTransactionSummaryDto>> GetOverdueAsync(string userId)
        {
            var overdue = await _context.PlannedTransactions
                .Include(pt => pt.Category)
                .Include(pt => pt.Account)
                .Where(pt => pt.UserId == userId &&
                           pt.Status == PlannedTransactionStatus.Planned &&
                           pt.PlannedDate.Date < DateTime.Now.Date)
                .OrderBy(pt => pt.PlannedDate)
                .ToListAsync();

            return overdue.Select(pt => _mapper.Map<PlannedTransactionSummaryDto>(pt));
        }

        public async Task<IEnumerable<PlannedTransactionDto>> GetByStatusAsync(string userId, PlannedTransactionStatus status)
        {
            var transactions = await _context.PlannedTransactions
                .Include(pt => pt.Category)
                .Include(pt => pt.Account)
                .Where(pt => pt.UserId == userId && pt.Status == status)
                .OrderBy(pt => pt.PlannedDate)
                .ToListAsync();

            return transactions.Select(pt => _mapper.Map<PlannedTransactionDto>(pt));
        }

        public async Task<IEnumerable<PlannedTransactionDto>> GetRecurringAsync(string userId)
        {
            var recurring = await _context.PlannedTransactions
                .Include(pt => pt.Category)
                .Include(pt => pt.Account)
                .Where(pt => pt.UserId == userId && pt.IsRecurring)
                .OrderBy(pt => pt.PlannedDate)
                .ToListAsync();

            return recurring.Select(pt => _mapper.Map<PlannedTransactionDto>(pt));
        }

        public async Task<decimal> GetTotalPlannedExpensesAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var start = startDate ?? DateTime.Now.Date;
            var end = endDate ?? DateTime.Now.AddDays(30).Date;

            return await _context.PlannedTransactions
                .Where(pt => pt.UserId == userId &&
                           pt.Status == PlannedTransactionStatus.Planned &&
                           pt.Type == TransactionType.Expense &&
                           pt.PlannedDate >= start && pt.PlannedDate <= end)
                .SumAsync(pt => (decimal?)pt.PlannedAmount) ?? 0;
        }

        public async Task<decimal> GetTotalPlannedIncomeAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var start = startDate ?? DateTime.Now.Date;
            var end = endDate ?? DateTime.Now.AddDays(30).Date;

            return await _context.PlannedTransactions
                .Where(pt => pt.UserId == userId &&
                           pt.Status == PlannedTransactionStatus.Planned &&
                           pt.Type == TransactionType.Income &&
                           pt.PlannedDate >= start && pt.PlannedDate <= end)
                .SumAsync(pt => (decimal?)pt.PlannedAmount) ?? 0;
        }

        public async Task<Dictionary<string, object>> GetPlannedTransactionStatsAsync(string userId)
        {
            var today = DateTime.Now.Date;
            var endOfMonth = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));

            var totalPlanned = await _context.PlannedTransactions
                .Where(pt => pt.UserId == userId && pt.Status == PlannedTransactionStatus.Planned)
                .CountAsync();

            var overdue = await _context.PlannedTransactions
                .Where(pt => pt.UserId == userId &&
                           pt.Status == PlannedTransactionStatus.Planned &&
                           pt.PlannedDate.Date < today)
                .CountAsync();

            var dueToday = await _context.PlannedTransactions
                .Where(pt => pt.UserId == userId &&
                           pt.Status == PlannedTransactionStatus.Planned &&
                           pt.PlannedDate.Date == today)
                .CountAsync();

            var thisMonth = await _context.PlannedTransactions
                .Where(pt => pt.UserId == userId &&
                           pt.Status == PlannedTransactionStatus.Planned &&
                           pt.PlannedDate >= today && pt.PlannedDate <= endOfMonth)
                .CountAsync();

            var recurringCount = await _context.PlannedTransactions
                .Where(pt => pt.UserId == userId && pt.IsRecurring)
                .CountAsync();

            return new Dictionary<string, object>
            {
                ["totalPlanned"] = totalPlanned,
                ["overdue"] = overdue,
                ["dueToday"] = dueToday,
                ["thisMonth"] = thisMonth,
                ["recurring"] = recurringCount
            };
        }

        // Валидации
        public async Task<bool> ExistsAsync(int id, string userId)
        {
            return await _context.PlannedTransactions
                .AnyAsync(pt => pt.Id == id && pt.UserId == userId);
        }

        public async Task<bool> CanCreateAsync(CreatePlannedTransactionDto createDto, string userId)
        {
            var accountExists = await _context.Accounts
                .AnyAsync(a => a.Id == createDto.AccountId && a.UserId == userId && a.IsActive);

            if (!accountExists) return false;

            var isValidCategory = await _categoryService
                .IsValidForTransactionTypeAsync(createDto.CategoryId, createDto.Type);

            return isValidCategory;
        }

        public async Task<bool> CanUpdateAsync(UpdatePlannedTransactionDto updateDto, string userId)
        {
            var planned = await _context.PlannedTransactions
                .FirstOrDefaultAsync(pt => pt.Id == updateDto.Id && pt.UserId == userId);

            if (planned == null || planned.Status != PlannedTransactionStatus.Planned)
                return false;

            var accountExists = await _context.Accounts
                .AnyAsync(a => a.Id == updateDto.AccountId && a.UserId == userId && a.IsActive);

            if (!accountExists) return false;

            var isValidCategory = await _categoryService
                .IsValidForTransactionTypeAsync(updateDto.CategoryId, updateDto.Type);

            return isValidCategory;
        }

        public async Task<bool> CanDeleteAsync(int id, string userId)
        {
            var planned = await _context.PlannedTransactions
                .FirstOrDefaultAsync(pt => pt.Id == id && pt.UserId == userId);

            return planned != null && planned.Status != PlannedTransactionStatus.Executed;
        }

        public async Task<bool> CanExecuteAsync(int id, string userId)
        {
            var planned = await _context.PlannedTransactions
                .FirstOrDefaultAsync(pt => pt.Id == id && pt.UserId == userId);

            return planned != null && planned.Status == PlannedTransactionStatus.Planned;
        }

        // Помощни методи
        private IQueryable<PlannedTransaction> BuildFilterQuery(string userId, PlannedTransactionFilterDto? filter)
        {
            var query = _context.PlannedTransactions.Where(pt => pt.UserId == userId);

            if (filter == null) return query;

            if (filter.StartDate.HasValue)
                query = query.Where(pt => pt.PlannedDate >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(pt => pt.PlannedDate <= filter.EndDate.Value);

            if (filter.CategoryId.HasValue)
                query = query.Where(pt => pt.CategoryId == filter.CategoryId.Value);

            if (filter.AccountId.HasValue)
                query = query.Where(pt => pt.AccountId == filter.AccountId.Value);

            if (filter.Type.HasValue)
                query = query.Where(pt => pt.Type == filter.Type.Value);

            if (filter.Status.HasValue)
                query = query.Where(pt => pt.Status == filter.Status.Value);

            if (filter.IsRecurring.HasValue)
                query = query.Where(pt => pt.IsRecurring == filter.IsRecurring.Value);

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                query = query.Where(pt => pt.Description.Contains(filter.SearchTerm) ||
                                         (pt.Notes != null && pt.Notes.Contains(filter.SearchTerm)));

            return query;
        }

        private async Task CreateNextRecurrenceAsync(PlannedTransaction originalPlanned)
        {
            if (!originalPlanned.IsRecurring || !originalPlanned.RecurrenceType.HasValue)
                return;

            var nextDate = CalculateNextRecurrenceDate(originalPlanned.PlannedDate, originalPlanned.RecurrenceType ?? throw new InvalidOperationException("RecurrenceType cannot be null."));

            var nextPlanned = new PlannedTransaction
            {
                Description = originalPlanned.Description,
                PlannedAmount = originalPlanned.PlannedAmount,
                PlannedDate = nextDate,
                CategoryId = originalPlanned.CategoryId,
                AccountId = originalPlanned.AccountId,
                UserId = originalPlanned.UserId,
                Type = originalPlanned.Type,
                Notes = originalPlanned.Notes,
                IsRecurring = originalPlanned.IsRecurring,
                RecurrenceType = originalPlanned.RecurrenceType,
                Status = PlannedTransactionStatus.Planned,
                CreatedDate = DateTime.Now
            };

            _context.PlannedTransactions.Add(nextPlanned);
            await _context.SaveChangesAsync();
        }

        private DateTime CalculateNextRecurrenceDate(DateTime currentDate, RecurrenceType recurrenceType)
        {
            return recurrenceType switch
            {
                RecurrenceType.Daily => currentDate.AddDays(1),
                RecurrenceType.Weekly => currentDate.AddDays(7),
                RecurrenceType.Monthly => currentDate.AddMonths(1),
                RecurrenceType.Yearly => currentDate.AddYears(1),
                _ => currentDate.AddMonths(1)
            };
        }
        // AutoMapper handles DTO conversions
    }

}
