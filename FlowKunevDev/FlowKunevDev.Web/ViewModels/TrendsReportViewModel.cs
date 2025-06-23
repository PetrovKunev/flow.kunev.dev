using FlowKunevDev.Services.DTOs;

namespace FlowKunevDev.Web.ViewModels
{
    public class TrendsReportViewModel
    {
        public Dictionary<string, decimal> MonthlyComparison { get; set; } = new();
        public List<MonthlyTransactionSummaryDto> YearlyData { get; set; } = new();
        public int SelectedMonths { get; set; }
    }
}
