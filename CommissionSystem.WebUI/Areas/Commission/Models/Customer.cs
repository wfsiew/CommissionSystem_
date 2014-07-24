using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommissionSystem.WebUI.Areas.Commission.Models
{
    public class Customer
    {
        public int CustID { get; set; }
        public string Name { get; set; }
        public int CustomerType { get; set; }
    }
}