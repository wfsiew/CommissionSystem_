using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommissionSystem.WebUI.Areas.Commission.Models
{
    public class CommissionView
    {
        public Customer Customer { get; set; }
        public decimal SettlementAmount { get; set; }
        public decimal Commission { get; set; }
        public double CommissionRate { get; set; }
    }
}