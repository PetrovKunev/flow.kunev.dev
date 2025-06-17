using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowKunevDev.Services.DTOs
{
    public class CategorySummaryDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public string CategoryColor { get; set; } = "#007bff";
        public decimal Amount { get; set; }
        public int TransactionCount { get; set; }
        public decimal Percentage { get; set; }
    }
}
