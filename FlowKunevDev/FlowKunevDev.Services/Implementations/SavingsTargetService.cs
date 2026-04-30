using FlowKunevDev.Common;
using FlowKunevDev.Data;
using FlowKunevDev.Services.DTOs;
using FlowKunevDev.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlowKunevDev.Services.Implementations
{
    public class SavingsTargetService : ISavingsTargetService
    {
        private readonly ApplicationDbContext _context;

        public SavingsTargetService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SavingsTargetDto> GetCurrentMonthTargetAsync(string userId)
        {
            var now = TimeHelper.LocalNow;
            var periodStart = new DateTime(now.Year, now.Month, 1);
            var periodEnd = periodStart.AddMonths(1).AddDays(-1);

            var dto = new SavingsTargetDto
            {
                PeriodStart = periodStart,
                PeriodEnd = periodEnd
            };

            var user = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => new
                {
                    u.SalaryAccountId,
                    u.SavingsAccountId,
                    u.MonthlySavingsPercent,
                    u.ExpectedMonthlyIncome
                })
                .FirstOrDefaultAsync();

            if (user == null
                || user.SalaryAccountId == null
                || user.SavingsAccountId == null
                || user.MonthlySavingsPercent == null
                || user.MonthlySavingsPercent <= 0)
            {
                dto.IsConfigured = false;
                return dto;
            }

            dto.IsConfigured = true;
            dto.SalaryAccountId = user.SalaryAccountId;
            dto.SavingsAccountId = user.SavingsAccountId;
            dto.Percent = user.MonthlySavingsPercent.Value;
            dto.ExpectedMonthlyIncome = user.ExpectedMonthlyIncome ?? 0;

            var accounts = await _context.Accounts
                .Where(a => a.UserId == userId
                            && (a.Id == user.SalaryAccountId || a.Id == user.SavingsAccountId))
                .Select(a => new { a.Id, a.Name })
                .ToListAsync();

            dto.SalaryAccountName = accounts.FirstOrDefault(a => a.Id == user.SalaryAccountId)?.Name;
            dto.SavingsAccountName = accounts.FirstOrDefault(a => a.Id == user.SavingsAccountId)?.Name;

            dto.RealizedIncome = await _context.Transactions
                .Where(t => t.UserId == userId
                            && t.AccountId == user.SalaryAccountId
                            && t.Type == TransactionType.Income
                            && t.Date >= periodStart
                            && t.Date <= periodEnd)
                .SumAsync(t => (decimal?)t.Amount) ?? 0;

            var transferredOut = await _context.AccountTransfers
                .Where(at => at.UserId == userId
                             && at.FromAccountId == user.SalaryAccountId
                             && at.ToAccountId == user.SavingsAccountId
                             && at.Date >= periodStart
                             && at.Date <= periodEnd)
                .SumAsync(at => (decimal?)at.Amount) ?? 0;

            var transferredBack = await _context.AccountTransfers
                .Where(at => at.UserId == userId
                             && at.FromAccountId == user.SavingsAccountId
                             && at.ToAccountId == user.SalaryAccountId
                             && at.Date >= periodStart
                             && at.Date <= periodEnd)
                .SumAsync(at => (decimal?)at.Amount) ?? 0;

            dto.Transferred = transferredOut - transferredBack;
            dto.Target = Math.Round(dto.RealizedIncome * dto.Percent / 100m, 2);
            dto.ForecastTarget = Math.Round(dto.ExpectedMonthlyIncome * dto.Percent / 100m, 2);

            return dto;
        }
    }
}
