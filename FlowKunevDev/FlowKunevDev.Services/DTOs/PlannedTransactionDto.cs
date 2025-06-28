using FlowKunevDev.Common;
namespace FlowKunevDev.Services.DTOs
{ 
    public class PlannedTransactionDto
    {
        public int Id { get; set; }
        public string Description { get; set; } = null!;
        public decimal PlannedAmount { get; set; }
        public DateTime PlannedDate { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public string CategoryColor { get; set; } = "#007bff";
        public string CategoryIcon { get; set; } = "fa fa-folder";
        public int AccountId { get; set; }
        public string AccountName { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public TransactionType Type { get; set; }
        public string? Notes { get; set; }
        public PlannedTransactionStatus Status { get; set; }
        public bool IsRecurring { get; set; }
        public RecurrenceType? RecurrenceType { get; set; }
        public int? ExecutedTransactionId { get; set; }
        public DateTime CreatedDate { get; set; }

        // Computed properties
        public bool IsOverdue => Status == PlannedTransactionStatus.Planned && PlannedDate < TimeHelper.LocalNow.Date;
        public bool IsDueToday => Status == PlannedTransactionStatus.Planned && PlannedDate.Date == TimeHelper.LocalNow.Date;
        public bool IsDueSoon => Status == PlannedTransactionStatus.Planned && PlannedDate.Date <= TimeHelper.LocalNow.AddDays(7).Date && PlannedDate.Date > TimeHelper.LocalNow.Date;
        public int DaysUntilDue => Status == PlannedTransactionStatus.Planned ? (PlannedDate.Date - TimeHelper.LocalNow.Date).Days : 0;
    }
}