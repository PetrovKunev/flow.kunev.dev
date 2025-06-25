using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using FlowKunevDev.Services.Interfaces;
using FlowKunevDev.Services.DTOs;
using FlowKunevDev.Common;
using FlowKunevDev.Data.Models;

namespace FlowKunevDev.Web.Controllers
{
    [Authorize]
    public class PlannedTransactionsController : Controller
    {
        private readonly IPlannedTransactionService _plannedTransactionService;
        private readonly IAccountService _accountService;
        private readonly ICategoryService _categoryService;
        private readonly UserManager<ApplicationUser> _userManager;

        public PlannedTransactionsController(
            IPlannedTransactionService plannedTransactionService,
            IAccountService accountService,
            ICategoryService categoryService,
            UserManager<ApplicationUser> userManager)
        {
            _plannedTransactionService = plannedTransactionService;
            _accountService = accountService;
            _categoryService = categoryService;
            _userManager = userManager;
        }

        // GET: PlannedTransactions
        public async Task<IActionResult> Index(PlannedTransactionFilterDto? filter)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            filter ??= new PlannedTransactionFilterDto();

            try
            {
                var (transactions, totalCount) = await _plannedTransactionService.GetPagedAsync(filter, userId);

                ViewBag.Accounts = await _accountService.GetSummariesAsync(userId);
                ViewBag.Categories = await _categoryService.GetActiveAsync();
                ViewBag.Filter = filter;
                ViewBag.TotalCount = totalCount;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);

                return View(transactions);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Възникна грешка при зареждането на планираните транзакции.";
                return View(new List<PlannedTransactionDto>());
            }
        }

        // GET: PlannedTransactions/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            var planned = await _plannedTransactionService.GetByIdAsync(id, userId);
            if (planned == null)
            {
                return NotFound();
            }

            return View(planned);
        }

        // GET: PlannedTransactions/Create
        public async Task<IActionResult> Create(int? accountId, TransactionType? type)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            var accounts = await _accountService.GetSummariesAsync(userId);
            var categories = await _categoryService.GetActiveAsync();

            if (!accounts.Any())
            {
                TempData["ErrorMessage"] = "Първо трябва да създадете поне една сметка.";
                return RedirectToAction("Create", "Accounts");
            }

            var model = new CreatePlannedTransactionDto
            {
                PlannedDate = DateTime.Now.Date.AddDays(1), // По подразбиране утре
                AccountId = accountId ?? accounts.First().Id,
                Type = type ?? TransactionType.Expense
            };

            ViewBag.Accounts = accounts;
            ViewBag.Categories = categories;
            return View(model);
        }

        // POST: PlannedTransactions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePlannedTransactionDto createDto)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            if (!ModelState.IsValid)
            {
                ViewBag.Accounts = await _accountService.GetSummariesAsync(userId);
                ViewBag.Categories = await _categoryService.GetActiveAsync();
                return View(createDto);
            }

            try
            {
                var planned = await _plannedTransactionService.CreateAsync(createDto, userId);
                TempData["SuccessMessage"] = $"Планираната транзакция '{planned.Description}' беше създадена успешно!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Грешка при създаването: {ex.Message}");
                ViewBag.Accounts = await _accountService.GetSummariesAsync(userId);
                ViewBag.Categories = await _categoryService.GetActiveAsync();
                return View(createDto);
            }
        }

        // GET: PlannedTransactions/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            var planned = await _plannedTransactionService.GetByIdAsync(id, userId);
            if (planned == null)
            {
                return NotFound();
            }

            if (planned.Status != PlannedTransactionStatus.Planned)
            {
                TempData["ErrorMessage"] = "Може да редактирате само планирани транзакции.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var updateDto = new UpdatePlannedTransactionDto
            {
                Id = planned.Id,
                Description = planned.Description,
                PlannedAmount = planned.PlannedAmount,
                PlannedDate = planned.PlannedDate,
                CategoryId = planned.CategoryId,
                AccountId = planned.AccountId,
                Type = planned.Type,
                Notes = planned.Notes,
                IsRecurring = planned.IsRecurring,
                RecurrenceType = planned.RecurrenceType
            };

            ViewBag.Accounts = await _accountService.GetSummariesAsync(userId);
            ViewBag.Categories = await _categoryService.GetActiveAsync();
            return View(updateDto);
        }

        // POST: PlannedTransactions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdatePlannedTransactionDto updateDto)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            if (!ModelState.IsValid)
            {
                ViewBag.Accounts = await _accountService.GetSummariesAsync(userId);
                ViewBag.Categories = await _categoryService.GetActiveAsync();
                return View(updateDto);
            }

            try
            {
                var updated = await _plannedTransactionService.UpdateAsync(updateDto, userId);
                if (updated == null)
                {
                    return NotFound();
                }

                TempData["SuccessMessage"] = "Планираната транзакция беше обновена успешно!";
                return RedirectToAction(nameof(Details), new { id = updateDto.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Грешка при обновяването: {ex.Message}");
                ViewBag.Accounts = await _accountService.GetSummariesAsync(userId);
                ViewBag.Categories = await _categoryService.GetActiveAsync();
                return View(updateDto);
            }
        }

        // GET: PlannedTransactions/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            var planned = await _plannedTransactionService.GetByIdAsync(id, userId);
            if (planned == null)
            {
                return NotFound();
            }

            return View(planned);
        }

        // POST: PlannedTransactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            try
            {
                var success = await _plannedTransactionService.DeleteAsync(id, userId);
                if (success)
                {
                    TempData["SuccessMessage"] = "Планираната транзакция беше изтрита успешно!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Не може да се изтрие планираната транзакция.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Грешка при изтриването: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: PlannedTransactions/Execute/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Execute(int id, decimal? actualAmount = null, DateTime? actualDate = null, string? actualNotes = null)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            try
            {
                var transactionId = await _plannedTransactionService.ExecuteAsync(id, userId, actualAmount, actualDate, actualNotes);
                TempData["SuccessMessage"] = "Планираната транзакция беше изпълнена успешно!";
                return RedirectToAction("Details", "Transactions", new { id = transactionId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Грешка при изпълнението: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // POST: PlannedTransactions/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            try
            {
                var success = await _plannedTransactionService.CancelAsync(id, userId);
                if (success)
                {
                    TempData["SuccessMessage"] = "Планираната транзакция беше отказана.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Не може да се отказва планираната транзакция.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Грешка при отказването: {ex.Message}";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: PlannedTransactions/Upcoming
        public async Task<IActionResult> Upcoming(int days = 30)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            try
            {
                var upcoming = await _plannedTransactionService.GetUpcomingAsync(userId, days);
                ViewBag.Days = days;
                return View(upcoming);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Възникна грешка при зареждането на предстоящите транзакции.";
                return View(new List<PlannedTransactionSummaryDto>());
            }
        }

        // GET: PlannedTransactions/Overdue
        public async Task<IActionResult> Overdue()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            try
            {
                var overdue = await _plannedTransactionService.GetOverdueAsync(userId);
                return View(overdue);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Възникна грешка при зареждането на просрочените транзакции.";
                return View(new List<PlannedTransactionSummaryDto>());
            }
        }

        // GET: PlannedTransactions/Recurring
        public async Task<IActionResult> Recurring()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            try
            {
                var recurring = await _plannedTransactionService.GetRecurringAsync(userId);
                return View(recurring);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Възникна грешка при зареждането на повтарящите се транзакции.";
                return View(new List<PlannedTransactionDto>());
            }
        }

        // API за AJAX заявки
        [HttpGet]
        public async Task<IActionResult> GetUpcomingJson(int days = 7)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false, message = "Неоторизиран достъп" });

            try
            {
                var upcoming = await _plannedTransactionService.GetUpcomingAsync(userId, days);
                return Json(new { success = true, data = upcoming });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetOverdueJson()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false, message = "Неоторизиран достъп" });

            try
            {
                var overdue = await _plannedTransactionService.GetOverdueAsync(userId);
                return Json(new { success = true, data = overdue });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}