using FlowKunevDev.Common;
using FlowKunevDev.Data.Models;
using FlowKunevDev.Services.DTOs;
using FlowKunevDev.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FlowKunevDev.Web.Controllers
{
    [Authorize]
    public class TransactionsController : Controller
    {
        private readonly ITransactionService _transactionService;
        private readonly IAccountService _accountService;
        private readonly ICategoryService _categoryService;
        private readonly UserManager<ApplicationUser> _userManager;

        public TransactionsController(
            ITransactionService transactionService,
            IAccountService accountService,
            ICategoryService categoryService,
            UserManager<ApplicationUser> userManager)
        {
            _transactionService = transactionService;
            _accountService = accountService;
            _categoryService = categoryService;
            _userManager = userManager;
        }

        // GET: Transactions
        public async Task<IActionResult> Index(TransactionFilterDto? filter)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            // Ако няма филтър, създаваме празен
            filter ??= new TransactionFilterDto();

            try
            {
                var (transactions, totalCount) = await _transactionService.GetPagedAsync(filter, userId);

                // За dropdown списъците
                ViewBag.Accounts = await _accountService.GetSummariesAsync(userId);
                ViewBag.Categories = await _categoryService.GetActiveAsync();
                ViewBag.Filter = filter;
                ViewBag.TotalCount = totalCount;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);

                return View(transactions);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Възникна грешка при зареждането на транзакциите.";
                return View(new List<TransactionDto>());
            }
        }

        // GET: Transactions/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            var transaction = await _transactionService.GetByIdAsync(id, userId);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // GET: Transactions/Create
        public async Task<IActionResult> Create(int? accountId, TransactionType? type)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            var createDto = new CreateTransactionDto();

            // Ако е подаден account ID, го задаваме по подразбиране
            if (accountId.HasValue)
                createDto.AccountId = accountId.Value;

            // Ако е подаден тип, го задаваме по подразбиране
            if (type.HasValue)
                createDto.Type = type.Value;

            await PrepareCreateViewBagAsync(userId, createDto.Type);
            return View(createDto);
        }

        // POST: Transactions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTransactionDto createDto)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            if (!ModelState.IsValid)
            {
                await PrepareCreateViewBagAsync(userId, createDto.Type);
                return View(createDto);
            }

            try
            {
                var transaction = await _transactionService.CreateAsync(createDto, userId);
                TempData["SuccessMessage"] = $"Транзакцията '{transaction.Description}' беше създадена успешно!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                await PrepareCreateViewBagAsync(userId, createDto.Type);
                return View(createDto);
            }
        }

        // GET: Transactions/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            var transaction = await _transactionService.GetByIdAsync(id, userId);
            if (transaction == null)
            {
                return NotFound();
            }

            var updateDto = new UpdateTransactionDto
            {
                Id = transaction.Id,
                Description = transaction.Description,
                Amount = transaction.Amount,
                Date = transaction.Date,
                CategoryId = transaction.CategoryId,
                AccountId = transaction.AccountId,
                Type = transaction.Type,
                Notes = transaction.Notes
            };

            await PrepareEditViewBagAsync(userId, updateDto.Type);
            return View(updateDto);
        }

        // POST: Transactions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateTransactionDto updateDto)
        {
            if (id != updateDto.Id)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            if (!ModelState.IsValid)
            {
                await PrepareEditViewBagAsync(userId, updateDto.Type);
                return View(updateDto);
            }

            try
            {
                var transaction = await _transactionService.UpdateAsync(updateDto, userId);
                if (transaction == null)
                {
                    return NotFound();
                }

                TempData["SuccessMessage"] = $"Транзакцията '{transaction.Description}' беше обновена успешно!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                await PrepareEditViewBagAsync(userId, updateDto.Type);
                return View(updateDto);
            }
        }

        // GET: Transactions/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            var transaction = await _transactionService.GetByIdAsync(id, userId);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // POST: Transactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            var canDelete = await _transactionService.CanDeleteAsync(id, userId);
            if (!canDelete)
            {
                TempData["ErrorMessage"] = "Не може да изтриете тази транзакция.";
                return RedirectToAction(nameof(Index));
            }

            var success = await _transactionService.DeleteAsync(id, userId);
            if (success)
            {
                TempData["SuccessMessage"] = "Транзакцията беше изтрита успешно!";
            }
            else
            {
                TempData["ErrorMessage"] = "Възникна грешка при изтриването на транзакцията.";
            }

            return RedirectToAction(nameof(Index));
        }

        // API методи за AJAX заявки
        [HttpGet]
        public async Task<IActionResult> GetCategoriesForType(TransactionType type)
        {
            var categories = await _categoryService.GetCategoriesForTypeAsync(type);
            var result = categories.Select(c => new {
                id = c.Id,
                name = c.Name,
                color = c.Color,
                icon = c.Icon
            });
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAccountBalance(int accountId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false, message = "Не сте оторизирани" });

            try
            {
                var balance = await _accountService.GetCurrentBalanceAsync(accountId, userId);
                return Json(new { success = true, balance = balance });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Грешка при получаване на баланса" });
            }
        }

        // Помощни методи
        private async Task PrepareCreateViewBagAsync(string userId, TransactionType type)
        {
            ViewBag.Accounts = await _accountService.GetSummariesAsync(userId);
            ViewBag.Categories = await _categoryService.GetCategoriesForTypeAsync(type);
        }

        private async Task PrepareEditViewBagAsync(string userId, TransactionType type)
        {
            ViewBag.Accounts = await _accountService.GetSummariesAsync(userId);
            ViewBag.Categories = await _categoryService.GetCategoriesForTypeAsync(type);
        }
    }
}