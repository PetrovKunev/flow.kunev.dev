using System.ComponentModel.DataAnnotations;
using FlowKunevDev.Common;

namespace FlowKunevDev.Services.DTOs
{
    public class CreateTransactionDto
    {
        [Required(ErrorMessage = "Описанието е задължително")]
        [StringLength(200, ErrorMessage = "Описанието не може да бъде по-дълго от 200 символа")]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "Сумата е задължителна")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Сумата трябва да бъде по-голяма от 0")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Датата е задължителна")]
        public DateTime Date { get; set; } = TimeHelper.UtcNow;

        [Required(ErrorMessage = "Категорията е задължителна")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Сметката е задължителна")]
        public int AccountId { get; set; }

        [Required(ErrorMessage = "Типът е задължителен")]
        public TransactionType Type { get; set; }

        [StringLength(1000, ErrorMessage = "Бележките не могат да бъдат по-дълги от 1000 символа")]
        public string? Notes { get; set; }
    }
}
