using System.ComponentModel.DataAnnotations;
using FlowKunevDev.Data.Models;

namespace FlowKunevDev.Web.ViewModels
{
    public class DashboardViewModel
    {        
            // Обща информация
            [Display(Name = "Общ баланс")]
            public decimal TotalBalance { get; set; }

            [Display(Name = "Общи приходи")]
            public decimal TotalIncome { get; set; }

            [Display(Name = "Общи разходи")]
            public decimal TotalExpenses { get; set; }

            [Display(Name = "Месечни приходи")]
            public decimal MonthlyIncome { get; set; }

            [Display(Name = "Месечни разходи")]
            public decimal MonthlyExpenses { get; set; }

            [Display(Name = "Месечен баланс")]
            public decimal MonthlyBalance { get; set; }

            // Планирани разходи
            [Display(Name = "Общо планирани разходи")]
            public decimal TotalPlannedExpenses { get; set; }

            [Display(Name = "Баланс след планирани")]
            public decimal BalanceAfterPlanned { get; set; }

            [Display(Name = "Дневно разполагаеми средства")]
            public decimal DailyAvailableAmount { get; set; }

            // Период за анализ
            [Display(Name = "Начало на период")]
            public DateTime AnalysisPeriodStart { get; set; }

            [Display(Name = "Край на период")]
            public DateTime AnalysisPeriodEnd { get; set; }

            // Колекции данни
            public List<AccountSummary> Accounts { get; set; } = new List<AccountSummary>();
            public List<CategorySummary> ExpensesByCategory { get; set; } = new List<CategorySummary>();
            public List<CategorySummary> IncomeByCategory { get; set; } = new List<CategorySummary>();
            public List<PlannedTransaction> UpcomingPlannedTransactions { get; set; } = new List<PlannedTransaction>();
            public List<Transaction> RecentTransactions { get; set; } = new List<Transaction>();
            public List<PeriodComparison> PeriodComparisons { get; set; } = new List<PeriodComparison>();
            public List<Budget> ActiveBudgets { get; set; } = new List<Budget>();
    }

    
}
