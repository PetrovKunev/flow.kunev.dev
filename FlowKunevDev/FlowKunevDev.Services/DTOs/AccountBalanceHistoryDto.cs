namespace FlowKunevDev.Services.DTOs
{
    public class AccountBalanceHistoryDto
    {
        public DateTime Date { get; set; }
        public decimal Balance { get; set; }
        public string TransactionDescription { get; set; } = string.Empty;
        public decimal TransactionAmount { get; set; }
    }
}
