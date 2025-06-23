namespace FlowKunevDev.Services.DTOs
{
    public class PeriodComparison
    {
        public string PeriodName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal Balance { get; set; }
        public decimal ChangeFromPrevious { get; set; }
        public decimal PercentageChange { get; set; }
        public bool IsCurrentPeriod { get; set; }
        public string TrendIcon => PercentageChange > 5 ? "fa-arrow-up text-danger" :
                                  PercentageChange < -5 ? "fa-arrow-down text-success" :
                                  "fa-minus text-warning";
        public string TrendText => PercentageChange > 5 ? "Увеличение" :
                                  PercentageChange < -5 ? "Намаление" :
                                  "Стабилно";
    }
}
