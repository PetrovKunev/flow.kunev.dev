using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace FlowKunevDev.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Display(Name = "Сметка за заплата")]
        public int? SalaryAccountId { get; set; }

        [Display(Name = "Сметка за спестявания")]
        public int? SavingsAccountId { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        [Range(0, 100, ErrorMessage = "Процентът трябва да е между 0 и 100.")]
        [Display(Name = "Месечен процент за спестяване")]
        public decimal? MonthlySavingsPercent { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Очакваният приход не може да е отрицателен.")]
        [Display(Name = "Очакван месечен приход")]
        public decimal? ExpectedMonthlyIncome { get; set; }

        // Запазени филтри за дневния бюджет — null означава "стандартен период / всички сметки".
        // Ако DailyBudgetAccountIds е null/empty -> "всички сметки", иначе CSV от Id-та -> "избрани сметки".
        public DateTime? DailyBudgetFromDate { get; set; }

        public DateTime? DailyBudgetToDate { get; set; }

        [MaxLength(512)]
        public string? DailyBudgetAccountIds { get; set; }
    }
}
