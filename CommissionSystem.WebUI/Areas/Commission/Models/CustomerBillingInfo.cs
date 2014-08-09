using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommissionSystem.WebUI.Areas.Commission.Models
{
    public class CustomerBillingInfo
    {
        public int CustID { get; set; }
        public decimal Rental { get; set; }
        public int ProductID { get; set; }
        public decimal Amount { get; set; }
        public DateTime RealCommencementDate { get; set; }
        public DateTime? RealCommencementEndDate { get; set; }
        public ProductTypes ProductType { get; set; }
    }
}