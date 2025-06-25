using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using FlowKunevDev.Services.Interfaces;
using FlowKunevDev.Services.DTOs;
using FlowKunevDev.Web.ViewModels;
using FlowKunevDev.Common;
using FlowKunevDev.Data.Models;

namespace FlowKunevDev.Web.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly ITransactionService _transactionService;
        private readonly IAccountService _accountService;
        private readonly ICategoryService _categoryService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReportsController(
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

        // Месечни отчети
        [HttpGet]
        public async Task<IActionResult> Monthly(int? year = null, int? month = null)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            try
            {
                var currentYear = year ?? DateTime.Now.Year;
                var currentMonth = month ?? DateTime.Now.Month;

                var summary = await _transactionService.GetMonthlySummaryAsync(userId, currentYear, currentMonth);
                var accounts = await _accountService.GetSummariesAsync(userId);

                var viewModel = new MonthlyReportViewModel
                {
                    Year = currentYear,
                    Month = currentMonth,
                    Summary = summary,
                    Accounts = accounts.ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Monthly: {ex.Message}");
                TempData["ErrorMessage"] = "Възникна грешка при зареждането на месечния отчет.";
                return View(new MonthlyReportViewModel());
            }
        }

        // Отчети по категории
        [HttpGet]
        public async Task<IActionResult> Categories(DateTime? startDate = null, DateTime? endDate = null, TransactionType? type = null)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            try
            {
                var start = startDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var end = endDate ?? DateTime.Now.Date;

                var categorySummary = await _transactionService.GetCategorySummaryAsync(userId, start, end, type);
                var categories = await _categoryService.GetAllAsync();

                var viewModel = new CategoryReportViewModel
                {
                    StartDate = start,
                    EndDate = end,
                    Type = type,
                    CategorySummaries = categorySummary.ToList(),
                    AllCategories = categories.ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Categories: {ex.Message}");
                TempData["ErrorMessage"] = "Възникна грешка при зареждането на отчета по категории.";
                return View(new CategoryReportViewModel());
            }
        }

        // Тенденции
        [HttpGet]
        public async Task<IActionResult> Trends(int months = 12)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            try
            {
                var monthlyComparison = await _transactionService.GetMonthlyComparisonAsync(userId, months);
                var yearlyData = await _transactionService.GetYearlySummaryAsync(userId, DateTime.Now.Year);

                var viewModel = new TrendsReportViewModel
                {
                    MonthlyComparison = monthlyComparison,
                    YearlyData = yearlyData.ToList(),
                    SelectedMonths = months
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Trends: {ex.Message}");
                TempData["ErrorMessage"] = "Възникна грешка при зареждането на тенденциите.";
                return View(new TrendsReportViewModel());
            }
        }

        // Сравнение на разходи за периоди
        [HttpGet]
        public async Task<IActionResult> PeriodComparison(ComparisonType type = ComparisonType.Monthly, DateTime? baseDate = null, int periodsCount = 6)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account", new { area = "Identity" });

            try
            {
                var selectedDate = baseDate ?? DateTime.Now;

                // Получаваме сравнението по периоди
                var periods = await _transactionService.GetPeriodComparisonAsync(userId, type, selectedDate, periodsCount);

                // Ако имаме поне 2 периода, правим сравнение по категории
                var categoryComparisons = new List<CategoryComparison>();
                if (periods.Count >= 2)
                {
                    var currentPeriod = periods.First();
                    var previousPeriod = periods.Skip(1).First();

                    categoryComparisons = await _transactionService.GetCategoryComparisonAsync(
                        userId,
                        currentPeriod.StartDate, currentPeriod.EndDate,
                        previousPeriod.StartDate, previousPeriod.EndDate
                    );
                }

                // Изчисляваме обобщението
                var summary = await _transactionService.GetComparisonSummaryAsync(userId, periods, categoryComparisons);

                var viewModel = new PeriodComparisonViewModel
                {
                    Periods = periods,
                    CategoryComparisons = categoryComparisons,
                    SelectedType = type,
                    SelectedDate = selectedDate,
                    PeriodsCount = periodsCount,
                    Summary = summary
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PeriodComparison: {ex.Message}");
                TempData["ErrorMessage"] = "Възникна грешка при зареждането на сравнението.";
                return View(new PeriodComparisonViewModel());
            }
        }

        // AJAX методи за динамично зареждане на данни

        [HttpGet]
        public async Task<IActionResult> GetMonthlyData(int year, int month)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var summary = await _transactionService.GetMonthlySummaryAsync(userId, year, month);
                return Json(new { success = true, data = summary });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Възникна грешка при зареждането на данните." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCategoryData(DateTime startDate, DateTime endDate, TransactionType? type)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var categorySummary = await _transactionService.GetCategorySummaryAsync(userId, startDate, endDate, type);
                return Json(new { success = true, data = categorySummary });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Възникна грешка при зареждането на данните." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetComparisonData(ComparisonType type, DateTime baseDate, int periodsCount)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var periods = await _transactionService.GetPeriodComparisonAsync(userId, type, baseDate, periodsCount);

                var data = periods.Select(p => new {
                    periodName = p.PeriodName,
                    expenses = p.TotalExpenses,
                    income = p.TotalIncome,
                    balance = p.Balance,
                    changeFromPrevious = p.ChangeFromPrevious,
                    percentageChange = p.PercentageChange,
                    isCurrentPeriod = p.IsCurrentPeriod
                }).ToList();

                return Json(new { success = true, data });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Възникна грешка при зареждането на данните." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSpendingAnalysis(DateTime startDate, DateTime endDate)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var analysis = await _transactionService.GetSpendingAnalysisAsync(userId, startDate, endDate);
                return Json(new { success = true, data = analysis });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Възникна грешка при анализа." });
            }
        }
    }
}