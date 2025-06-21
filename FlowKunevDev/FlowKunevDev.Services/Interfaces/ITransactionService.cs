using FlowKunevDev.Common;
using FlowKunevDev.Services.DTOs;

namespace FlowKunevDev.Services.Interfaces
{
    public interface ITransactionService
    {
        // CRUD операции
        Task<TransactionDto?> GetByIdAsync(int id, string userId);
        Task<IEnumerable<TransactionDto>> GetAllAsync(string userId, TransactionFilterDto? filter = null);
        Task<(IEnumerable<TransactionDto> Items, int TotalCount)> GetPagedAsync(TransactionFilterDto filter, string userId);
        Task<TransactionDto> CreateAsync(CreateTransactionDto createDto, string userId);
        Task<TransactionDto?> UpdateAsync(UpdateTransactionDto updateDto, string userId);
        Task<bool> DeleteAsync(int id, string userId);

        // Последни транзакции
        Task<IEnumerable<TransactionSummaryDto>> GetRecentAsync(string userId, int count = 10);
        Task<IEnumerable<TransactionDto>> GetByAccountAsync(int accountId, string userId, int? limit = null);
        Task<IEnumerable<TransactionDto>> GetByCategoryAsync(int categoryId, string userId, int? limit = null);

        // Статистики и анализи
        Task<MonthlyTransactionSummaryDto> GetMonthlySummaryAsync(string userId, int year, int month);
        Task<IEnumerable<MonthlyTransactionSummaryDto>> GetYearlySummaryAsync(string userId, int year);
        Task<IEnumerable<CategorySummaryDto>> GetCategorySummaryAsync(string userId, DateTime? startDate = null, DateTime? endDate = null, TransactionType? type = null);

        // Сравнителни анализи
        Task<Dictionary<string, decimal>> GetMonthlyComparisonAsync(string userId, int months = 6);
        Task<Dictionary<string, decimal>> GetCategoryTrendsAsync(string userId, int categoryId, int months = 12);
        Task<Dictionary<string, object>> GetSpendingAnalysisAsync(string userId, DateTime startDate, DateTime endDate);

        // Дневни средства - ОБНОВЕНИ МЕТОДИ
        Task<decimal> GetDailyAvailableAmountAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null, List<int>? accountIds = null);
        Task<decimal> GetAverageDailyExpensesAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null, int? lastDays = null, List<int>? accountIds = null);
        Task<DailyBudgetInfoDto> GetDailyBudgetInfoAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null, List<int>? accountIds = null);
        Task<DailyBudgetInfoDto> GetDailyBudgetInfoWithAccountsAsync(string userId, DailyBudgetCalculationRequest request);
        Task<decimal> GetProjectedBalanceAsync(string userId, DateTime targetDate, List<int>? accountIds = null);

        // Валидации
        Task<bool> ExistsAsync(int id, string userId);
        Task<bool> CanCreateAsync(CreateTransactionDto createDto, string userId);
        Task<bool> CanUpdateAsync(UpdateTransactionDto updateDto, string userId);
        Task<bool> CanDeleteAsync(int id, string userId);

        // Batch операции
        Task<IEnumerable<TransactionDto>> CreateMultipleAsync(IEnumerable<CreateTransactionDto> createDtos, string userId);
        Task<bool> DeleteMultipleAsync(IEnumerable<int> ids, string userId);

    }
}