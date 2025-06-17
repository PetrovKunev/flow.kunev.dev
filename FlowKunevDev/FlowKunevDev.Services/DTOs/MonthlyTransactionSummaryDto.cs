namespace FlowKunevDev.Services.DTOs
{
    public class MonthlyTransactionSummaryDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = null!;
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetAmount { get; set; }
        public int TransactionCount { get; set; }
        public List<CategorySummaryDto> CategoryBreakdown { get; set; } = new();
    }
}
