namespace FlowKunevDev.Web.ViewModels
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
    }
}
