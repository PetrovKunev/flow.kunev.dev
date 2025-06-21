using System.ComponentModel.DataAnnotations;

namespace FlowKunevDev.Services.DTOs
{
    public class AccountTransferDto
    {
        public int Id { get; set; }

        [Display(Name = "Сума")]
        public decimal Amount { get; set; }

        [Display(Name = "От сметка")]
        public int FromAccountId { get; set; }
        public string FromAccountName { get; set; } = null!;
        public string FromAccountColor { get; set; } = "#007bff";

        [Display(Name = "Към сметка")]
        public int ToAccountId { get; set; }
        public string ToAccountName { get; set; } = null!;
        public string ToAccountColor { get; set; } = "#007bff";

        [Display(Name = "Дата")]
        public DateTime Date { get; set; }

        [Display(Name = "Описание")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Бележки")]
        public string Notes { get; set; } = string.Empty;

        public string UserId { get; set; } = null!;

        [Display(Name = "Дата на създаване")]
        public DateTime CreatedDate { get; set; }
    }
}