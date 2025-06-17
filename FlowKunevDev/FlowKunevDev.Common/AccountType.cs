using System.ComponentModel.DataAnnotations;

namespace FlowKunevDev.Common
{
    public enum AccountType
    {
        [Display(Name = "Спестявания")]
        Savings = 0,
        [Display(Name = "Заплата")]
        Salary = 1,
        [Display(Name = "Текуща сметка")]
        Current = 2,
        [Display(Name = "Кредитна карта")]
        CreditCard = 3,
        [Display(Name = "Инвестиции")]
        Investment = 4,
        [Display(Name = "Кеш")]
        Cash = 5,
        [Display(Name = "Друго")]
        Other = 6
    }
}