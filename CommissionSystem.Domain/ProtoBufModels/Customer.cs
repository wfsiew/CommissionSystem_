using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace CommissionSystem.Domain.ProtoBufModels
{
    [ProtoContract]
    public class Customer
    {
        public Customer()
        {
            BillingInfoList = new List<CustomerBillingInfo>();
            SettlementList = new List<CustomerSettlement>();
            settlementdic = new Dictionary<int, bool>();
        }

        [ProtoMember(1)]
        public int CustID { get; set; }
        [ProtoMember(2)]
        public string Name { get; set; }
        [ProtoMember(3)]
        public int RatePlanID { get; set; }
        [ProtoMember(4)]
        public int CustomerType { get; set; }
        [ProtoMember(5)]
        public int BillingDay { get; set; }
        [ProtoMember(6)]
        public int Status { get; set; }
        [ProtoMember(7)]
        public List<CustomerBillingInfo> BillingInfoList { get; set; }
        [ProtoMember(8)]
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

        public bool HasSettlement(CustomerSettlement o)
        {
            return settlementdic.ContainsKey(o.SettlementIdx);
        }
    }
}
