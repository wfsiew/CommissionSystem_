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
            SettlementList = new List<CustomerSettlement>();
            settlementdic = new Dictionary<int, bool>();
        }

        public int CustID { get; set; }
        public string Name { get; set; }
        public int RatePlanID { get; set; }
        public int CustomerType { get; set; }
        public int BillingDay { get; set; }
        public int Status { get; set; }
        public List<CustomerBillingInfo> BillingInfoList { get; set; }
        public List<CustomerSettlement> SettlementList { get; private set; }
        private Dictionary<int, bool> settlementdic;

        public void AddBillingInfo(CustomerBillingInfo o)
        {
            BillingInfoList.Add(o);
        }

        public void AddSettlement(CustomerSettlement o)
        {
            if (!settlementdic.ContainsKey(o.SettlementIdx))
            {
                settlementdic[o.SettlementIdx] = true;
                SettlementList.Add(o);
            }
        }
    }
}