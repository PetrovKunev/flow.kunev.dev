using FlowKunevDev.Services.DTOs;

namespace FlowKunevDev.Services.Interfaces
{
    public interface IBudgetService
    {
        Task<BudgetDto?> GetByIdAsync(int id, string userId);
        Task<IEnumerable<BudgetDto>> GetAllAsync(string userId, bool onlyActive = false);
        Task<BudgetDto> CreateAsync(CreateBudgetDto createDto, string userId);
        Task<BudgetDto?> UpdateAsync(UpdateBudgetDto updateDto, string userId);
        Task<bool> DeleteAsync(int id, string userId);
        Task<bool> ToggleActiveAsync(int id, string userId);
        Task<bool> ExistsAsync(int id, string userId);
        Task<bool> NameExistsAsync(string name, string userId, int? excludeId = null);
    }
}