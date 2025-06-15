using System.ComponentModel.DataAnnotations;

namespace FlowKunevDev.Common
{
    public enum AccountType
    {
        [Display(Name = "Спестявания")]
        Savings = 1,
        [Display(Name = "Заплата")]
        Salary = 2,
        [Display(Name = "Текуща сметка")]
        Current = 3,
        [Display(Name = "Кредитна карта")]
        CreditCard = 4,
        [Display(Name = "Инвестиции")]
        Investment = 5,
        [Display(Name = "Кеш")]
        Cash = 6,
        [Display(Name = "Друго")]
        Other = 7
    }
}