using FlowKunevDev.Common;
using FlowKunevDev.Data.Models;

namespace FlowKunevDev.Services.Interfaces
{
    public interface ICategoryService
    {
        // Основни операции
        Task<Category?> GetByIdAsync(int id);
        Task<IEnumerable<Category>> GetAllAsync();
        Task<IEnumerable<Category>> GetActiveAsync();
        Task<IEnumerable<Category>> GetByTypeAsync(CategoryType type);

        // Категории за транзакции
        Task<IEnumerable<Category>> GetCategoriesForIncomeAsync();
        Task<IEnumerable<Category>> GetCategoriesForExpenseAsync();
        Task<IEnumerable<Category>> GetCategoriesForTypeAsync(TransactionType transactionType);

        // Валидации
        Task<bool> ExistsAsync(int id);
        Task<bool> IsValidForTransactionTypeAsync(int categoryId, TransactionType transactionType);

        // Статистики
        Task<Dictionary<int, int>> GetUsageStatsAsync(string userId);
        Task<IEnumerable<Category>> GetMostUsedAsync(string userId, int count = 10);
    }
}
