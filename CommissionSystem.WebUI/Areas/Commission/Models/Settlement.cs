using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommissionSystem.WebUI.Areas.Commission.Models
{
    public class Settlement
    {
        public int CustID { get; set; }
        public string Comment { get; set; }
        public double Amount { get; set; }
        public DateTime RealDate { get; set; }
        public string Reference { get; set; }
        public string ORNo { get; set; }
        public string CustName { get; set; }
    }
}