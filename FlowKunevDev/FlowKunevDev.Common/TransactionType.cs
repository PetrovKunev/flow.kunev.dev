using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowKunevDev.Common
{
    public enum TransactionType
    {
        [Display(Name = "Приход")]
        Income = 1,
        [Display(Name = "Разход")]
        Expense = 2
    }
}
