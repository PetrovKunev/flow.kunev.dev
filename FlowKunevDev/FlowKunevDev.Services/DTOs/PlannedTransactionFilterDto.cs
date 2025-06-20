using FlowKunevDev.Common;

namespace FlowKunevDev.Services.DTOs
{
    public class PlannedTransactionFilterDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? CategoryId { get; set; }
        public int? AccountId { get; set; }
        public TransactionType? Type { get; set; }
        public PlannedTransactionStatus? Status { get; set; }
        public bool? IsRecurring { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
