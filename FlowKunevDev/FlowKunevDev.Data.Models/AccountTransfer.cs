using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowKunevDev.Data.Models
{
    [Table("AccountTransfers")]
    public class AccountTransfer
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Сумата е задължителна")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Сумата трябва да бъде по-голяма от 0")]
        [Display(Name = "Сума")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Изходящата сметка е задължителна")]
        [Display(Name = "От сметка")]
        [ForeignKey("FromAccount")]
        public int FromAccountId { get; set; }

        [Required(ErrorMessage = "Входящата сметка е задължителна")]
        [Display(Name = "Към сметка")]
        [ForeignKey("ToAccount")]
        public int ToAccountId { get; set; }

        [Required(ErrorMessage = "Датата е задължителна")]
        [Display(Name = "Дата")]
        public DateTime Date { get; set; }

        [StringLength(500, ErrorMessage = "Описанието не може да бъде по-дълго от 500 символа")]
        [Display(Name = "Описание")]
        public string Description { get; set; }

        [StringLength(1000, ErrorMessage = "Бележките не могат да бъдат по-дълги от 1000 символа")]
        [Display(Name = "Бележки")]
        public string Notes { get; set; }

        [Required]
        [StringLength(450)]
        [ForeignKey("User")]
        public string UserId { get; set; }

        [Display(Name = "Дата на създаване")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Account FromAccount { get; set; }
        public virtual Account ToAccount { get; set; }
    }
}
