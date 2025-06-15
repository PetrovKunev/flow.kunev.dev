using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowKunevDev.Common;

namespace FlowKunevDev.Data.Models
{
    [Table("Categories")]
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Името на категорията е задължително")]
        [StringLength(100, ErrorMessage = "Namnet не може да бъде по-дълго от 100 символа")]
        [Display(Name = "Име")]
        public string Name { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Описанието не може да бъде по-дълго от 500 символа")]
        [Display(Name = "Описание")]
        public string Description { get; set; } = string.Empty;

        [StringLength(7, ErrorMessage = "Цветът трябва да бъде в hex формат")]
        [Display(Name = "Цвят")]
        public string Color { get; set; } = "#007bff";

        [StringLength(50, ErrorMessage = "Иконата не може да бъде по-дълга от 50 символа")]
        [Display(Name = "Икона")]
        public string Icon { get; set; } = "fa fa-folder"; // Default icon

        [Display(Name = "Тип категория")]
        public CategoryType Type { get; set; } = CategoryType.Both;

        [Display(Name = "Активна")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [InverseProperty("Category")]
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

        [InverseProperty("Category")]
        public virtual ICollection<PlannedTransaction> PlannedTransactions { get; set; } = new List<PlannedTransaction>();

        [InverseProperty("Category")]
        public virtual ICollection<Budget> Budgets { get; set; } = new List<Budget>();
    }
}
