using FlowKunevDev.Common;
using FlowKunevDev.Services.DTOs;

namespace FlowKunevDev.Web.ViewModels
{
    public class CategoryReportViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TransactionType? Type { get; set; }
        public List<CategorySummaryDto> CategorySummaries { get; set; } = new();
        public List<FlowKunevDev.Data.Models.Category> AllCategories { get; set; } = new();
    }
}
