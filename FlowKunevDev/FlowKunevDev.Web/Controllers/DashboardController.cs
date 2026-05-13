using FlowKunevDev.Common;
using FlowKunevDev.Data.Models;
using FlowKunevDev.Services.DTOs;
using FlowKunevDev.Services.Interfaces;
using FlowKunevDev.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FlowKunevDev.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly ITransactionService _transactionService;
        private readonly IPlannedTransactionService _plannedTransactionService;
        private readonly ISavingsTargetService _savingsTargetService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(
            IAccountService accountService,
            ITransactionService transactionService,
            IPlannedTransactionService plannedTransactionService,
            ISavingsTargetService savingsTargetService,
            UserManager<ApplicationUser> userManager)
        {
            _accountService = accountService;
            _transactionService = transactionService;
            _plannedTransactionService = plannedTransactionService;
            _savingsTargetService = savingsTargetService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var userId = user.Id;

            try
            {
                // Получаваме основни данни за таблото
                var accounts = await _accountService.GetSummariesAsync(userId);
                var totalBalance = await _accountService.GetTotalBalanceAsync(userId);
                var recentTransactions = await _transactionService.GetRecentAsync(userId, 5);

                // Получаваме месечни данни
                var currentDate = TimeHelper.LocalNow;
                var monthlySummary = await _transactionService.GetMonthlySummaryAsync(userId, currentDate.Year, currentDate.Month);

                // Прилагаме запазените филтри за дневния бюджет (ако има такива).
                // Невалидни запазени Id-та (изтрита сметка) се игнорират автоматично от service-а.
                var savedAccountIds = ParseSavedAccountIds(user.DailyBudgetAccountIds);
                var dailyBudgetInfo = await _transactionService.GetDailyBudgetInfoAsync(
                    userId,
                    user.DailyBudgetFromDate,
                    user.DailyBudgetToDate,
                    savedAccountIds.Count > 0 ? savedAccountIds : null);

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

                // Цел за спестяване (текущ месец)
                var savingsTarget = await _savingsTargetService.GetCurrentMonthTargetAsync(userId);

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

                    // Запазени филтри за дневния бюджет (за пре-попълване на модала)
                    SavedDailyBudgetFromDate = user.DailyBudgetFromDate,
                    SavedDailyBudgetToDate = user.DailyBudgetToDate,
                    SavedDailyBudgetAccountIds = savedAccountIds,

                    // Цел за спестяване
                    SavingsTarget = savingsTarget,

                    // Мапваме данните
                    Accounts = accounts.Select(a => new AccountSummary
                    {
                        Id = a.Id,
                        Name = a.Name,
                        CurrentBalance = a.CurrentBalance,
                        Color = a.Color,
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
        public async Task<IActionResult> GetDailyBudgetInfo(DateTime? fromDate, DateTime? toDate, string? accountIds = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            try
            {
                List<int>? selectedAccountIds = null;

                // Парсираме избраните сметки ако са подадени
                if (!string.IsNullOrEmpty(accountIds))
                {
                    var accountIdStrings = accountIds.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    selectedAccountIds = accountIdStrings
                        .Where(id => int.TryParse(id, out _))
                        .Select(int.Parse)
                        .ToList();
                }

                var info = await _transactionService.GetDailyBudgetInfoAsync(user.Id, fromDate, toDate, selectedAccountIds);

                // Запазваме филтрите, за да оцелеят навигация / logout / refresh.
                await PersistDailyBudgetPreferencesAsync(user, fromDate, toDate, selectedAccountIds);

                return Json(new { success = true, data = info });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Възникна грешка при изчислението." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetDailyBudgetInfoWithAccounts([FromBody] DailyBudgetCalculationRequest request)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            try
            {
                var info = await _transactionService.GetDailyBudgetInfoWithAccountsAsync(user.Id, request);

                // "Всички сметки" => запазваме null; "Избрани" => запазваме конкретните Id-та.
                var idsToPersist = request.IncludeAllAccounts ? null : request.SelectedAccountIds;
                await PersistDailyBudgetPreferencesAsync(user, request.FromDate, request.ToDate, idsToPersist);

                return Json(new { success = true, data = info });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Възникна грешка при изчислението.", error = ex.Message });
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

        private static List<int> ParseSavedAccountIds(string? csv)
        {
            if (string.IsNullOrWhiteSpace(csv)) return new List<int>();
            return csv.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s.Trim(), out var id) ? id : (int?)null)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToList();
        }

        private async Task PersistDailyBudgetPreferencesAsync(
            ApplicationUser user, DateTime? fromDate, DateTime? toDate, List<int>? accountIds)
        {
            user.DailyBudgetFromDate = fromDate;
            user.DailyBudgetToDate = toDate;
            user.DailyBudgetAccountIds = (accountIds == null || accountIds.Count == 0)
                ? null
                : string.Join(",", accountIds);

            await _userManager.UpdateAsync(user);
        }
    }
}