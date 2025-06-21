using FlowKunevDev.Services.DTOs;

namespace FlowKunevDev.Services.Interfaces
{
    public interface IAccountTransferService
    {
        // CRUD операции
        Task<AccountTransferDto?> GetByIdAsync(int id, string userId);
        Task<IEnumerable<AccountTransferDto>> GetAllAsync(string userId);
        Task<AccountTransferDto> CreateAsync(CreateAccountTransferDto createDto, string userId);
        Task<AccountTransferDto?> UpdateAsync(UpdateAccountTransferDto updateDto, string userId);
        Task<bool> DeleteAsync(int id, string userId);

        // Филтриране и търсене
        Task<IEnumerable<AccountTransferDto>> GetByAccountAsync(int accountId, string userId, int? limit = null);
        Task<IEnumerable<AccountTransferDto>> GetByDateRangeAsync(string userId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<AccountTransferDto>> GetRecentAsync(string userId, int count = 10);

        // Валидации
        Task<bool> ExistsAsync(int id, string userId);
        Task<bool> CanCreateAsync(CreateAccountTransferDto createDto, string userId);
        Task<bool> CanUpdateAsync(UpdateAccountTransferDto updateDto, string userId);
        Task<bool> CanDeleteAsync(int id, string userId);

        // Статистики
        Task<decimal> GetTotalTransferredAmountAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
        Task<Dictionary<string, object>> GetTransferStatsAsync(string userId);
        Task<IEnumerable<AccountTransferDto>> GetMostFrequentTransfersAsync(string userId, int count = 5);
    }
}