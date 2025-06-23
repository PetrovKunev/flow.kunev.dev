namespace FlowKunevDev.Services.DTOs
{
    public class CategoryComparison
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryColor { get; set; } = "#007bff";
        public decimal CurrentPeriodAmount { get; set; }
        public decimal PreviousPeriodAmount { get; set; }
        public decimal Change { get; set; }
        public decimal PercentageChange { get; set; }
        public bool HasSignificantChange => Math.Abs(PercentageChange) > 10;
        public string ChangeIcon => PercentageChange > 0 ? "fa-arrow-up text-danger" :
                                   PercentageChange < 0 ? "fa-arrow-down text-success" :
                                   "fa-minus text-muted";
    }
}
