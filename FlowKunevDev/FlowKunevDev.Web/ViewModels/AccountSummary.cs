namespace FlowKunevDev.Web.ViewModels
{
    public class AccountSummary
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal CurrentBalance { get; set; }
        public string Color { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
