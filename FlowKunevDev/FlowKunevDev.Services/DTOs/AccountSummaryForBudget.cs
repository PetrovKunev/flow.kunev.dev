namespace FlowKunevDev.Services.DTOs
{
    public class AccountSummaryForBudget
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal CurrentBalance { get; set; }
        public string Color { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public bool IsIncluded { get; set; }
    }
}
