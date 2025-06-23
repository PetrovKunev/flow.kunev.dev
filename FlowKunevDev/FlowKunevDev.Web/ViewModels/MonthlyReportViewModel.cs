using FlowKunevDev.Services.DTOs;

namespace FlowKunevDev.Web.ViewModels
{
    public class MonthlyReportViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public MonthlyTransactionSummaryDto Summary { get; set; } = new();
        public List<AccountSummaryDto> Accounts { get; set; } = new();
    }
}
