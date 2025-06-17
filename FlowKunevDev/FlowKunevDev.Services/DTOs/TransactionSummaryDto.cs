using FlowKunevDev.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowKunevDev.Services.DTOs
{
    public class TransactionSummaryDto
    {
        public int Id { get; set; }
        public string Description { get; set; } = null!;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string CategoryName { get; set; } = null!;
        public string CategoryColor { get; set; } = "#007bff";
        public string AccountName { get; set; } = null!;
        public TransactionType Type { get; set; }
    }
}
