namespace FlowKunevDev.Services.DTOs
{
    public class SavingsTargetDto
    {
        public bool IsConfigured { get; set; }

        public int? SalaryAccountId { get; set; }
        public string? SalaryAccountName { get; set; }
        public int? SavingsAccountId { get; set; }
        public string? SavingsAccountName { get; set; }

        public decimal Percent { get; set; }
        public decimal ExpectedMonthlyIncome { get; set; }
        public decimal RealizedIncome { get; set; }
        public decimal ForecastTarget { get; set; }
        public decimal Target { get; set; }
        public decimal Transferred { get; set; }

        public decimal Gap => Target - Transferred;
        public decimal PercentComplete => Target > 0 ? Math.Min(100, (Transferred / Target) * 100) : 0;
        public bool IsTargetReached => Target > 0 && Transferred >= Target;
        public bool HasIncomeYet => RealizedIncome > 0;

        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }
}
