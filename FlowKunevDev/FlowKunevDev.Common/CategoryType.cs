using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowKunevDev.Common
{
    public enum CategoryType
    {
        [Display(Name = "Само приходи")]
        IncomeOnly = 1,
        [Display(Name = "Само разходи")]
        ExpenseOnly = 2,
        [Display(Name = "И двете")]
        Both = 3
    }
}
