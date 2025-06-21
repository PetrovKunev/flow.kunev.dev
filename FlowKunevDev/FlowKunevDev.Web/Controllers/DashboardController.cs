using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using FlowKunevDev.Services.Interfaces;
using FlowKunevDev.Web.ViewModels;
using FlowKunevDev.Services.DTOs;

namespace FlowKunevDev.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly ITransactionService _transactionService;
        private readonly IPlannedTransactionService _plannedTransactionService;
        private readonly UserManager<IdentityUser> _userManager;

        public DashboardController(
            IAccountService accountService,
            ITransactionService transactionService,
            IPlannedTransactionService plannedTransactionService,
            UserManager<IdentityUser> userManager)
        {
            _accountService = accountService;
            _transactionService = transactionService;
            _plannedTransactionService = plannedTransactionService;
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

                // Изчисляваме дневно разполагаемите средства (вече отчита планираните транзакции)
                var dailyBudgetInfo = await _transactionService.GetDailyBudgetInfoAsync(userId);

                // Получаваме планирани транзакции
                var startOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                // Предстоящи планирани транзакции (следващите 7 дни)
                var upcoming = await _plannedTransactionService.GetUpcomingAsync(userId, 7);

                // Просрочени планирани транзакции
                var overdue = await _plannedTransactionService.GetOverdueAsync(userId);

                // Общо планирани разходи и приходи за месеца
                var totalPlannedExpenses = await _plannedTransactionService.GetTotalPlannedExpensesAsync(userId, startOfMonth, endOfMonth);
                var plannedIncome = await _plannedTransactionService.GetTotalPlannedIncomeAsync(userId, startOfMonth, endOfMonth);

                var viewModel = new DashboardViewModel
                {
                    // Основни данни
                    TotalBalance = totalBalance,
                    MonthlyIncome = monthlySummary.TotalIncome,
                    MonthlyExpenses = monthlySummary.TotalExpenses,
                    MonthlyBalance = monthlySummary.NetAmount,

                    // Дневни разполагаеми средства (обновена логика с планирани)
                    DailyAvailableAmount = dailyBudgetInfo.DailyAvailable,
                    AverageDailyExpenses = dailyBudgetInfo.AverageDailyExpenses,
                    RemainingDays = dailyBudgetInfo.RemainingDays,

                    // Планирани транзакции данни
                    TotalPlannedExpenses = totalPlannedExpenses,
                    PlannedIncome = plannedIncome,
                    BalanceAfterPlanned = totalBalance - totalPlannedExpenses + plannedIncome,

                    // Планирани транзакции списъци
                    UpcomingPlannedTransactions = upcoming.Take(5).ToList(),
                    OverduePlannedTransactions = overdue.Take(5).ToList(),

                    // Периоди за анализ
                    AnalysisPeriodStart = startOfMonth,
                    AnalysisPeriodEnd = endOfMonth,

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
            catch (Exception ex)
            {
                
                Console.WriteLine($"Error: {ex.Message}");
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

        // Нови API endpoints за планирани транзакции
        [HttpGet]
        public async Task<IActionResult> GetPlannedTransactionsOverview()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            try
            {
                var upcoming = await _plannedTransactionService.GetUpcomingAsync(userId, 7);
                var overdue = await _plannedTransactionService.GetOverdueAsync(userId);
                var stats = await _plannedTransactionService.GetPlannedTransactionStatsAsync(userId);

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        upcoming = upcoming.Take(5),
                        overdue = overdue.Take(5),
                        stats = stats
                    }
                });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Възникна грешка при зареждането на планираните транзакции." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> QuickExecutePlanned(int id, decimal? actualAmount = null, DateTime? actualDate = null, string? actualNotes = null)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            try
            {
                var transactionId = await _plannedTransactionService.ExecuteAsync(id, userId, actualAmount, actualDate, actualNotes);
                return Json(new
                {
                    success = true,
                    message = "Планираната транзакция беше изпълнена успешно!",
                    transactionId = transactionId
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Грешка при изпълнението: {ex.Message}" });
            }
        }
    }
}