namespace FlowKunevDev.Services.DTOs
{
    public class DailyBudgetInfoDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int RemainingDays { get; set; }
        public decimal TotalBalance { get; set; }
        public decimal PlannedExpenses { get; set; }
        public decimal PlannedIncome { get; set; }
        public decimal AvailableAmount { get; set; }
        public decimal DailyAvailable { get; set; }
        public decimal AverageDailyExpenses { get; set; }
        public decimal RecommendedDailyBudget { get; set; }

        // Помощни свойства за по-лесно четене
        public string PeriodDescription => $"{StartDate.ToString("dd.MM")} - {EndDate.ToString("dd.MM")}";
        public bool HasPlannedTransactions => PlannedExpenses > 0 || PlannedIncome > 0;
        public bool IsAboveAverage => DailyAvailable > AverageDailyExpenses;
        public decimal DifferenceFromAverage => Math.Abs(DailyAvailable - AverageDailyExpenses);
        public string RecommendationText => IsAboveAverage
            ? $"Имате {DifferenceFromAverage:F2} лв. повече от обичайното дневно"
            : DailyAvailable < AverageDailyExpenses
                ? $"Имате {DifferenceFromAverage:F2} лв. по-малко от обичайното дневно"
                : "Дневният бюджет съвпада с обичайните разходи";
    }
}