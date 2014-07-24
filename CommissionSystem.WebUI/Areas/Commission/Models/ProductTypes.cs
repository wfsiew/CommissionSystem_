using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommissionSystem.WebUI.Areas.Commission.Models
{
    public class ProductTypes
    {
        public int ProductID { get; set; }
        public string Description { get; set; }
        public decimal InitialAmount { get; set; }
    }
}