using FlowKunevDev.Data.Models;
using FlowKunevDev.Services.DTOs;
using FlowKunevDev.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FlowKunevDev.Web.Controllers
{
    [Authorize]
    public class BudgetsController : Controller
    {
        private readonly IBudgetService _budgetService;
        private readonly ICategoryService _categoryService;
        private readonly UserManager<ApplicationUser> _userManager;

        public BudgetsController(IBudgetService budgetService, ICategoryService categoryService, UserManager<ApplicationUser> userManager)
        {
            _budgetService = budgetService;
            _categoryService = categoryService;
            _userManager = userManager;
        }

        // GET: Budgets
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            var budgets = await _budgetService.GetAllAsync(userId);
            return View(budgets);
        }

        // GET: Budgets/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            var budget = await _budgetService.GetByIdAsync(id, userId);
            if (budget == null)
            {
                return NotFound();
            }

            return View(budget);
        }

        // GET: Budgets/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _categoryService.GetCategoriesForExpenseAsync();
            return View();
        }

        // POST: Budgets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBudgetDto createDto)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _categoryService.GetCategoriesForExpenseAsync();
                return View(createDto);
            }

            try
            {
                var budget = await _budgetService.CreateAsync(createDto, userId);
                TempData["SuccessMessage"] = $"Бюджетът '{budget.Name}' беше създаден успешно!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Categories = await _categoryService.GetCategoriesForExpenseAsync();
                return View(createDto);
            }
        }

        // GET: Budgets/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            var budget = await _budgetService.GetByIdAsync(id, userId);
            if (budget == null)
            {
                return NotFound();
            }

            var updateDto = new UpdateBudgetDto
            {
                Id = budget.Id,
                Name = budget.Name,
                Amount = budget.Amount,
                CategoryId = budget.CategoryId,
                StartDate = budget.StartDate,
                EndDate = budget.EndDate,
                IsActive = budget.IsActive
            };
            ViewBag.Categories = await _categoryService.GetCategoriesForExpenseAsync();
            return View(updateDto);
        }

        // POST: Budgets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateBudgetDto updateDto)
        {
            if (id != updateDto.Id)
                return NotFound();

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _categoryService.GetCategoriesForExpenseAsync();
                return View(updateDto);
            }

            try
            {
                var budget = await _budgetService.UpdateAsync(updateDto, userId);
                if (budget == null)
                {
                    return NotFound();
                }
                TempData["SuccessMessage"] = $"Бюджетът '{budget.Name}' беше обновен успешно!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Categories = await _categoryService.GetCategoriesForExpenseAsync();
                return View(updateDto);
            }
        }

        // GET: Budgets/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            var budget = await _budgetService.GetByIdAsync(id, userId);
            if (budget == null)
            {
                return NotFound();
            }
            return View(budget);
        }

        // POST: Budgets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            var success = await _budgetService.DeleteAsync(id, userId);
            if (success)
            {
                TempData["SuccessMessage"] = "Бюджетът беше изтрит успешно!";
            }
            else
            {
                TempData["ErrorMessage"] = "Възникна грешка при изтриването на бюджета.";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Budgets/ToggleActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var userId = _userManager.GetUserId(User);
            var success = await _budgetService.ToggleActiveAsync(id, userId!);
            return RedirectToAction(nameof(Index));
        }
    }
}