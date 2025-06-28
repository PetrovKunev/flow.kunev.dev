using FlowKunevDev.Common;
using FlowKunevDev.Services.DTOs;

namespace FlowKunevDev.Web.ViewModels
{
    public class PeriodComparisonViewModel
    {
        public List<PeriodComparison> Periods { get; set; } = [];
        public List<CategoryComparison> CategoryComparisons { get; set; } = [];
        public ComparisonType SelectedType { get; set; } = ComparisonType.Monthly;
        public DateTime SelectedDate { get; set; } = TimeHelper.LocalNow;
        public int PeriodsCount { get; set; } = 6;
        public ComparisonSummary Summary { get; set; } = new ComparisonSummary();
    }
}
