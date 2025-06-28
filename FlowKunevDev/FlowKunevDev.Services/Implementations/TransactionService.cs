using System.Globalization;
using FlowKunevDev.Common;
using FlowKunevDev.Data;
using FlowKunevDev.Data.Models;
using FlowKunevDev.Services.DTOs;
using FlowKunevDev.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlowKunevDev.Services.Implementations
{
    public class TransactionService : ITransactionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICategoryService _categoryService;

        public TransactionService(ApplicationDbContext context, ICategoryService categoryService)
        {
            _context = context;
            _categoryService = categoryService;
        }

        public async Task<TransactionDto?> GetByIdAsync(int id, string userId)
        {
            var transaction = await _context.Transactions
                .Include(t => t.Category)
                .Include(t => t.Account)
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (transaction == null) return null;

            return MapToDto(transaction);
        }

        public async Task<IEnumerable<TransactionDto>> GetAllAsync(string userId, TransactionFilterDto? filter = null)
        {
            var query = BuildFilterQuery(userId, filter);

            var transactions = await query
                .Include(t => t.Category)
                .Include(t => t.Account)
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.Id)
                .ToListAsync();

            return transactions.Select(MapToDto);
        }

        public async Task<(IEnumerable<TransactionDto> Items, int TotalCount)> GetPagedAsync(TransactionFilterDto filter, string userId)
        {
            var query = BuildFilterQuery(userId, filter);

            var totalCount = await query.CountAsync();

            var transactions = await query
                .Include(t => t.Category)
                .Include(t => t.Account)
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.Id)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return (transactions.Select(MapToDto), totalCount);
        }

        public async Task<TransactionDto> CreateAsync(CreateTransactionDto createDto, string userId)
        {
            // Валидации
            if (!await CanCreateAsync(createDto, userId))
            {
                throw new InvalidOperationException("Не може да се създаде транзакцията.");
            }

            var transaction = new Transaction
            {
                Description = createDto.Description,
                Amount = createDto.Amount,
                Date = createDto.Date,
                CategoryId = createDto.CategoryId,
                AccountId = createDto.AccountId,
                UserId = userId,
                Type = createDto.Type,
                Notes = createDto.Notes,
                CreatedDate = TimeHelper.UtcNow
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            // Зареждаме транзакцията с navigation properties
            await _context.Entry(transaction)
                .Reference(t => t.Category)
                .LoadAsync();
            await _context.Entry(transaction)
                .Reference(t => t.Account)
                .LoadAsync();

            return MapToDto(transaction);
        }

        public async Task<TransactionDto?> UpdateAsync(UpdateTransactionDto updateDto, string userId)
        {
            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.Id == updateDto.Id && t.UserId == userId);

            if (transaction == null) return null;

            if (!await CanUpdateAsync(updateDto, userId))
            {
                throw new InvalidOperationException("Не може да се обнови транзакцията.");
            }

            transaction.Description = updateDto.Description;
            transaction.Amount = updateDto.Amount;
            transaction.Date = updateDto.Date;
            transaction.CategoryId = updateDto.CategoryId;
            transaction.AccountId = updateDto.AccountId;
            transaction.Type = updateDto.Type;
            transaction.Notes = updateDto.Notes;

            await _context.SaveChangesAsync();

            return await GetByIdAsync(transaction.Id, userId);
        }

        public async Task<bool> DeleteAsync(int id, string userId)
        {
            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (transaction == null) return false;

            if (!await CanDeleteAsync(id, userId))
            {
                return false;
            }

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<TransactionSummaryDto>> GetRecentAsync(string userId, int count = 10)
        {
            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId)
                .Include(t => t.Category)
                .Include(t => t.Account)
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.Id)
                .Take(count)
                .ToListAsync();

            return transactions.Select(t => new TransactionSummaryDto
            {
                Id = t.Id,
                Description = t.Description,
                Amount = t.Amount,
                Date = t.Date,
                CategoryName = t.Category.Name,
                CategoryColor = t.Category.Color,
                AccountName = t.Account.Name,
                Type = t.Type
            });
        }

        public async Task<IEnumerable<TransactionDto>> GetByAccountAsync(int accountId, string userId, int? limit = null)
        {
            IQueryable<Transaction> query = _context.Transactions
                .Where(t => t.AccountId == accountId && t.UserId == userId)
                .Include(t => t.Category)
                .Include(t => t.Account)
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.Id);

            if (limit.HasValue)
                query = query.Take(limit.Value);

            var transactions = await query.ToListAsync();
            return transactions.Select(MapToDto);
        }

        public async Task<IEnumerable<TransactionDto>> GetByCategoryAsync(int categoryId, string userId, int? limit = null)
        {
            IQueryable<Transaction> query = _context.Transactions
                .Where(t => t.CategoryId == categoryId && t.UserId == userId)
                .Include(t => t.Category)
                .Include(t => t.Account)
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.Id);

            if (limit.HasValue)
                query = query.Take(limit.Value);

            var transactions = await query.ToListAsync();
            return transactions.Select(MapToDto);
        }

        public async Task<MonthlyTransactionSummaryDto> GetMonthlySummaryAsync(string userId, int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId && t.Date >= startDate && t.Date <= endDate)
                .Include(t => t.Category)
                .ToListAsync();

            var totalIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => (decimal?)t.Amount) ?? 0;
            var totalExpenses = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => (decimal?)t.Amount) ?? 0;

            var categoryBreakdown = transactions
                .GroupBy(t => new { t.CategoryId, t.Category.Name, t.Category.Color })
                .Select(g => new CategorySummaryDto
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.Name,
                    CategoryColor = g.Key.Color,
                    Amount = g.Sum(t => t.Amount),
                    TransactionCount = g.Count(),
                    Percentage = totalExpenses > 0 ? (g.Sum(t => t.Amount) / totalExpenses) * 100 : 0
                })
                .OrderByDescending(c => c.Amount)
                .ToList();

            return new MonthlyTransactionSummaryDto
            {
                Year = year,
                Month = month,
                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month),
                TotalIncome = totalIncome,
                TotalExpenses = totalExpenses,
                NetAmount = totalIncome - totalExpenses,
                TransactionCount = transactions.Count,
                CategoryBreakdown = categoryBreakdown
            };
        }

        public async Task<IEnumerable<MonthlyTransactionSummaryDto>> GetYearlySummaryAsync(string userId, int year)
        {
            var summaries = new List<MonthlyTransactionSummaryDto>();

            for (int month = 1; month <= 12; month++)
            {
                var summary = await GetMonthlySummaryAsync(userId, year, month);
                summaries.Add(summary);
            }

            return summaries;
        }

        public async Task<IEnumerable<CategorySummaryDto>> GetCategorySummaryAsync(string userId, DateTime? startDate = null, DateTime? endDate = null, TransactionType? type = null)
        {
            var query = _context.Transactions
                .Where(t => t.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(t => t.Date >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(t => t.Date <= endDate.Value);

            if (type.HasValue)
                query = query.Where(t => t.Type == type.Value);

            var transactions = await query
                .Include(t => t.Category)
                .ToListAsync();

            var totalAmount = transactions.Sum(t => (decimal?)t.Amount) ?? 0;

            return transactions
                .GroupBy(t => new { t.CategoryId, t.Category.Name, t.Category.Color })
                .Select(g => new CategorySummaryDto
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.Name,
                    CategoryColor = g.Key.Color,
                    Amount = g.Sum(t => t.Amount),
                    TransactionCount = g.Count(),
                    Percentage = totalAmount > 0 ? (g.Sum(t => t.Amount) / totalAmount) * 100 : 0
                })
                .OrderByDescending(c => c.Amount)
                .ToList();
        }

        public async Task<Dictionary<string, decimal>> GetMonthlyComparisonAsync(string userId, int months = 6)
        {
            var result = new Dictionary<string, decimal>();
            var currentDate = TimeHelper.LocalNow;

            for (int i = 0; i < months; i++)
            {
                var date = currentDate.AddMonths(-i);
                var summary = await GetMonthlySummaryAsync(userId, date.Year, date.Month);
                var key = $"{date.Year}-{date.Month:D2}";
                result[key] = summary.TotalExpenses;
            }

            return result.OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public async Task<Dictionary<string, decimal>> GetCategoryTrendsAsync(string userId, int categoryId, int months = 12)
        {
            var result = new Dictionary<string, decimal>();
            var currentDate = TimeHelper.LocalNow;

            for (int i = 0; i < months; i++)
            {
                var date = currentDate.AddMonths(-i);
                var startDate = new DateTime(date.Year, date.Month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var amount = await _context.Transactions
                    .Where(t => t.UserId == userId && t.CategoryId == categoryId &&
                               t.Date >= startDate && t.Date <= endDate)
                    .SumAsync(t => (decimal?)t.Amount) ?? 0;

                var key = $"{date.Year}-{date.Month:D2}";
                result[key] = amount;
            }

            return result.OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public async Task<Dictionary<string, object>> GetSpendingAnalysisAsync(string userId, DateTime startDate, DateTime endDate)
        {
            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId && t.Date >= startDate && t.Date <= endDate)
                .Include(t => t.Category)
                .ToListAsync();

            var totalIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => (decimal?)t.Amount) ?? 0;
            var totalExpenses = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => (decimal?)t.Amount) ?? 0;
            var averageDaily = transactions.Where(t => t.Type == TransactionType.Expense)
                .Sum(t => (decimal?)t.Amount) ?? 0 / Math.Max(1, (endDate - startDate).Days);

            var topCategories = transactions
                .Where(t => t.Type == TransactionType.Expense)
                .GroupBy(t => t.Category.Name)
                .Select(g => new { Category = g.Key, Amount = g.Sum(t => t.Amount) })
                .OrderByDescending(x => x.Amount)
                .Take(5)
                .ToList();

            return new Dictionary<string, object>
            {
                ["totalIncome"] = totalIncome,
                ["totalExpenses"] = totalExpenses,
                ["netAmount"] = totalIncome - totalExpenses,
                ["averageDailySpending"] = averageDaily,
                ["transactionCount"] = transactions.Count,
                ["topCategories"] = topCategories,
                ["savingsRate"] = totalIncome > 0 ? ((totalIncome - totalExpenses) / totalIncome) * 100 : 0
            };
        }

        public async Task<decimal> GetProjectedBalanceAsync(string userId, DateTime targetDate)
        {
            // Получаваме текущия общ баланс
            var accounts = await _context.Accounts
                .Where(a => a.UserId == userId && a.IsActive)
                .ToListAsync();

            decimal currentBalance = 0;
            foreach (var account in accounts)
            {
                currentBalance += await CalculateCurrentBalanceAsync(account.Id);
            }

            // Получаваме планираните транзакции до целевата дата
            var plannedTransactions = await _context.PlannedTransactions
                .Where(pt => pt.UserId == userId &&
                           pt.Status == PlannedTransactionStatus.Planned &&
                           pt.PlannedDate <= targetDate)
                .ToListAsync();

            decimal projectedChange = 0;
            foreach (var planned in plannedTransactions)
            {
                projectedChange += planned.Type == TransactionType.Income ?
                    planned.PlannedAmount : -planned.PlannedAmount;
            }

            return currentBalance + projectedChange;
        }

        // FlowKunevDev.Services/Implementations/TransactionService.cs
        // Заменете съществуващите методи и добавете новите

        public async Task<decimal> GetDailyAvailableAmountAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null, List<int>? accountIds = null)
        {
            // Ако не са подадени дати, използваме от днес до края на месеца
            var startDate = fromDate ?? TimeHelper.LocalNow.Date;
            var endDate = toDate ?? new DateTime(TimeHelper.LocalNow.Year, TimeHelper.LocalNow.Month, DateTime.DaysInMonth(TimeHelper.LocalNow.Year, TimeHelper.LocalNow.Month));

            // Изчисляваме броя дни (включително днешния ден)
            var days = Math.Max(1, (endDate - startDate).Days + 1);

            // Получаваме текущия общ баланс от избраните сметки
            var accountsQuery = _context.Accounts
                .Where(a => a.UserId == userId && a.IsActive);

            if (accountIds != null && accountIds.Any())
            {
                accountsQuery = accountsQuery.Where(a => accountIds.Contains(a.Id));
            }

            var accounts = await accountsQuery.ToListAsync();

            decimal totalBalance = 0;
            foreach (var account in accounts)
            {
                totalBalance += await CalculateCurrentBalanceAsync(account.Id);
            }

            // Получаваме планираните разходи за периода (само от избраните сметки)
            var plannedExpensesQuery = _context.PlannedTransactions
                .Where(pt => pt.UserId == userId &&
                           pt.Status == PlannedTransactionStatus.Planned &&
                           pt.Type == TransactionType.Expense &&
                           pt.PlannedDate >= startDate && pt.PlannedDate <= endDate);

            if (accountIds != null && accountIds.Any())
            {
                plannedExpensesQuery = plannedExpensesQuery.Where(pt => accountIds.Contains(pt.AccountId));
            }

            var plannedExpenses = await plannedExpensesQuery.SumAsync(pt => (decimal?)pt.PlannedAmount) ?? 0;

            // Получаваме планираните приходи за периода (само от избраните сметки)
            var plannedIncomeQuery = _context.PlannedTransactions
                .Where(pt => pt.UserId == userId &&
                           pt.Status == PlannedTransactionStatus.Planned &&
                           pt.Type == TransactionType.Income &&
                           pt.PlannedDate >= startDate && pt.PlannedDate <= endDate);

            if (accountIds != null && accountIds.Any())
            {
                plannedIncomeQuery = plannedIncomeQuery.Where(pt => accountIds.Contains(pt.AccountId));
            }

            var plannedIncome = await plannedIncomeQuery.SumAsync(pt => (decimal?)pt.PlannedAmount) ?? 0;

            // Изчисляваме наличните средства след планираните транзакции
            var availableAmount = totalBalance - plannedExpenses + plannedIncome;

            // Връщаме средно дневно (не може да бъде отрицателно)
            return Math.Max(0, availableAmount / days);
        }

        public async Task<decimal> GetAverageDailyExpensesAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null, int? lastDays = null, List<int>? accountIds = null)
        {
            DateTime startDate;
            DateTime endDate;

            if (lastDays.HasValue)
            {
                // Ако е зададен брой дни, използваме последните X дни
                endDate = TimeHelper.LocalNow.Date;
                startDate = endDate.AddDays(-lastDays.Value + 1);
            }
            else
            {
                // Ако не са подадени дати, използваме текущия месец
                startDate = fromDate ?? new DateTime(TimeHelper.LocalNow.Year, TimeHelper.LocalNow.Month, 1);
                endDate = toDate ?? TimeHelper.LocalNow.Date;
            }

            var days = Math.Max(1, (endDate - startDate).Days + 1);

            // Получаваме всички разходи за периода от избраните сметки
            var expensesQuery = _context.Transactions
                .Where(t => t.UserId == userId &&
                           t.Type == TransactionType.Expense &&
                           t.Date >= startDate && t.Date <= endDate);

            if (accountIds != null && accountIds.Any())
            {
                expensesQuery = expensesQuery.Where(t => accountIds.Contains(t.AccountId));
            }

            var totalExpenses = await expensesQuery.SumAsync(t => (decimal?)t.Amount) ?? 0;

            return totalExpenses / days;
        }

        public async Task<DailyBudgetInfoDto> GetDailyBudgetInfoAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null, List<int>? accountIds = null)
        {
            var startDate = fromDate ?? TimeHelper.LocalNow.Date;
            var endDate = toDate ?? new DateTime(TimeHelper.LocalNow.Year, TimeHelper.LocalNow.Month, DateTime.DaysInMonth(TimeHelper.LocalNow.Year, TimeHelper.LocalNow.Month));
            var days = Math.Max(1, (endDate - startDate).Days + 1);

            // Текущ баланс от избраните сметки
            var accountsQuery = _context.Accounts
                .Where(a => a.UserId == userId && a.IsActive);

            if (accountIds != null && accountIds.Any())
            {
                accountsQuery = accountsQuery.Where(a => accountIds.Contains(a.Id));
            }

            var accounts = await accountsQuery.ToListAsync();

            decimal totalBalance = 0;
            foreach (var account in accounts)
            {
                totalBalance += await CalculateCurrentBalanceAsync(account.Id);
            }

            // Безопасно проверяваме за планирани транзакции
            decimal plannedExpenses = 0;
            decimal plannedIncome = 0;

            try
            {
                // Опитваме се да заредим планирани транзакции от избраните сметки
                var plannedExpensesQuery = _context.PlannedTransactions
                    .Where(pt => pt.UserId == userId &&
                               pt.Status == PlannedTransactionStatus.Planned &&
                               pt.Type == TransactionType.Expense &&
                               pt.PlannedDate >= startDate && pt.PlannedDate <= endDate);

                if (accountIds != null && accountIds.Any())
                {
                    plannedExpensesQuery = plannedExpensesQuery.Where(pt => accountIds.Contains(pt.AccountId));
                }

                plannedExpenses = await plannedExpensesQuery.SumAsync(pt => (decimal?)pt.PlannedAmount) ?? 0;

                var plannedIncomeQuery = _context.PlannedTransactions
                    .Where(pt => pt.UserId == userId &&
                               pt.Status == PlannedTransactionStatus.Planned &&
                               pt.Type == TransactionType.Income &&
                               pt.PlannedDate >= startDate && pt.PlannedDate <= endDate);

                if (accountIds != null && accountIds.Any())
                {
                    plannedIncomeQuery = plannedIncomeQuery.Where(pt => accountIds.Contains(pt.AccountId));
                }

                plannedIncome = await plannedIncomeQuery.SumAsync(pt => (decimal?)pt.PlannedAmount) ?? 0;
            }
            catch (Exception)
            {
                // Ако има грешка (таблицата не съществува или друг проблем), използваме 0
                plannedExpenses = 0;
                plannedIncome = 0;
            }

            // Исторически среден дневен разход от избраните сметки
            var averageDailyExpenses = await GetAverageDailyExpensesAsync(userId, lastDays: 30, accountIds: accountIds);

            var availableAmount = totalBalance - plannedExpenses + plannedIncome;
            var dailyAvailable = Math.Max(0, availableAmount / days);

            return new DailyBudgetInfoDto
            {
                StartDate = startDate,
                EndDate = endDate,
                RemainingDays = days,
                TotalBalance = totalBalance,
                PlannedExpenses = plannedExpenses,
                PlannedIncome = plannedIncome,
                AvailableAmount = availableAmount,
                DailyAvailable = dailyAvailable,
                AverageDailyExpenses = averageDailyExpenses,
                RecommendedDailyBudget = dailyAvailable > 0 ? dailyAvailable : averageDailyExpenses,
                SelectedAccountIds = accountIds ?? new List<int>()
            };
        }

        public async Task<DailyBudgetInfoDto> GetDailyBudgetInfoWithAccountsAsync(string userId, DailyBudgetCalculationRequest request)
        {
            // Получаваме всички активни сметки
            var allAccounts = await _context.Accounts
                .Where(a => a.UserId == userId && a.IsActive)
                .Select(a => new AccountSummaryForBudget
                {
                    Id = a.Id,
                    Name = a.Name,
                    CurrentBalance = 0, // ще се изчисли по-късно
                    Color = a.Color,
                    Currency = a.Currency,
                    IsIncluded = request.IncludeAllAccounts || (request.SelectedAccountIds != null && request.SelectedAccountIds.Contains(a.Id))
                })
                .ToListAsync();

            // Изчисляваме балансите на сметките
            foreach (var account in allAccounts)
            {
                account.CurrentBalance = await CalculateCurrentBalanceAsync(account.Id);
            }

            // Определяме кои сметки да включим в изчислението
            List<int>? accountIds = null;
            if (!request.IncludeAllAccounts && request.SelectedAccountIds != null && request.SelectedAccountIds.Any())
            {
                accountIds = request.SelectedAccountIds;
            }

            // Получаваме основната информация
            var budgetInfo = await GetDailyBudgetInfoAsync(userId, request.FromDate, request.ToDate, accountIds);

            // Добавяме информацията за сметките
            budgetInfo.IncludedAccounts = allAccounts;
            budgetInfo.SelectedAccountIds = accountIds ?? allAccounts.Select(a => a.Id).ToList();

            return budgetInfo;
        }

        public async Task<bool> ExistsAsync(int id, string userId)
        {
            return await _context.Transactions
                .AnyAsync(t => t.Id == id && t.UserId == userId);
        }

        public async Task<bool> CanCreateAsync(CreateTransactionDto createDto, string userId)
        {
            // Проверяваме дали сметката съществува и принадлежи на потребителя
            var accountExists = await _context.Accounts
                .AnyAsync(a => a.Id == createDto.AccountId && a.UserId == userId && a.IsActive);

            if (!accountExists) return false;

            // Проверяваме дали категорията е валидна за типа транзакция
            var isValidCategory = await _categoryService
                .IsValidForTransactionTypeAsync(createDto.CategoryId, createDto.Type);

            return isValidCategory;
        }

        public async Task<bool> CanUpdateAsync(UpdateTransactionDto updateDto, string userId)
        {
            // Проверяваме дали сметката съществува и принадлежи на потребителя
            var accountExists = await _context.Accounts
                .AnyAsync(a => a.Id == updateDto.AccountId && a.UserId == userId && a.IsActive);

            if (!accountExists) return false;

            // Проверяваме дали категорията е валидна за типа транзакция
            var isValidCategory = await _categoryService
                .IsValidForTransactionTypeAsync(updateDto.CategoryId, updateDto.Type);

            return isValidCategory;
        }

        public async Task<bool> CanDeleteAsync(int id, string userId)
        {
            // Проверяваме дали транзакцията не е свързана с планирана транзакция
            var isLinkedToPlanned = await _context.PlannedTransactions
                .AnyAsync(pt => pt.ExecutedTransactionId == id);

            return !isLinkedToPlanned;
        }

        public async Task<IEnumerable<TransactionDto>> CreateMultipleAsync(IEnumerable<CreateTransactionDto> createDtos, string userId)
        {
            var results = new List<TransactionDto>();

            foreach (var createDto in createDtos)
            {
                try
                {
                    var result = await CreateAsync(createDto, userId);
                    results.Add(result);
                }
                catch
                {
                    // Логираме грешката, но продължаваме с останалите
                    continue;
                }
            }

            return results;
        }

        public async Task<bool> DeleteMultipleAsync(IEnumerable<int> ids, string userId)
        {
            var transactions = await _context.Transactions
                .Where(t => ids.Contains(t.Id) && t.UserId == userId)
                .ToListAsync();

            if (!transactions.Any()) return false;

            // Проверяваме дали всички могат да се изтриват
            foreach (var transaction in transactions)
            {
                if (!await CanDeleteAsync(transaction.Id, userId))
                {
                    return false;
                }
            }

            _context.Transactions.RemoveRange(transactions);
            await _context.SaveChangesAsync();

            return true;
        }

        private IQueryable<Transaction> BuildFilterQuery(string userId, TransactionFilterDto? filter)
        {
            var query = _context.Transactions.Where(t => t.UserId == userId);

            if (filter == null) return query;

            if (filter.StartDate.HasValue)
                query = query.Where(t => t.Date >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(t => t.Date <= filter.EndDate.Value);

            if (filter.CategoryId.HasValue)
                query = query.Where(t => t.CategoryId == filter.CategoryId.Value);

            if (filter.AccountId.HasValue)
                query = query.Where(t => t.AccountId == filter.AccountId.Value);

            if (filter.Type.HasValue)
                query = query.Where(t => t.Type == filter.Type.Value);

            if (filter.MinAmount.HasValue)
                query = query.Where(t => t.Amount >= filter.MinAmount.Value);

            if (filter.MaxAmount.HasValue)
                query = query.Where(t => t.Amount <= filter.MaxAmount.Value);

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.ToLower();
                query = query.Where(t => 
                    (t.Description != null && t.Description.ToLower().Contains(searchTerm)) ||
                    (t.Notes != null && t.Notes.ToLower().Contains(searchTerm)));
            }

            return query;
        }

        private static TransactionDto MapToDto(Transaction transaction)
        {
            return new TransactionDto
            {
                Id = transaction.Id,
                Description = transaction.Description,
                Amount = transaction.Amount,
                Date = transaction.Date,
                CategoryId = transaction.CategoryId,
                CategoryName = transaction.Category.Name,
                CategoryColor = transaction.Category.Color,
                CategoryIcon = transaction.Category.Icon,
                AccountId = transaction.AccountId,
                AccountName = transaction.Account.Name,
                UserId = transaction.UserId,
                Type = transaction.Type,
                Notes = transaction.Notes,
                CreatedDate = transaction.CreatedDate
            };
        }

        private async Task<decimal> CalculateCurrentBalanceAsync(int accountId)
        {
            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null) return 0;

            var incomeSum = await _context.Transactions
                .Where(t => t.AccountId == accountId && t.Type == TransactionType.Income)
                .SumAsync(t => (decimal?)t.Amount) ?? 0;

            var expenseSum = await _context.Transactions
                .Where(t => t.AccountId == accountId && t.Type == TransactionType.Expense)
                .SumAsync(t => (decimal?)t.Amount) ?? 0;

            var transfersIn = await _context.AccountTransfers
                .Where(at => at.ToAccountId == accountId)
                .SumAsync(at => (decimal?)at.Amount) ?? 0;

            var transfersOut = await _context.AccountTransfers
                .Where(at => at.FromAccountId == accountId)
                .SumAsync(at => (decimal?)at.Amount) ?? 0;

            return account.InitialBalance + incomeSum - expenseSum + transfersIn - transfersOut;
        }

        public async Task<decimal> GetProjectedBalanceAsync(string userId, DateTime targetDate, List<int>? accountIds = null)
        {
            // Получаваме текущия баланс от избраните сметки
            var accountsQuery = _context.Accounts
                .Where(a => a.UserId == userId && a.IsActive);

            if (accountIds != null && accountIds.Any())
            {
                accountsQuery = accountsQuery.Where(a => accountIds.Contains(a.Id));
            }

            var accounts = await accountsQuery.ToListAsync();

            decimal currentBalance = 0;
            foreach (var account in accounts)
            {
                currentBalance += await CalculateCurrentBalanceAsync(account.Id);
            }

            // Получаваме планираните транзакции до целевата дата
            var plannedQuery = _context.PlannedTransactions
                .Where(pt => pt.UserId == userId &&
                           pt.Status == PlannedTransactionStatus.Planned &&
                               pt.PlannedDate >= TimeHelper.LocalNow.Date &&
                           pt.PlannedDate <= targetDate);

            if (accountIds != null && accountIds.Any())
            {
                plannedQuery = plannedQuery.Where(pt => accountIds.Contains(pt.AccountId));
            }

            var plannedTransactions = await plannedQuery.ToListAsync();

            // Изчисляваме промяната от планираните транзакции
            decimal projectedChange = 0;
            foreach (var planned in plannedTransactions)
            {
                projectedChange += planned.Type == TransactionType.Income
                    ? planned.PlannedAmount
                    : -planned.PlannedAmount;
            }

            return currentBalance + projectedChange;
        }

        // Сравнение на разходи за периоди и категории

        public async Task<List<PeriodComparison>> GetPeriodComparisonAsync(string userId, ComparisonType type, DateTime baseDate, int periodsCount = 6)
        {
            var comparisons = new List<PeriodComparison>();

            for (int i = 0; i < periodsCount; i++)
            {
                var (startDate, endDate, periodName) = GetPeriodDates(baseDate, type, i);

                var summary = await GetPeriodSummaryAsync(userId, startDate, endDate);

                var comparison = new PeriodComparison
                {
                    PeriodName = periodName,
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalIncome = summary.TotalIncome,
                    TotalExpenses = summary.TotalExpenses,
                    Balance = summary.NetAmount,
                    IsCurrentPeriod = i == 0
                };

                // Изчисляваме промяната спрямо предишния период
                if (comparisons.Any())
                {
                    var previousPeriod = comparisons.Last();
                    comparison.ChangeFromPrevious = comparison.TotalExpenses - previousPeriod.TotalExpenses;
                    comparison.PercentageChange = previousPeriod.TotalExpenses > 0
                        ? (comparison.ChangeFromPrevious / previousPeriod.TotalExpenses) * 100
                        : 0;
                }

                comparisons.Add(comparison);
            }

            return comparisons.OrderBy(c => c.StartDate).ToList();
        }

        public async Task<List<CategoryComparison>> GetCategoryComparisonAsync(string userId, DateTime currentStart, DateTime currentEnd, DateTime previousStart, DateTime previousEnd)
        {
            // Текущ период
            var currentCategories = await GetCategorySummaryAsync(userId, currentStart, currentEnd, TransactionType.Expense);

            // Предишен период
            var previousCategories = await GetCategorySummaryAsync(userId, previousStart, previousEnd, TransactionType.Expense);

            var comparisons = new List<CategoryComparison>();

            // Обединяваме всички категории
            var allCategoryIds = currentCategories.Select(c => c.CategoryId)
                .Union(previousCategories.Select(c => c.CategoryId))
                .Distinct();

            foreach (var categoryId in allCategoryIds)
            {
                var current = currentCategories.FirstOrDefault(c => c.CategoryId == categoryId);
                var previous = previousCategories.FirstOrDefault(c => c.CategoryId == categoryId);

                var currentAmount = current?.Amount ?? 0;
                var previousAmount = previous?.Amount ?? 0;
                var change = currentAmount - previousAmount;
                var percentageChange = previousAmount > 0 ? (change / previousAmount) * 100 : 0;

                comparisons.Add(new CategoryComparison
                {
                    CategoryId = categoryId,
                    CategoryName = current?.CategoryName ?? previous?.CategoryName ?? "Неизвестна",
                    CategoryColor = current?.CategoryColor ?? previous?.CategoryColor ?? "#007bff",
                    CurrentPeriodAmount = currentAmount,
                    PreviousPeriodAmount = previousAmount,
                    Change = change,
                    PercentageChange = percentageChange
                });
            }

            return comparisons.OrderByDescending(c => Math.Abs(c.Change)).ToList();
        }

        public Task<ComparisonSummary> GetComparisonSummaryAsync(string userId, List<PeriodComparison> periods, List<CategoryComparison> categories)
        {
            var expenses = periods.Select(p => p.TotalExpenses).Where(e => e > 0).ToList();

            if (!expenses.Any())
            {
                return Task.FromResult(new ComparisonSummary());
            }

            var highest = periods.OrderByDescending(p => p.TotalExpenses).First();
            var lowest = periods.Where(p => p.TotalExpenses > 0).OrderBy(p => p.TotalExpenses).First();

            // Изчисляваме тренда (линейна регресия)
            var trend = CalculateExpensesTrend(periods);

            return Task.FromResult(new ComparisonSummary
            {
                AverageMonthlyExpenses = expenses.Average(),
                HighestMonthlyExpenses = highest.TotalExpenses,
                LowestMonthlyExpenses = lowest.TotalExpenses,
                HighestExpenseMonth = highest.PeriodName,
                LowestExpenseMonth = lowest.PeriodName,
                TotalExpensesAllPeriods = expenses.Sum(),
                ExpensesTrend = trend,
                TopGrowingCategories = categories
                    .Where(c => c.PercentageChange > 10)
                    .OrderByDescending(c => c.PercentageChange)
                    .Take(3)
                    .Select(c => c.CategoryName)
                    .ToList(),
                TopDecreasingCategories = categories
                    .Where(c => c.PercentageChange < -10)
                    .OrderBy(c => c.PercentageChange)
                    .Take(3)
                    .Select(c => c.CategoryName)
                    .ToList()
            });
        }

        private (DateTime startDate, DateTime endDate, string periodName) GetPeriodDates(DateTime baseDate, ComparisonType type, int periodsBack)
        {
            DateTime startDate, endDate;
            string periodName;

            switch (type)
            {
                case ComparisonType.Monthly:
                    var monthDate = baseDate.AddMonths(-periodsBack);
                    startDate = new DateTime(monthDate.Year, monthDate.Month, 1);
                    endDate = startDate.AddMonths(1).AddDays(-1);
                    periodName = monthDate.ToString("MMM yyyy", new System.Globalization.CultureInfo("bg-BG"));
                    break;

                case ComparisonType.Quarterly:
                    var quarterDate = baseDate.AddMonths(-periodsBack * 3);
                    var quarter = (quarterDate.Month - 1) / 3 + 1;
                    startDate = new DateTime(quarterDate.Year, (quarter - 1) * 3 + 1, 1);
                    endDate = startDate.AddMonths(3).AddDays(-1);
                    periodName = $"Q{quarter} {quarterDate.Year}";
                    break;

                case ComparisonType.Yearly:
                    var yearDate = baseDate.AddYears(-periodsBack);
                    startDate = new DateTime(yearDate.Year, 1, 1);
                    endDate = new DateTime(yearDate.Year, 12, 31);
                    periodName = yearDate.Year.ToString();
                    break;

                default:
                    throw new ArgumentException("Невалиден тип сравнение");
            }

            return (startDate, endDate, periodName);
        }

        private decimal CalculateExpensesTrend(List<PeriodComparison> periods)
        {
            if (periods.Count < 2) return 0;

            var orderedPeriods = periods.OrderBy(p => p.StartDate).ToList();
            var n = orderedPeriods.Count;

            // Проста линейна регресия за тренда
            var sumX = 0m;
            var sumY = 0m;
            var sumXY = 0m;
            var sumX2 = 0m;

            for (int i = 0; i < n; i++)
            {
                var x = i;
                var y = orderedPeriods[i].TotalExpenses;

                sumX += x;
                sumY += y;
                sumXY += x * y;
                sumX2 += x * x;
            }

            var slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
            return slope; // Positive = increasing trend, Negative = decreasing trend
        }

        private async Task<MonthlyTransactionSummaryDto> GetPeriodSummaryAsync(string userId, DateTime startDate, DateTime endDate)
        {
            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId && t.Date >= startDate && t.Date <= endDate)
                .Include(t => t.Category)
                .ToListAsync();

            var income = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
            var expenses = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

            return new MonthlyTransactionSummaryDto
            {
                Month = startDate.Month,
                Year = startDate.Year,
                TotalIncome = income,
                TotalExpenses = expenses,
                NetAmount = income - expenses,
                TransactionCount = transactions.Count,
                CategoryBreakdown = new List<CategorySummaryDto>()
            };
        }
    }
}