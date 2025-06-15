using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowKunevDev.Common
{
    public enum RecurrenceType
    {
        [Display(Name = "Дневно")]
        Daily = 1,
        [Display(Name = "Седмично")]
        Weekly = 2,
        [Display(Name = "Месечно")]
        Monthly = 3,
        [Display(Name = "Годишно")]
        Yearly = 4
    }
}
