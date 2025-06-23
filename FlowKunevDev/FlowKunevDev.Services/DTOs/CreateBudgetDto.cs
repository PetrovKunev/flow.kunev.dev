using System;
using System.ComponentModel.DataAnnotations;

namespace FlowKunevDev.Services.DTOs
{
    public class CreateBudgetDto
    {
        [Required(ErrorMessage = "Името на бюджета е задължително")]
        [StringLength(200, ErrorMessage = "Името не може да бъде по-дълго от 200 символа")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Сумата е задължителна")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Сумата трябва да бъде по-голяма от 0")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Категорията е задължителна")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Началната дата е задължителна")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Крайната дата е задължителна")]
        public DateTime EndDate { get; set; }
    }
}