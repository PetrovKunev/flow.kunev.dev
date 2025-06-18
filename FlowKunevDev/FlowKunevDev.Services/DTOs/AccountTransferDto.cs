namespace FlowKunevDev.Services.DTOs
{
    public class AccountTransferDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public int FromAccountId { get; set; }
        public string FromAccountName { get; set; } = string.Empty;
        public int ToAccountId { get; set; }
        public string ToAccountName { get; set; } = string.Empty;
    }
}
