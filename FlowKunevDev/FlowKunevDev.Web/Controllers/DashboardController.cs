using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using FlowKunevDev.Services.Interfaces;
using FlowKunevDev.Web.ViewModels;

namespace FlowKunevDev.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly ITransactionService _transactionService;
        private readonly UserManager<IdentityUser> _userManager;

        public DashboardController(
            IAccountService accountService,
            ITransactionService transactionService,
            UserManager<IdentityUser> userManager)
        {
            _accountService = accountService;
            _transactionService = transactionService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            try
            {
                // Получаваме основни данни за таблото
                var accounts = await _accountService.GetSummariesAsync(userId);
                var totalBalance = await _accountService.GetTotalBalanceAsync(userId);
                var recentTransactions = await _transactionService.GetRecentAsync(userId, 5);

                // Получаваме месечни данни
                var currentDate = DateTime.Now;
                var monthlySummary = await _transactionService.GetMonthlySummaryAsync(userId, currentDate.Year, currentDate.Month);

                // Изчисляваме дневно разполагаемите средства
                var dailyAvailable = await _transactionService.GetDailyAvailableAmountAsync(userId);

                var viewModel = new DashboardViewModel
                {
                    TotalBalance = totalBalance,
                    MonthlyIncome = monthlySummary.TotalIncome,
                    MonthlyExpenses = monthlySummary.TotalExpenses,
                    MonthlyBalance = monthlySummary.NetAmount,
                    DailyAvailableAmount = dailyAvailable,
                    AnalysisPeriodStart = new DateTime(currentDate.Year, currentDate.Month, 1),
                    AnalysisPeriodEnd = new DateTime(currentDate.Year, currentDate.Month, DateTime.DaysInMonth(currentDate.Year, currentDate.Month)),

                    // Мапваме данните
                    Accounts = accounts.Select(a => new AccountSummary
                    {
                        Id = a.Id,
                        Name = a.Name,
                        CurrentBalance = a.CurrentBalance,
                        Color = a.Color,
                        Currency = a.Currency,
                        Type = a.Type.ToString(),
                        IsActive = a.IsActive
                    }).ToList(),

                    ExpensesByCategory = monthlySummary.CategoryBreakdown.Select(c => new CategorySummary
                    {
                        CategoryId = c.CategoryId,
                        CategoryName = c.CategoryName,
                        Amount = c.Amount,
                        Color = c.CategoryColor,
                        TransactionCount = c.TransactionCount,
                        Percentage = c.Percentage
                    }).ToList(),

                    RecentTransactions = recentTransactions.Select(t => new FlowKunevDev.Data.Models.Transaction
                    {
                        Id = t.Id,
                        Description = t.Description,
                        Amount = t.Amount,
                        Date = t.Date,
                        Type = t.Type
                    }).ToList()
                };

                return View(viewModel);
            }
            catch (Exception)
            {
                // Логираме грешката и показваме празно табло
                TempData["ErrorMessage"] = "Възникна грешка при зареждането на таблото.";
                return View(new DashboardViewModel());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDailyBudgetInfo(DateTime? fromDate, DateTime? toDate)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            try
            {
                var info = await _transactionService.GetDailyBudgetInfoAsync(userId, fromDate, toDate);
                return Json(new { success = true, data = info });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Възникна грешка при изчислението." });
            }
        }
    }
}