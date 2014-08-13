using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommissionSystem.WebUI.Areas.Commission.Models
{
    public class Invoice
    {
        public int CustID { get; set; }
        public string InvoiceNumber { get; set; }
        public decimal TotalCurrentCharge { get; set; }
        public DateTime InvoiceDate { get; set; }
        public int SettlementIdx { get; set; }
    }
}