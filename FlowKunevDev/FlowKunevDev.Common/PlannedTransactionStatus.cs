using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowKunevDev.Common
{
    public enum PlannedTransactionStatus
    {
        [Display(Name = "Планирана")]
        Planned = 1,
        [Display(Name = "Изпълнена")]
        Executed = 2,
        [Display(Name = "Отказана")]
        Cancelled = 3
    }
}
