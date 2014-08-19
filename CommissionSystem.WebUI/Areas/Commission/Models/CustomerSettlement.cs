using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommissionSystem.WebUI.Areas.Commission.Models
{
    public class CustomerSettlement
    {
        public CustomerSettlement()
        {
            InvoiceList = new List<Invoice>();
            invoiceDic = new Dictionary<string, bool>();
        }

        public int SettlementIdx { get; set; }
        public int CustID { get; set; }
        public string Comment { get; set; }
        public decimal Amount { get; set; }
        public DateTime RealDate { get; set; }
        public int PaymentType { get; set; }
        public string Reference { get; set; }
        public string ORNo { get; set; }
        public int PaymentMode { get; set; }
        public decimal CallCharge { get; set; }
        public List<Invoice> InvoiceList { get; private set; }
        private Dictionary<string, bool> invoiceDic;

        public void AddInvoice(Invoice o)
        {
            if (!invoiceDic.ContainsKey(o.InvoiceNumber))
            {
                invoiceDic[o.InvoiceNumber] = true;
                InvoiceList.Add(o);
            }
        }
    }
}