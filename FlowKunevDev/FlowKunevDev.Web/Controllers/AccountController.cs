using FlowKunevDev.Data.Models;
using FlowKunevDev.Services.Interfaces;
using FlowKunevDev.Web.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FlowKunevDev.Web.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IAccountService _accountService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IAccountService accountService,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _accountService = accountService;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = model.UserNameOrEmail.Contains('@')
                ? await _userManager.FindByEmailAsync(model.UserNameOrEmail)
                : await _userManager.FindByNameAsync(model.UserNameOrEmail);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Невалидно потребителско име или парола.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("Потребител {UserName} влезе успешно.", user.UserName);
                return RedirectToLocal(model.ReturnUrl);
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty,
                    "Профилът е временно заключен заради твърде много неуспешни опити. Опитайте по-късно.");
                _logger.LogWarning("Профилът на {UserName} беше заключен.", user.UserName);
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Невалидно потребителско име или парола.");
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");

            return View(new RegisterViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, TranslateIdentityError(error));
                return View(model);
            }

            _logger.LogInformation("Нов потребител {UserName} се регистрира.", user.UserName);
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToLocal(model.ReturnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Потребителят излезе.");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied() => View();

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction(nameof(Login));

            return View(new ProfileViewModel
            {
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                StatusMessage = TempData["StatusMessage"] as string
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction(nameof(Login));

            if (!ModelState.IsValid)
                return View(model);

            if (user.UserName != model.UserName)
            {
                var setUserName = await _userManager.SetUserNameAsync(user, model.UserName);
                if (!setUserName.Succeeded)
                {
                    foreach (var error in setUserName.Errors)
                        ModelState.AddModelError(string.Empty, TranslateIdentityError(error));
                    return View(model);
                }
            }

            if (user.Email != model.Email)
            {
                var setEmail = await _userManager.SetEmailAsync(user, model.Email);
                if (!setEmail.Succeeded)
                {
                    foreach (var error in setEmail.Errors)
                        ModelState.AddModelError(string.Empty, TranslateIdentityError(error));
                    return View(model);
                }
            }

            if (user.PhoneNumber != model.PhoneNumber)
            {
                var setPhone = await _userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
                if (!setPhone.Succeeded)
                {
                    foreach (var error in setPhone.Errors)
                        ModelState.AddModelError(string.Empty, TranslateIdentityError(error));
                    return View(model);
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["StatusMessage"] = "Профилът беше обновен успешно.";
            return RedirectToAction(nameof(Profile));
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel
            {
                StatusMessage = TempData["StatusMessage"] as string
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction(nameof(Login));

            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, TranslateIdentityError(error));
                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);
            _logger.LogInformation("Потребителят смени паролата си.");
            TempData["StatusMessage"] = "Паролата беше сменена успешно.";
            return RedirectToAction(nameof(ChangePassword));
        }

        [HttpGet]
        public async Task<IActionResult> SavingsSettings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction(nameof(Login));

            var model = new SavingsSettingsViewModel
            {
                SalaryAccountId = user.SalaryAccountId,
                SavingsAccountId = user.SavingsAccountId,
                MonthlySavingsPercent = user.MonthlySavingsPercent,
                ExpectedMonthlyIncome = user.ExpectedMonthlyIncome,
                StatusMessage = TempData["StatusMessage"] as string
            };
            await PopulateAccountOptionsAsync(model, user.Id);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavingsSettings(SavingsSettingsViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction(nameof(Login));

            if (!ModelState.IsValid)
            {
                await PopulateAccountOptionsAsync(model, user.Id);
                return View(model);
            }

            if (model.SalaryAccountId.HasValue && model.SavingsAccountId.HasValue
                && model.SalaryAccountId == model.SavingsAccountId)
            {
                ModelState.AddModelError(string.Empty,
                    "Сметката за заплата и сметката за спестявания трябва да са различни.");
                await PopulateAccountOptionsAsync(model, user.Id);
                return View(model);
            }

            if (model.SalaryAccountId.HasValue
                && !await _accountService.ExistsAsync(model.SalaryAccountId.Value, user.Id))
            {
                ModelState.AddModelError(string.Empty, "Невалидна сметка за заплата.");
                await PopulateAccountOptionsAsync(model, user.Id);
                return View(model);
            }

            if (model.SavingsAccountId.HasValue
                && !await _accountService.ExistsAsync(model.SavingsAccountId.Value, user.Id))
            {
                ModelState.AddModelError(string.Empty, "Невалидна сметка за спестявания.");
                await PopulateAccountOptionsAsync(model, user.Id);
                return View(model);
            }

            user.SalaryAccountId = model.SalaryAccountId;
            user.SavingsAccountId = model.SavingsAccountId;
            user.MonthlySavingsPercent = model.MonthlySavingsPercent;
            user.ExpectedMonthlyIncome = model.ExpectedMonthlyIncome;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, TranslateIdentityError(error));
                await PopulateAccountOptionsAsync(model, user.Id);
                return View(model);
            }

            TempData["StatusMessage"] = "Настройките за спестяване бяха запазени.";
            return RedirectToAction(nameof(SavingsSettings));
        }

        private async Task PopulateAccountOptionsAsync(SavingsSettingsViewModel model, string userId)
        {
            var accounts = await _accountService.GetSummariesAsync(userId);
            model.AccountOptions = accounts
                .Where(a => a.IsActive)
                .OrderBy(a => a.Name)
                .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Name })
                .ToList();
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Dashboard");
        }

        private static string TranslateIdentityError(IdentityError error) => error.Code switch
        {
            "DuplicateUserName" => "Потребителското име е заето.",
            "DuplicateEmail" => "Този имейл вече е регистриран.",
            "InvalidUserName" => "Потребителското име съдържа невалидни символи.",
            "InvalidEmail" => "Невалиден имейл адрес.",
            "PasswordTooShort" => "Паролата е твърде кратка.",
            "PasswordRequiresDigit" => "Паролата трябва да съдържа поне една цифра.",
            "PasswordRequiresLower" => "Паролата трябва да съдържа малка буква.",
            "PasswordRequiresUpper" => "Паролата трябва да съдържа главна буква.",
            "PasswordRequiresNonAlphanumeric" => "Паролата трябва да съдържа специален символ.",
            "PasswordRequiresUniqueChars" => "Паролата трябва да съдържа повече различни символи.",
            "PasswordMismatch" => "Текущата парола е грешна.",
            _ => error.Description
        };
    }
}
