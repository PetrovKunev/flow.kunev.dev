using System.ComponentModel.DataAnnotations;

namespace FlowKunevDev.Services.DTOs
{
    public class CreateAccountTransferDto
    {
        [Required(ErrorMessage = "Сумата е задължителна")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Сумата трябва да бъде по-голяма от 0")]
        [Display(Name = "Сума")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Изходящата сметка е задължителна")]
        [Display(Name = "От сметка")]
        public int FromAccountId { get; set; }

        [Required(ErrorMessage = "Входящата сметка е задължителна")]
        [Display(Name = "Към сметка")]
        public int ToAccountId { get; set; }

        [Required(ErrorMessage = "Датата е задължителна")]
        [Display(Name = "Дата")]
        public DateTime Date { get; set; } = DateTime.Now;

        [StringLength(500, ErrorMessage = "Описанието не може да бъде по-дълго от 500 символа")]
        [Display(Name = "Описание")]
        public string? Description { get; set; }

        [StringLength(1000, ErrorMessage = "Бележките не могат да бъдат по-дълги от 1000 символа")]
        [Display(Name = "Бележки")]
        public string? Notes { get; set; }
    }
}