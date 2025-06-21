using Microsoft.EntityFrameworkCore;
using FlowKunevDev.Data;
using FlowKunevDev.Data.Models;
using FlowKunevDev.Services.DTOs;
using FlowKunevDev.Services.Interfaces;

namespace FlowKunevDev.Services.Implementations
{
    public class AccountTransferService : IAccountTransferService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAccountService _accountService;

        public AccountTransferService(ApplicationDbContext context, IAccountService accountService)
        {
            _context = context;
            _accountService = accountService;
        }

        public async Task<AccountTransferDto?> GetByIdAsync(int id, string userId)
        {
            var transfer = await _context.AccountTransfers
                .Include(at => at.FromAccount)
                .Include(at => at.ToAccount)
                .FirstOrDefaultAsync(at => at.Id == id && at.UserId == userId);

            if (transfer == null) return null;

            return MapToDto(transfer);
        }

        public async Task<IEnumerable<AccountTransferDto>> GetAllAsync(string userId)
        {
            var transfers = await _context.AccountTransfers
                .Where(at => at.UserId == userId)
                .Include(at => at.FromAccount)
                .Include(at => at.ToAccount)
                .OrderByDescending(at => at.Date)
                .ThenByDescending(at => at.Id)
                .ToListAsync();

            return transfers.Select(MapToDto);
        }

        public async Task<AccountTransferDto> CreateAsync(CreateAccountTransferDto createDto, string userId)
        {
            // Валидации
            if (!await CanCreateAsync(createDto, userId))
            {
                throw new InvalidOperationException("Не може да се създаде трансферът.");
            }

            var transfer = new AccountTransfer
            {
                Amount = createDto.Amount,
                FromAccountId = createDto.FromAccountId,
                ToAccountId = createDto.ToAccountId,
                Date = createDto.Date,
                Description = createDto.Description,
                Notes = createDto.Notes,
                UserId = userId,
                CreatedDate = DateTime.Now
            };

            _context.AccountTransfers.Add(transfer);
            await _context.SaveChangesAsync();

            // Зареждаме трансфера с navigation properties
            await _context.Entry(transfer)
                .Reference(t => t.FromAccount)
                .LoadAsync();
            await _context.Entry(transfer)
                .Reference(t => t.ToAccount)
                .LoadAsync();

            return MapToDto(transfer);
        }

        public async Task<AccountTransferDto?> UpdateAsync(UpdateAccountTransferDto updateDto, string userId)
        {
            var transfer = await _context.AccountTransfers
                .FirstOrDefaultAsync(at => at.Id == updateDto.Id && at.UserId == userId);

            if (transfer == null) return null;

            if (!await CanUpdateAsync(updateDto, userId))
            {
                throw new InvalidOperationException("Не може да се обнови трансферът.");
            }

            transfer.Amount = updateDto.Amount;
            transfer.FromAccountId = updateDto.FromAccountId;
            transfer.ToAccountId = updateDto.ToAccountId;
            transfer.Date = updateDto.Date;
            transfer.Description = updateDto.Description;
            transfer.Notes = updateDto.Notes;

            await _context.SaveChangesAsync();

            return await GetByIdAsync(transfer.Id, userId);
        }

        public async Task<bool> DeleteAsync(int id, string userId)
        {
            var transfer = await _context.AccountTransfers
                .FirstOrDefaultAsync(at => at.Id == id && at.UserId == userId);

            if (transfer == null) return false;

            if (!await CanDeleteAsync(id, userId))
            {
                return false;
            }

            _context.AccountTransfers.Remove(transfer);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<AccountTransferDto>> GetByAccountAsync(int accountId, string userId, int? limit = null)
        {
            IQueryable<AccountTransfer> query = _context.AccountTransfers
                .Where(at => at.UserId == userId &&
                           (at.FromAccountId == accountId || at.ToAccountId == accountId))
                .Include(at => at.FromAccount)
                .Include(at => at.ToAccount)
                .OrderByDescending(at => at.Date)
                .ThenByDescending(at => at.Id);

            if (limit.HasValue)
                query = query.Take(limit.Value);

            var transfers = await query.ToListAsync();
            return transfers.Select(MapToDto);
        }

        public async Task<IEnumerable<AccountTransferDto>> GetByDateRangeAsync(string userId, DateTime startDate, DateTime endDate)
        {
            var transfers = await _context.AccountTransfers
                .Where(at => at.UserId == userId &&
                           at.Date >= startDate && at.Date <= endDate)
                .Include(at => at.FromAccount)
                .Include(at => at.ToAccount)
                .OrderByDescending(at => at.Date)
                .ThenByDescending(at => at.Id)
                .ToListAsync();

            return transfers.Select(MapToDto);
        }

        public async Task<IEnumerable<AccountTransferDto>> GetRecentAsync(string userId, int count = 10)
        {
            var transfers = await _context.AccountTransfers
                .Where(at => at.UserId == userId)
                .Include(at => at.FromAccount)
                .Include(at => at.ToAccount)
                .OrderByDescending(at => at.Date)
                .ThenByDescending(at => at.Id)
                .Take(count)
                .ToListAsync();

            return transfers.Select(MapToDto);
        }

        public async Task<bool> ExistsAsync(int id, string userId)
        {
            return await _context.AccountTransfers
                .AnyAsync(at => at.Id == id && at.UserId == userId);
        }

        public async Task<bool> CanCreateAsync(CreateAccountTransferDto createDto, string userId)
        {
            // Проверяваме дали сметките са различни
            if (createDto.FromAccountId == createDto.ToAccountId)
                return false;

            // Проверяваме дали сметките съществуват и принадлежат на потребителя
            var fromAccountExists = await _context.Accounts
                .AnyAsync(a => a.Id == createDto.FromAccountId && a.UserId == userId && a.IsActive);

            var toAccountExists = await _context.Accounts
                .AnyAsync(a => a.Id == createDto.ToAccountId && a.UserId == userId && a.IsActive);

            if (!fromAccountExists || !toAccountExists)
                return false;

            // Проверяваме дали има достатъчно средства
            var currentBalance = await _accountService.GetCurrentBalanceAsync(createDto.FromAccountId, userId);
            return currentBalance >= createDto.Amount;
        }

        public async Task<bool> CanUpdateAsync(UpdateAccountTransferDto updateDto, string userId)
        {
            // Същите проверки като при създаване
            if (updateDto.FromAccountId == updateDto.ToAccountId)
                return false;

            var fromAccountExists = await _context.Accounts
                .AnyAsync(a => a.Id == updateDto.FromAccountId && a.UserId == userId && a.IsActive);

            var toAccountExists = await _context.Accounts
                .AnyAsync(a => a.Id == updateDto.ToAccountId && a.UserId == userId && a.IsActive);

            return fromAccountExists && toAccountExists;
        }

        public async Task<bool> CanDeleteAsync(int id, string userId)
        {
            // Трансферите могат винаги да се изтриват
            return await ExistsAsync(id, userId);
        }

        public async Task<decimal> GetTotalTransferredAmountAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.AccountTransfers
                .Where(at => at.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(at => at.Date >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(at => at.Date <= endDate.Value);

            return await query.SumAsync(at => (decimal?)at.Amount) ?? 0;
        }

        public async Task<Dictionary<string, object>> GetTransferStatsAsync(string userId)
        {
            var transfers = await _context.AccountTransfers
                .Where(at => at.UserId == userId)
                .ToListAsync();

            var totalAmount = transfers.Sum(t => t.Amount);
            var averageAmount = transfers.Any() ? transfers.Average(t => t.Amount) : 0;

            var mostActiveFromAccount = transfers
                .GroupBy(t => t.FromAccountId)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();

            var mostActiveToAccount = transfers
                .GroupBy(t => t.ToAccountId)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();

            return new Dictionary<string, object>
            {
                ["totalTransfers"] = transfers.Count,
                ["totalAmount"] = totalAmount,
                ["averageAmount"] = averageAmount,
                ["thisMonthTransfers"] = transfers.Count(t => t.Date.Month == DateTime.Now.Month && t.Date.Year == DateTime.Now.Year),
                ["thisMonthAmount"] = transfers.Where(t => t.Date.Month == DateTime.Now.Month && t.Date.Year == DateTime.Now.Year).Sum(t => t.Amount),
                ["mostActiveFromAccountId"] = mostActiveFromAccount?.Key ?? 0,
                ["mostActiveFromAccountCount"] = mostActiveFromAccount?.Count() ?? 0,
                ["mostActiveToAccountId"] = mostActiveToAccount?.Key ?? 0,
                ["mostActiveToAccountCount"] = mostActiveToAccount?.Count() ?? 0
            };
        }

        public async Task<IEnumerable<AccountTransferDto>> GetMostFrequentTransfersAsync(string userId, int count = 5)
        {
            // Намираме най-честите комбинации от/към сметки
            var frequentTransfers = await _context.AccountTransfers
                .Where(at => at.UserId == userId)
                .GroupBy(at => new { at.FromAccountId, at.ToAccountId })
                .OrderByDescending(g => g.Count())
                .Take(count)
                .Select(g => g.OrderByDescending(t => t.Date).First())
                .Include(at => at.FromAccount)
                .Include(at => at.ToAccount)
                .ToListAsync();

            return frequentTransfers.Select(MapToDto);
        }

        private static AccountTransferDto MapToDto(AccountTransfer transfer)
        {
            return new AccountTransferDto
            {
                Id = transfer.Id,
                Amount = transfer.Amount,
                FromAccountId = transfer.FromAccountId,
                FromAccountName = transfer.FromAccount.Name,
                FromAccountColor = transfer.FromAccount.Color,
                ToAccountId = transfer.ToAccountId,
                ToAccountName = transfer.ToAccount.Name,
                ToAccountColor = transfer.ToAccount.Color,
                Date = transfer.Date,
                Description = transfer.Description ?? string.Empty,
                Notes = transfer.Notes ?? string.Empty,
                UserId = transfer.UserId,
                CreatedDate = transfer.CreatedDate
            };
        }
    }
}