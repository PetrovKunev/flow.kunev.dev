namespace FlowKunevDev.Web.ViewModels
{
    public class CategorySummary
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Color { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public int TransactionCount { get; set; }
        public decimal Percentage { get; set; }
    }
}
