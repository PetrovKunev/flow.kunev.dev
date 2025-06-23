using AutoMapper;
using FlowKunevDev.Common;
using FlowKunevDev.Data;
using FlowKunevDev.Data.Models;
using FlowKunevDev.Services.DTOs;
using FlowKunevDev.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlowKunevDev.Services.Implementations
{
    public class BudgetService : IBudgetService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public BudgetService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<BudgetDto?> GetByIdAsync(int id, string userId)
        {
            var budget = await _context.Budgets
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (budget == null) return null;

            var dto = _mapper.Map<BudgetDto>(budget);
            dto.CategoryName = budget.Category.Name;
            dto.SpentAmount = await CalculateSpentAmountAsync(budget);
            return dto;
        }

        public async Task<IEnumerable<BudgetDto>> GetAllAsync(string userId, bool onlyActive = false)
        {
            var query = _context.Budgets
                .Include(b => b.Category)
                .Where(b => b.UserId == userId);

            if (onlyActive)
                query = query.Where(b => b.IsActive);

            var budgets = await query
                .OrderByDescending(b => b.StartDate)
                .ToListAsync();

            var result = new List<BudgetDto>();
            foreach (var b in budgets)
            {
                var dto = _mapper.Map<BudgetDto>(b);
                dto.CategoryName = b.Category.Name;
                dto.SpentAmount = await CalculateSpentAmountAsync(b);
                result.Add(dto);
            }
            return result;
        }

        public async Task<BudgetDto> CreateAsync(CreateBudgetDto createDto, string userId)
        {
            if (await NameExistsAsync(createDto.Name, userId))
                throw new InvalidOperationException($"Бюджет с име '{createDto.Name}' вече съществува.");

            var budget = _mapper.Map<Budget>(createDto);
            budget.UserId = userId;
            _context.Budgets.Add(budget);
            await _context.SaveChangesAsync();

            return (await GetByIdAsync(budget.Id, userId))!;
        }

        public async Task<BudgetDto?> UpdateAsync(UpdateBudgetDto updateDto, string userId)
        {
            var budget = await _context.Budgets.FirstOrDefaultAsync(b => b.Id == updateDto.Id && b.UserId == userId);
            if (budget == null) return null;

            if (await NameExistsAsync(updateDto.Name, userId, updateDto.Id))
                throw new InvalidOperationException($"Бюджет с име '{updateDto.Name}' вече съществува.");

            _mapper.Map(updateDto, budget);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(budget.Id, userId);
        }

        public async Task<bool> DeleteAsync(int id, string userId)
        {
            var budget = await _context.Budgets.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
            if (budget == null) return false;

            _context.Budgets.Remove(budget);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleActiveAsync(int id, string userId)
        {
            var budget = await _context.Budgets.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
            if (budget == null) return false;

            budget.IsActive = !budget.IsActive;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id, string userId)
        {
            return await _context.Budgets.AnyAsync(b => b.Id == id && b.UserId == userId);
        }

        public async Task<bool> NameExistsAsync(string name, string userId, int? excludeId = null)
        {
            var query = _context.Budgets
                .Where(b => b.UserId == userId && b.Name.ToLower() == name.ToLower());
            if (excludeId.HasValue)
                query = query.Where(b => b.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        private async Task<decimal> CalculateSpentAmountAsync(Budget budget)
        {
            return await _context.Transactions
                .Where(t => t.UserId == budget.UserId
                             && t.CategoryId == budget.CategoryId
                             && t.Type == TransactionType.Expense
                             && t.Date >= budget.StartDate
                             && t.Date <= budget.EndDate)
                .SumAsync(t => (decimal?)t.Amount) ?? 0;
        }
    }
}
