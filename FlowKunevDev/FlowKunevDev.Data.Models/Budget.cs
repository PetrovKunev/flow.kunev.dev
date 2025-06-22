using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowKunevDev.Data.Models
{
    [Table("Budgets")]
    public class Budget
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Името на бюджета е задължително")]
        [StringLength(200, ErrorMessage = "Името не може да бъде по-дълго от 200 символа")]
        [Display(Name = "Име")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Сумата е задължителна")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Сумата трябва да бъде по-голяма от 0")]
        [Display(Name = "Сума")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Категорията е задължителна")]
        [Display(Name = "Категория")]
        [ForeignKey("Category")]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(450)]
        [ForeignKey("User")]
        public string UserId { get; set; } = null!;

        [Required(ErrorMessage = "Началната дата е задължителна")]
        [Display(Name = "Начална дата")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Крайната дата е задължителна")]
        [Display(Name = "Крайна дата")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Активен")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Дата на създаване")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Category Category { get; set; } = null!;

        // Computed property за изразходвана сума (не се записва в БД)
        [NotMapped]
        [Display(Name = "Изразходвано")]
        public decimal SpentAmount { get; set; }

        [NotMapped]
        [Display(Name = "Оставащо")]
        public decimal RemainingAmount => Amount - SpentAmount;

        [NotMapped]
        [Display(Name = "Процент изразходвано")]
        public decimal SpentPercentage => Amount > 0 ? (SpentAmount / Amount) * 100 : 0;
    }
}
