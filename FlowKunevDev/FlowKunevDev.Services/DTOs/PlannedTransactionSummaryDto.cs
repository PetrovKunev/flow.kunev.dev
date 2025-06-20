using FlowKunevDev.Common;

namespace FlowKunevDev.Services.DTOs
{
    public class PlannedTransactionSummaryDto
    {
        public int Id { get; set; }
        public string Description { get; set; } = null!;
        public decimal PlannedAmount { get; set; }
        public DateTime PlannedDate { get; set; }
        public string CategoryName { get; set; } = null!;
        public string CategoryColor { get; set; } = "#007bff";
        public string AccountName { get; set; } = null!;
        public TransactionType Type { get; set; }
        public PlannedTransactionStatus Status { get; set; }
        public bool IsOverdue { get; set; }
        public bool IsDueToday { get; set; }
        public int DaysUntilDue { get; set; }
    }
}
