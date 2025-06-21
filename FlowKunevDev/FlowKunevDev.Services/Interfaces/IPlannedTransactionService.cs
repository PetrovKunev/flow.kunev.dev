using FlowKunevDev.Common;
using FlowKunevDev.Services.DTOs;

namespace FlowKunevDev.Services.Interfaces
{
    public interface IPlannedTransactionService
    {
        // CRUD операции
        Task<PlannedTransactionDto?> GetByIdAsync(int id, string userId);
        Task<IEnumerable<PlannedTransactionDto>> GetAllAsync(string userId, PlannedTransactionFilterDto? filter = null);
        Task<(IEnumerable<PlannedTransactionDto> Items, int TotalCount)> GetPagedAsync(PlannedTransactionFilterDto filter, string userId);
        Task<PlannedTransactionDto> CreateAsync(CreatePlannedTransactionDto createDto, string userId);
        Task<PlannedTransactionDto?> UpdateAsync(UpdatePlannedTransactionDto updateDto, string userId);
        Task<bool> DeleteAsync(int id, string userId);

        // Статус операции
        Task<bool> CancelAsync(int id, string userId);
        Task<int> ExecuteAsync(int id, string userId); // Връща ID на създадената транзакция
        Task<int> ExecuteAsync(int id, string userId, decimal? actualAmount = null, DateTime? actualDate = null, string? actualNotes = null);

        // Автоматизация
        Task<IEnumerable<PlannedTransactionDto>> GetDueForExecutionAsync(DateTime? targetDate = null);
        Task<int> ExecuteDueTransactionsAsync(DateTime? targetDate = null);
        Task<int> ProcessRecurringTransactionsAsync();

        // Филтри и търсене
        Task<IEnumerable<PlannedTransactionSummaryDto>> GetUpcomingAsync(string userId, int days = 30);
        Task<IEnumerable<PlannedTransactionSummaryDto>> GetOverdueAsync(string userId);
        Task<IEnumerable<PlannedTransactionDto>> GetByStatusAsync(string userId, PlannedTransactionStatus status);
        Task<IEnumerable<PlannedTransactionDto>> GetRecurringAsync(string userId);

        // Статистики
        Task<decimal> GetTotalPlannedExpensesAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
        Task<decimal> GetTotalPlannedIncomeAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
        Task<Dictionary<string, object>> GetPlannedTransactionStatsAsync(string userId);

        // Валидации
        Task<bool> ExistsAsync(int id, string userId);
        Task<bool> CanCreateAsync(CreatePlannedTransactionDto createDto, string userId);
        Task<bool> CanUpdateAsync(UpdatePlannedTransactionDto updateDto, string userId);
        Task<bool> CanDeleteAsync(int id, string userId);
        Task<bool> CanExecuteAsync(int id, string userId);
    }
}
