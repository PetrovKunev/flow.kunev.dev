using FlowKunevDev.Data.Models;
using FlowKunevDev.Services.DTOs;
using FlowKunevDev.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FlowKunevDev.Web.Controllers
{
    [Authorize]
    public class AccountTransfersController : Controller
    {
        private readonly IAccountTransferService _transferService;
        private readonly IAccountService _accountService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountTransfersController(
            IAccountTransferService transferService,
            IAccountService accountService,
            UserManager<ApplicationUser> userManager)
        {
            _transferService = transferService;
            _accountService = accountService;
            _userManager = userManager;
        }

        // GET: AccountTransfers
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            try
            {
                var transfers = await _transferService.GetAllAsync(userId);
                return View(transfers);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Възникна грешка при зареждането на трансферите.";
                return View(new List<AccountTransferDto>());
            }
        }

        // GET: AccountTransfers/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            var transfer = await _transferService.GetByIdAsync(id, userId);
            if (transfer == null)
            {
                return NotFound();
            }

            return View(transfer);
        }

        // GET: AccountTransfers/Create
        public async Task<IActionResult> Create(int? fromAccountId, int? toAccountId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            var accounts = await _accountService.GetSummariesAsync(userId);

            if (!accounts.Any() || accounts.Count() < 2)
            {
                TempData["ErrorMessage"] = "Трябва да имате поне 2 активни сметки за да правите трансфери.";
                return RedirectToAction("Index", "Accounts");
            }

            var createDto = new CreateAccountTransferDto
            {
                Date = DateTime.Now
            };

            // Ако са подадени предварително избрани сметки
            if (fromAccountId.HasValue)
                createDto.FromAccountId = fromAccountId.Value;

            if (toAccountId.HasValue)
                createDto.ToAccountId = toAccountId.Value;

            ViewBag.Accounts = accounts;
            return View(createDto);
        }

        // POST: AccountTransfers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAccountTransferDto createDto)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            if (!ModelState.IsValid)
            {
                ViewBag.Accounts = await _accountService.GetSummariesAsync(userId);
                return View(createDto);
            }

            try
            {
                var transfer = await _transferService.CreateAsync(createDto, userId);
                TempData["SuccessMessage"] = $"Трансферът на стойност {transfer.Amount:F2} лв. от {transfer.FromAccountName} към {transfer.ToAccountName} беше създаден успешно!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Accounts = await _accountService.GetSummariesAsync(userId);
                return View(createDto);
            }
        }

        // GET: AccountTransfers/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            var transfer = await _transferService.GetByIdAsync(id, userId);
            if (transfer == null)
            {
                return NotFound();
            }

            var updateDto = new UpdateAccountTransferDto
            {
                Id = transfer.Id,
                Amount = transfer.Amount,
                FromAccountId = transfer.FromAccountId,
                ToAccountId = transfer.ToAccountId,
                Date = transfer.Date,
                Description = transfer.Description,
                Notes = transfer.Notes
            };

            ViewBag.Accounts = await _accountService.GetSummariesAsync(userId);
            return View(updateDto);
        }

        // POST: AccountTransfers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateAccountTransferDto updateDto)
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
                ViewBag.Accounts = await _accountService.GetSummariesAsync(userId);
                return View(updateDto);
            }

            try
            {
                var transfer = await _transferService.UpdateAsync(updateDto, userId);
                if (transfer == null)
                {
                    return NotFound();
                }

                TempData["SuccessMessage"] = "Трансферът беше обновен успешно!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Accounts = await _accountService.GetSummariesAsync(userId);
                return View(updateDto);
            }
        }

        // GET: AccountTransfers/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            var transfer = await _transferService.GetByIdAsync(id, userId);
            if (transfer == null)
            {
                return NotFound();
            }

            return View(transfer);
        }

        // POST: AccountTransfers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            var canDelete = await _transferService.CanDeleteAsync(id, userId);
            if (!canDelete)
            {
                TempData["ErrorMessage"] = "Не може да изтриете този трансфер.";
                return RedirectToAction(nameof(Index));
            }

            var success = await _transferService.DeleteAsync(id, userId);
            if (success)
            {
                TempData["SuccessMessage"] = "Трансферът беше изтрит успешно!";
            }
            else
            {
                TempData["ErrorMessage"] = "Възникна грешка при изтриването на трансфера.";
            }

            return RedirectToAction(nameof(Index));
        }

        // API method за получаване на баланса на сметка
        [HttpGet]
        public async Task<IActionResult> GetAccountBalance(int accountId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false, message = "Не сте оторизирани" });

            try
            {
                var balance = await _accountService.GetCurrentBalanceAsync(accountId, userId);
                var account = await _accountService.GetByIdAsync(accountId, userId);

                return Json(new
                {
                    success = true,
                    balance = balance,
                    currency = account?.Currency ?? "BGN",
                    name = account?.Name ?? ""
                });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Грешка при получаване на баланса" });
            }
        }

        // API method за статистики
        [HttpGet]
        public async Task<IActionResult> GetTransferStats()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false, message = "Не сте оторизирани" });

            try
            {
                var stats = await _transferService.GetTransferStatsAsync(userId);
                return Json(new { success = true, stats = stats });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Грешка при получаване на статистиките" });
            }
        }
    }
}