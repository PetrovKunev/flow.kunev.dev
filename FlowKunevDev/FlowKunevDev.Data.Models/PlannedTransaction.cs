using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FlowKunevDev.Common;
using Microsoft.EntityFrameworkCore;

namespace FlowKunevDev.Data.Models
{
    [Table("PlannedTransactions")]
    [Index(nameof(UserId), nameof(PlannedDate), Name = "IX_PlannedTransactions_UserId_PlannedDate")]
    public class PlannedTransaction
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Описанието е задължително")]
        [StringLength(200, ErrorMessage = "Описанието не може да бъде по-дълго от 200 символа")]
        [Display(Name = "Описание")]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "Планираната сума е задължителна")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Планираната сума трябва да бъде по-голяма от 0")]
        [Display(Name = "Планирана сума")]
        public decimal PlannedAmount { get; set; }

        [Required(ErrorMessage = "Планираната дата е задължителна")]
        [Display(Name = "Планирана дата")]
        public DateTime PlannedDate { get; set; }

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
        public string? Notes { get; set; }

        [Display(Name = "Статус")]
        public PlannedTransactionStatus Status { get; set; } = PlannedTransactionStatus.Planned;

        [Display(Name = "Повтаряща се")]
        public bool IsRecurring { get; set; } = false;

        [Display(Name = "Честота")]
        public RecurrenceType? RecurrenceType { get; set; }

        [Display(Name = "ID на създадена транзакция")]
        [ForeignKey("ExecutedTransaction")]
        public int? ExecutedTransactionId { get; set; }

        [Display(Name = "Дата на създаване")]
        public DateTime CreatedDate { get; set; } = TimeHelper.UtcNow;

        // Navigation properties
        public virtual Category Category { get; set; } = null!;
        public virtual Account Account { get; set; } = null!;
        public virtual Transaction ExecutedTransaction { get; set; } = null!;
    }
}
