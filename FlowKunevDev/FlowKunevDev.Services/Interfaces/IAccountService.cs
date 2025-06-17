using FlowKunevDev.Services.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowKunevDev.Services.Interfaces
{
    public interface IAccountService
    {
        // CRUD операции
        Task<AccountDto?> GetByIdAsync(int id, string userId);
        Task<IEnumerable<AccountDto>> GetAllAsync(string userId);
        Task<IEnumerable<AccountSummaryDto>> GetSummariesAsync(string userId);
        Task<AccountDto> CreateAsync(CreateAccountDto createDto, string userId);
        Task<AccountDto?> UpdateAsync(UpdateAccountDto updateDto, string userId);
        Task<bool> DeleteAsync(int id, string userId);
        Task<bool> ToggleActiveAsync(int id, string userId);

        // Баланс операции
        Task<decimal> GetCurrentBalanceAsync(int accountId, string userId);
        Task<decimal> GetTotalBalanceAsync(string userId);
        Task<IEnumerable<AccountBalanceHistoryDto>> GetBalanceHistoryAsync(int accountId, string userId, DateTime? fromDate = null, DateTime? toDate = null);

        // Валидации
        Task<bool> ExistsAsync(int id, string userId);
        Task<bool> NameExistsAsync(string name, string userId, int? excludeId = null);
        Task<bool> CanDeleteAsync(int id, string userId);
        Task<bool> HasSufficientBalanceAsync(int accountId, string userId, decimal amount);

        // Статистики
        Task<Dictionary<string, object>> GetAccountStatsAsync(int accountId, string userId);
        Task<Dictionary<string, object>> GetOverallStatsAsync(string userId);
    }
}
