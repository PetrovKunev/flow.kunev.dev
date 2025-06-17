using FlowKunevDev.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowKunevDev.Services.DTOs
{
    public class UpdateTransactionDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Описанието е задължително")]
        [StringLength(200, ErrorMessage = "Описанието не може да бъде по-дълго от 200 символа")]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "Сумата е задължителна")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Сумата трябва да бъде по-голяма от 0")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Датата е задължителна")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Категорията е задължителна")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Сметката е задължителна")]
        public int AccountId { get; set; }

        [Required(ErrorMessage = "Типът е задължителен")]
        public TransactionType Type { get; set; }

        [StringLength(1000, ErrorMessage = "Бележките не могат да бъдат по-дълги от 1000 символа")]
        public string Notes { get; set; } = string.Empty;
    }
}
