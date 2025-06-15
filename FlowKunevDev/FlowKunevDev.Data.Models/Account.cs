using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Transactions;
using FlowKunevDev.Common;

namespace FlowKunevDev.Data.Models
{
    [Table("Accounts")]
    public class Account
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Името на сметката е задължително")]
        [StringLength(100, ErrorMessage = "Името не може да бъде по-дълго от 100 символа")]
        [Display(Name = "Име на сметка")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Описанието не може да бъде по-дълго от 500 символа")]
        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Началният баланс е задължителен")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Начален баланс")]
        [Range(0, double.MaxValue, ErrorMessage = "Началният баланс не може да бъде отрицателен")]
        public decimal InitialBalance { get; set; }

        [Required(ErrorMessage = "Типът на сметката е задължителен")]
        [Display(Name = "Тип сметка")]
        public AccountType Type { get; set; }

        [Required(ErrorMessage = "Валутата е задължителна")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Валутата трябва да бъде 3 символа")]
        [Display(Name = "Валута")]
        public string Currency { get; set; } = "BGN";

        [StringLength(7, ErrorMessage = "Цветът трябва да бъде в hex формат")]
        [Display(Name = "Цвят")]
        public string Color { get; set; } = "#007bff";

        [Required]
        [StringLength(450)] // Identity UserId максимална дължина
        [ForeignKey("User")]
        public string UserId { get; set; }

        [Required]
        [Display(Name = "Дата на създаване")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Активна")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [InverseProperty("Account")]
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

        [InverseProperty("FromAccount")]
        public virtual ICollection<AccountTransfer> TransfersFrom { get; set; } = new List<AccountTransfer>();

        [InverseProperty("ToAccount")]
        public virtual ICollection<AccountTransfer> TransfersTo { get; set; } = new List<AccountTransfer>();

        [InverseProperty("Account")]
        public virtual ICollection<PlannedTransaction> PlannedTransactions { get; set; } = new List<PlannedTransaction>();

        // Computed property за текущ баланс (не се записва в БД)
        [NotMapped]
        [Display(Name = "Текущ баланс")]
        public decimal CurrentBalance { get; set; }
    }
}