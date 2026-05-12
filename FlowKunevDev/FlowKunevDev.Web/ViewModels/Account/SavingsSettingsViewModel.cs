using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FlowKunevDev.Web.ViewModels.Account
{
    public class SavingsSettingsViewModel
    {
        [Display(Name = "Сметка за заплата")]
        public int? SalaryAccountId { get; set; }

        [Display(Name = "Сметка за спестявания")]
        public int? SavingsAccountId { get; set; }

        [Range(0, 100, ErrorMessage = "Процентът трябва да е между {1} и {2}.")]
        [Display(Name = "Месечен процент за спестяване (%)")]
        public decimal? MonthlySavingsPercent { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Очакваният приход не може да е отрицателен.")]
        [Display(Name = "Очакван месечен приход (€)")]
        public decimal? ExpectedMonthlyIncome { get; set; }

        public List<SelectListItem> AccountOptions { get; set; } = new();

        public string? StatusMessage { get; set; }
    }
}
