using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using FlowKunevDev.Common;
using Microsoft.EntityFrameworkCore;

namespace FlowKunevDev.Data.Models
{
    [Table("Transactions")]
    [Index(nameof(UserId), nameof(Date), Name = "IX_Transactions_UserId_Date")]
    [Index(nameof(AccountId), nameof(Date), Name = "IX_Transactions_AccountId_Date")]
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Описанието е задължително")]
        [StringLength(200, ErrorMessage = "Описанието не може да бъде по-дълго от 200 символа")]
        [Display(Name = "Описание")]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "Сумата е задължителна")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Сумата трябва да бъде по-голяма от 0")]
        [Display(Name = "Сума")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Датата е задължителна")]
        [Display(Name = "Дата")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Категорията е задължителна")]
        [Display(Name = "Категория")]
        [ForeignKey("Category")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Сметката е задължителна")]
        [Display(Name = "Сметка")]
        [ForeignKey("Account")]
        public int AccountId { get; set; }

        [Required]
        [StringLength(450)]
        [ForeignKey("User")]
        public string UserId { get; set; } = null!;

        [Required(ErrorMessage = "Типът е задължителен")]
        [Display(Name = "Тип")]
        public TransactionType Type { get; set; }

        [StringLength(1000, ErrorMessage = "Бележките не могат да бъдат по-дълги от 1000 символа")]
        [Display(Name = "Бележки")]
        public string Notes { get; set; } = string.Empty;

        [Display(Name = "Дата на създаване")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Category Category { get; set; } = null!;
        public virtual Account Account { get; set; } = null!;
    }
}
