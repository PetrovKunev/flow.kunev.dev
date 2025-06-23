using System;
using System.ComponentModel.DataAnnotations;

namespace FlowKunevDev.Services.DTOs
{
    public class BudgetDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Amount { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public decimal SpentAmount { get; set; }
        public decimal RemainingAmount => Amount - SpentAmount;
        public decimal SpentPercentage => Amount > 0 ? (SpentAmount / Amount) * 100 : 0;
    }
}