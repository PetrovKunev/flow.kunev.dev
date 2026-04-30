#nullable disable

using System.ComponentModel.DataAnnotations;
using FlowKunevDev.Data.Models;
using FlowKunevDev.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FlowKunevDev.Web.Areas.Identity.Pages.Account.Manage
{
    public class SavingsSettingsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAccountService _accountService;

        public SavingsSettingsModel(UserManager<ApplicationUser> userManager, IAccountService accountService)
        {
            _userManager = userManager;
            _accountService = accountService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public List<SelectListItem> AccountOptions { get; set; } = new();

        [TempData]
        public string StatusMessage { get; set; }

        public class InputModel
        {
            [Display(Name = "Сметка за заплата")]
            public int? SalaryAccountId { get; set; }

            [Display(Name = "Сметка за спестявания")]
            public int? SavingsAccountId { get; set; }

            [Range(0, 100, ErrorMessage = "Процентът трябва да е между 0 и 100.")]
            [Display(Name = "Месечен процент за спестяване (%)")]
            public decimal? MonthlySavingsPercent { get; set; }

            [Range(0, double.MaxValue, ErrorMessage = "Очакваният приход не може да е отрицателен.")]
            [Display(Name = "Очакван месечен приход (лв.)")]
            public decimal? ExpectedMonthlyIncome { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound($"Не може да се зареди потребителят с ID '{_userManager.GetUserId(User)}'.");

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound($"Не може да се зареди потребителят с ID '{_userManager.GetUserId(User)}'.");

            if (!ModelState.IsValid)
            {
                await LoadAccountsAsync(user.Id);
                return Page();
            }

            if (Input.SalaryAccountId.HasValue && Input.SavingsAccountId.HasValue
                && Input.SalaryAccountId == Input.SavingsAccountId)
            {
                ModelState.AddModelError(string.Empty, "Сметката за заплата и сметката за спестявания трябва да са различни.");
                await LoadAccountsAsync(user.Id);
                return Page();
            }

            if (Input.SalaryAccountId.HasValue
                && !await _accountService.ExistsAsync(Input.SalaryAccountId.Value, user.Id))
            {
                ModelState.AddModelError(string.Empty, "Невалидна сметка за заплата.");
                await LoadAccountsAsync(user.Id);
                return Page();
            }

            if (Input.SavingsAccountId.HasValue
                && !await _accountService.ExistsAsync(Input.SavingsAccountId.Value, user.Id))
            {
                ModelState.AddModelError(string.Empty, "Невалидна сметка за спестявания.");
                await LoadAccountsAsync(user.Id);
                return Page();
            }

            user.SalaryAccountId = Input.SalaryAccountId;
            user.SavingsAccountId = Input.SavingsAccountId;
            user.MonthlySavingsPercent = Input.MonthlySavingsPercent;
            user.ExpectedMonthlyIncome = Input.ExpectedMonthlyIncome;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                await LoadAccountsAsync(user.Id);
                return Page();
            }

            StatusMessage = "Настройките за спестяване бяха запазени.";
            return RedirectToPage();
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            Input = new InputModel
            {
                SalaryAccountId = user.SalaryAccountId,
                SavingsAccountId = user.SavingsAccountId,
                MonthlySavingsPercent = user.MonthlySavingsPercent,
                ExpectedMonthlyIncome = user.ExpectedMonthlyIncome
            };
            await LoadAccountsAsync(user.Id);
        }

        private async Task LoadAccountsAsync(string userId)
        {
            var accounts = await _accountService.GetSummariesAsync(userId);
            AccountOptions = accounts
                .Where(a => a.IsActive)
                .OrderBy(a => a.Name)
                .Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.Name
                })
                .ToList();
        }
    }
}
