using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using FlowKunevDev.Services.Interfaces;
using FlowKunevDev.Services.DTOs;

namespace FlowKunevDev.Web.Controllers
{
    [Authorize]
    public class AccountsController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly UserManager<IdentityUser> _userManager;

        public AccountsController(IAccountService accountService, UserManager<IdentityUser> userManager)
        {
            _accountService = accountService;
            _userManager = userManager;
        }

        // GET: Accounts
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            var accounts = await _accountService.GetAllAsync(userId);
            return View(accounts);
        }

        // GET: Accounts/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            var account = await _accountService.GetByIdAsync(id, userId);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        // GET: Accounts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Accounts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAccountDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return View(createDto);
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            try
            {
                var account = await _accountService.CreateAsync(createDto, userId);
                TempData["SuccessMessage"] = $"Сметката '{account.Name}' беше създадена успешно!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(createDto);
            }
        }

        // GET: Accounts/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            var account = await _accountService.GetByIdAsync(id, userId);
            if (account == null)
            {
                return NotFound();
            }

            var updateDto = new UpdateAccountDto
            {
                Id = account.Id,
                Name = account.Name,
                Description = account.Description,
                Type = account.Type,
                Currency = account.Currency,
                Color = account.Color,
                IsActive = account.IsActive
            };

            return View(updateDto);
        }

        // POST: Accounts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateAccountDto updateDto)
        {
            if (id != updateDto.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(updateDto);
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            try
            {
                var account = await _accountService.UpdateAsync(updateDto, userId);
                if (account == null)
                {
                    return NotFound();
                }

                TempData["SuccessMessage"] = $"Сметката '{account.Name}' беше обновена успешно!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(updateDto);
            }
        }

        // GET: Accounts/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            var account = await _accountService.GetByIdAsync(id, userId);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        // POST: Accounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            var canDelete = await _accountService.CanDeleteAsync(id, userId);
            if (!canDelete)
            {
                TempData["ErrorMessage"] = "Не може да изтриете сметка, която има транзакции или трансфери.";
                return RedirectToAction(nameof(Index));
            }

            var success = await _accountService.DeleteAsync(id, userId);
            if (success)
            {
                TempData["SuccessMessage"] = "Сметката беше деактивирана успешно!";
            }
            else
            {
                TempData["ErrorMessage"] = "Възникна грешка при деактивирането на сметката.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Accounts/ToggleActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var userId = _userManager.GetUserId(User);
            var success = await _accountService.ToggleActiveAsync(id, userId!);

            if (success)
            {
                TempData["SuccessMessage"] = "Статусът на сметката е променен успешно!";
            }
            else
            {
                TempData["ErrorMessage"] = "Възникна грешка при промяната на статуса.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: API endpoint за баланс
        [HttpGet]
        public async Task<IActionResult> GetBalance(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false, message = "Не сте оторизирани" });

            var balance = await _accountService.GetCurrentBalanceAsync(id, userId);
            return Json(new { success = true, balance = balance });
        }


        // API метод за получаване на статистики
        [HttpGet]
        public async Task<IActionResult> GetStats(int id)
        {
            var userId = _userManager.GetUserId(User);

            try
            {
                var stats = await _accountService.GetAccountStatsAsync(id, userId!);
                return Json(new { success = true, stats = stats });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}