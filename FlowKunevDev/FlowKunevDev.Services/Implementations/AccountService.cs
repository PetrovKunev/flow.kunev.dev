using Microsoft.EntityFrameworkCore;
using AutoMapper;
using FlowKunevDev.Data;
using FlowKunevDev.Data.Models;
using FlowKunevDev.Services.DTOs;
using FlowKunevDev.Services.Interfaces;
using FlowKunevDev.Common;

namespace FlowKunevDev.Services.Implementations
{
    public class AccountService : IAccountService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public AccountService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<AccountDto?> GetByIdAsync(int id, string userId)
        {
            var account = await _context.Accounts
                .Where(a => a.Id == id && a.UserId == userId)
                .FirstOrDefaultAsync();

            if (account == null) return null;

            var accountDto = _mapper.Map<AccountDto>(account);
            accountDto.CurrentBalance = await CalculateCurrentBalanceAsync(account.Id);
            accountDto.TransactionCount = await _context.Transactions
                .CountAsync(t => t.AccountId == account.Id);
            accountDto.LastTransactionDate = await _context.Transactions
                .Where(t => t.AccountId == account.Id)
                .OrderByDescending(t => t.Date)
                .Select(t => t.Date)
                .FirstOrDefaultAsync();

            if (accountDto.LastTransactionDate == default)
                accountDto.LastTransactionDate = null;

            return accountDto;
        }

        public async Task<IEnumerable<AccountDto>> GetAllAsync(string userId)
        {
            var accounts = await _context.Accounts
                .Where(a => a.UserId == userId)
                .OrderBy(a => a.Name)
                .ToListAsync();

            var accountDtos = new List<AccountDto>();

            foreach (var account in accounts)
            {
                var currentBalance = await CalculateCurrentBalanceAsync(account.Id);
                var transactionCount = await _context.Transactions
                    .CountAsync(t => t.AccountId == account.Id);
                var lastTransactionDate = await _context.Transactions
                    .Where(t => t.AccountId == account.Id)
                    .OrderByDescending(t => t.Date)
                    .Select(t => t.Date)
                    .FirstOrDefaultAsync();

                accountDtos.Add(new AccountDto
                {
                    Id = account.Id,
                    Name = account.Name,
                    Description = account.Description ?? string.Empty, // Fix for CS8601
                    InitialBalance = account.InitialBalance,
                    CurrentBalance = currentBalance,
                    Type = account.Type,
                    Currency = account.Currency,
                    Color = account.Color,
                    UserId = account.UserId,
                    CreatedDate = account.CreatedDate,
                    IsActive = account.IsActive,
                    TransactionCount = transactionCount,
                    LastTransactionDate = lastTransactionDate == default ? null : lastTransactionDate
                });
            }

            return accountDtos;
        }

        public async Task<IEnumerable<AccountSummaryDto>> GetSummariesAsync(string userId)
        {
            var accounts = await _context.Accounts
                .Where(a => a.UserId == userId && a.IsActive)
                .OrderBy(a => a.Name)
                .ToListAsync();

            var summaries = new List<AccountSummaryDto>();

            foreach (var account in accounts)
            {
                var currentBalance = await CalculateCurrentBalanceAsync(account.Id);
                summaries.Add(new AccountSummaryDto
                {
                    Id = account.Id,
                    Name = account.Name,
                    CurrentBalance = currentBalance,
                    Currency = account.Currency,
                    Color = account.Color,
                    Type = account.Type,
                    IsActive = account.IsActive
                });
            }

            return summaries;
        }

        public async Task<AccountDto> CreateAsync(CreateAccountDto createDto, string userId)
        {
            // Проверка за уникално име
            var nameExists = await NameExistsAsync(createDto.Name, userId);
            if (nameExists)
            {
                throw new InvalidOperationException($"Сметка с име '{createDto.Name}' вече съществува.");
            }

            var account = _mapper.Map<Account>(createDto);
            account.UserId = userId;

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            var accountDto = _mapper.Map<AccountDto>(account);
            accountDto.CurrentBalance = account.InitialBalance; // Първоначално баланса е равен на началния
            accountDto.TransactionCount = 0;
            accountDto.LastTransactionDate = null;

            return accountDto;
        }

        public async Task<AccountDto?> UpdateAsync(UpdateAccountDto updateDto, string userId)
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == updateDto.Id && a.UserId == userId);

            if (account == null) return null;

            // Проверка за уникално име (изключваме текущата сметка)
            var nameExists = await NameExistsAsync(updateDto.Name, userId, updateDto.Id);
            if (nameExists)
            {
                throw new InvalidOperationException($"Сметка с име '{updateDto.Name}' вече съществува.");
            }

            _mapper.Map(updateDto, account);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(account.Id, userId);
        }

        public async Task<bool> DeleteAsync(int id, string userId)
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (account == null) return false;

            // Проверка дали може да се изтрие
            var canDelete = await CanDeleteAsync(id, userId);
            if (!canDelete) return false;

            // Soft delete - просто я деактивираме
            account.IsActive = false;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ToggleActiveAsync(int id, string userId)
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (account == null) return false;

            account.IsActive = !account.IsActive;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<decimal> GetCurrentBalanceAsync(int accountId, string userId)
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId);

            if (account == null) return 0;

            return await CalculateCurrentBalanceAsync(accountId);
        }

        public async Task<decimal> GetTotalBalanceAsync(string userId)
        {
            var accounts = await _context.Accounts
                .Where(a => a.UserId == userId && a.IsActive)
                .ToListAsync();

            decimal totalBalance = 0;
            foreach (var account in accounts)
            {
                totalBalance += await CalculateCurrentBalanceAsync(account.Id);
            }

            return totalBalance;
        }

        public async Task<IEnumerable<AccountBalanceHistoryDto>> GetBalanceHistoryAsync(int accountId, string userId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId);

            if (account == null) return new List<AccountBalanceHistoryDto>();

            var query = _context.Transactions
                .Where(t => t.AccountId == accountId);

            if (fromDate.HasValue)
                query = query.Where(t => t.Date >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(t => t.Date <= toDate.Value);

            var transactions = await query
                .OrderBy(t => t.Date)
                .ThenBy(t => t.Id)
                .ToListAsync();

            var history = new List<AccountBalanceHistoryDto>();
            decimal runningBalance = account.InitialBalance;

            // Добавяме началния баланс
            history.Add(new AccountBalanceHistoryDto
            {
                Date = account.CreatedDate,
                Balance = runningBalance,
                TransactionDescription = "Начален баланс",
                TransactionAmount = 0
            });

            // Изчисляваме баланса след всяка транзакция
            foreach (var transaction in transactions)
            {
                var amount = transaction.Type == TransactionType.Income ? transaction.Amount : -transaction.Amount;
                runningBalance += amount;

                history.Add(new AccountBalanceHistoryDto
                {
                    Date = transaction.Date,
                    Balance = runningBalance,
                    TransactionDescription = transaction.Description,
                    TransactionAmount = amount
                });
            }

            return history;
        }

        public async Task<bool> ExistsAsync(int id, string userId)
        {
            return await _context.Accounts
                .AnyAsync(a => a.Id == id && a.UserId == userId);
        }

        public async Task<bool> NameExistsAsync(string name, string userId, int? excludeId = null)
        {
            var query = _context.Accounts
                .Where(a => a.UserId == userId && a.Name.ToLower() == name.ToLower());

            if (excludeId.HasValue)
                query = query.Where(a => a.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task<bool> CanDeleteAsync(int id, string userId)
        {
            // Проверяваме дали има транзакции или трансфери
            var hasTransactions = await _context.Transactions
                .AnyAsync(t => t.AccountId == id && t.UserId == userId);

            var hasTransfersFrom = await _context.AccountTransfers
                .AnyAsync(at => at.FromAccountId == id && at.UserId == userId);

            var hasTransfersTo = await _context.AccountTransfers
                .AnyAsync(at => at.ToAccountId == id && at.UserId == userId);

            var hasPlannedTransactions = await _context.PlannedTransactions
                .AnyAsync(pt => pt.AccountId == id && pt.UserId == userId);

            // Ако има свързани записи, не може да се изтрие
            return !(hasTransactions || hasTransfersFrom || hasTransfersTo || hasPlannedTransactions);
        }

        public async Task<bool> HasSufficientBalanceAsync(int accountId, string userId, decimal amount)
        {
            var currentBalance = await GetCurrentBalanceAsync(accountId, userId);
            return currentBalance >= amount;
        }

        public async Task<Dictionary<string, object>> GetAccountStatsAsync(int accountId, string userId)
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId);

            if (account == null) return new Dictionary<string, object>();

            var currentBalance = await CalculateCurrentBalanceAsync(accountId);
            var transactionCount = await _context.Transactions
                .CountAsync(t => t.AccountId == accountId);

            var incomeSum = await _context.Transactions
                .Where(t => t.AccountId == accountId && t.Type == TransactionType.Income)
                .SumAsync(t => (decimal?)t.Amount) ?? 0;

            var expenseSum = await _context.Transactions
                .Where(t => t.AccountId == accountId && t.Type == TransactionType.Expense)
                .SumAsync(t => (decimal?)t.Amount) ?? 0;

            var lastTransaction = await _context.Transactions
                .Where(t => t.AccountId == accountId)
                .OrderByDescending(t => t.Date)
                .FirstOrDefaultAsync();

            var avgTransactionAmount = transactionCount > 0 ? (incomeSum + expenseSum) / transactionCount : 0;

            return new Dictionary<string, object>
            {
                ["currentBalance"] = currentBalance,
                ["initialBalance"] = account.InitialBalance,
                ["totalIncome"] = incomeSum,
                ["totalExpenses"] = expenseSum,
                ["netChange"] = incomeSum - expenseSum,
                ["transactionCount"] = transactionCount,
                ["averageTransactionAmount"] = avgTransactionAmount,
                ["lastTransactionDate"] = lastTransaction?.Date.ToString("yyyy-MM-ddTHH") ?? (object)DBNull.Value,
                ["lastTransactionAmount"] = lastTransaction?.Amount ?? 0,
                ["accountAge"] = TimeHelper.LocalNow.Subtract(account.CreatedDate).Days
            };
        }

        public async Task<Dictionary<string, object>> GetOverallStatsAsync(string userId)
        {
            var accounts = await _context.Accounts
                .Where(a => a.UserId == userId && a.IsActive)
                .ToListAsync();

            var totalBalance = decimal.Zero;
            var totalInitialBalance = decimal.Zero;
            var accountCount = accounts.Count;

            foreach (var account in accounts)
            {
                totalBalance += await CalculateCurrentBalanceAsync(account.Id);
                totalInitialBalance += account.InitialBalance;
            }

            var totalIncome = await _context.Transactions
                .Where(t => t.UserId == userId && t.Type == TransactionType.Income)
                .SumAsync(t => (decimal?)t.Amount) ?? 0;

            var totalExpenses = await _context.Transactions
                .Where(t => t.UserId == userId && t.Type == TransactionType.Expense)
                .SumAsync(t => (decimal?)t.Amount) ?? 0;

            var transactionCount = await _context.Transactions
                .CountAsync(t => t.UserId == userId);

            var accountsByType = accounts.GroupBy(a => a.Type)
                .ToDictionary(g => g.Key.ToString(), g => g.Count());

            return new Dictionary<string, object>
            {
                ["totalBalance"] = totalBalance,
                ["totalInitialBalance"] = totalInitialBalance,
                ["netWorthChange"] = totalBalance - totalInitialBalance,
                ["accountCount"] = accountCount,
                ["totalIncome"] = totalIncome,
                ["totalExpenses"] = totalExpenses,
                ["netIncome"] = totalIncome - totalExpenses,
                ["transactionCount"] = transactionCount,
                ["averageAccountBalance"] = accountCount > 0 ? totalBalance / accountCount : 0,
                ["accountsByType"] = accountsByType
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

            // Добавяме входящи трансфери
            var transfersIn = await _context.AccountTransfers
                .Where(at => at.ToAccountId == accountId)
                .SumAsync(at => (decimal?)at.Amount) ?? 0;

            // Извадяме изходящи трансфери
            var transfersOut = await _context.AccountTransfers
                .Where(at => at.FromAccountId == accountId)
                .SumAsync(at => (decimal?)at.Amount) ?? 0;

            return account.InitialBalance + incomeSum - expenseSum + transfersIn - transfersOut;
        }
    }
}