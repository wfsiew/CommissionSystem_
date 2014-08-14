using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommissionSystem.WebUI.Areas.Commission.Models
{
    public class Customer
    {
        public Customer()
        {
            BillingInfoList = new List<CustomerBillingInfo>();
            InvoiceList = new List<Invoice>();
            SettlementList = new List<CustomerSettlement>();
        }

        public int CustID { get; set; }
        public string Name { get; set; }
        public int RatePlanID { get; set; }
        public int CustomerType { get; set; }
        public int BillingDay { get; set; }
        public int Status { get; set; }
        public List<CustomerBillingInfo> BillingInfoList { get; set; }
        public List<Invoice> InvoiceList { get; private set; }
        public List<CustomerSettlement> SettlementList { get; private set; }

        public void AddBillingInfo(CustomerBillingInfo o)
        {
            BillingInfoList.Add(o);
        }

        public void AddInvoice(Invoice o)
        {
            InvoiceList.Add(o);
        }

        public void AddSettlement(CustomerSettlement o)
        {
            SettlementList.Add(o);
        }
    }

    public class ADSLCustomer : Customer
    {
        public int MasterAgentID { get; set; }
    }
}