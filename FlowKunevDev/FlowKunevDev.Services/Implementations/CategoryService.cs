using Microsoft.EntityFrameworkCore;
using FlowKunevDev.Data;
using FlowKunevDev.Data.Models;
using FlowKunevDev.Services.Interfaces;
using FlowKunevDev.Common;

namespace FlowKunevDev.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetActiveAsync()
        {
            return await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetByTypeAsync(CategoryType type)
        {
            return await _context.Categories
                .Where(c => c.IsActive && c.Type == type)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetCategoriesForIncomeAsync()
        {
            return await _context.Categories
                .Where(c => c.IsActive && (c.Type == CategoryType.IncomeOnly || c.Type == CategoryType.Both))
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetCategoriesForExpenseAsync()
        {
            return await _context.Categories
                .Where(c => c.IsActive && (c.Type == CategoryType.ExpenseOnly || c.Type == CategoryType.Both))
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetCategoriesForTypeAsync(TransactionType transactionType)
        {
            return transactionType switch
            {
                TransactionType.Income => await GetCategoriesForIncomeAsync(),
                TransactionType.Expense => await GetCategoriesForExpenseAsync(),
                _ => await GetActiveAsync()
            };
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Categories
                .AnyAsync(c => c.Id == id);
        }

        public async Task<bool> IsValidForTransactionTypeAsync(int categoryId, TransactionType transactionType)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == categoryId && c.IsActive);

            if (category == null) return false;

            return transactionType switch
            {
                TransactionType.Income => category.Type == CategoryType.IncomeOnly || category.Type == CategoryType.Both,
                TransactionType.Expense => category.Type == CategoryType.ExpenseOnly || category.Type == CategoryType.Both,
                _ => false
            };
        }

        public async Task<Dictionary<int, int>> GetUsageStatsAsync(string userId)
        {
            var stats = await _context.Transactions
                .Where(t => t.UserId == userId)
                .GroupBy(t => t.CategoryId)
                .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CategoryId, x => x.Count);

            return stats;
        }

        public async Task<IEnumerable<Category>> GetMostUsedAsync(string userId, int count = 10)
        {
            var mostUsedCategoryIds = await _context.Transactions
                .Where(t => t.UserId == userId)
                .GroupBy(t => t.CategoryId)
                .OrderByDescending(g => g.Count())
                .Take(count)
                .Select(g => g.Key)
                .ToListAsync();

            var categories = await _context.Categories
                .Where(c => mostUsedCategoryIds.Contains(c.Id))
                .ToListAsync();

            // Подреждаме по реда на използване
            return mostUsedCategoryIds
                .Select(id => categories.First(c => c.Id == id))
                .ToList();
        }
    }
}