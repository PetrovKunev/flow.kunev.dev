namespace FlowKunevDev.Services.DTOs
{
    public class ComparisonSummary
    {
        public decimal AverageMonthlyExpenses { get; set; }
        public decimal HighestMonthlyExpenses { get; set; }
        public decimal LowestMonthlyExpenses { get; set; }
        public string HighestExpenseMonth { get; set; } = string.Empty;
        public string LowestExpenseMonth { get; set; } = string.Empty;
        public decimal TotalExpensesAllPeriods { get; set; }
        public decimal ExpensesTrend { get; set; } // positive = увеличаване, negative = намаляване
        public List<string> TopGrowingCategories { get; set; } = [];
        public List<string> TopDecreasingCategories { get; set; } = [];
    }
}
