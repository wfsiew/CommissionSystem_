using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommissionSystem.WebUI.Areas.Commission.Models
{
    public class CallCharge
    {
        public decimal Total { get; set; }
        public decimal IDD { get; set; }
        public decimal STD { get; set; }
        public decimal MOB { get; set; }
    }
}