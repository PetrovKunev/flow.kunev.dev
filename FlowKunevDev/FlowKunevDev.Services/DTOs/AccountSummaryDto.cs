using FlowKunevDev.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowKunevDev.Services.DTOs
{
    public class AccountSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal CurrentBalance { get; set; }
        public string Currency { get; set; } = "BGN";
        public string Color { get; set; } = "#007bff";
        public AccountType Type { get; set; }
        public bool IsActive { get; set; }
    }
}
