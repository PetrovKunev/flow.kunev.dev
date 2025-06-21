namespace FlowKunevDev.Services.DTOs
{
    public class DailyBudgetCalculationRequest
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public List<int>? SelectedAccountIds { get; set; }
        public bool IncludeAllAccounts { get; set; } = true;
    }
}
