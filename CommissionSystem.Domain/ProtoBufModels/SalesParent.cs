using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace CommissionSystem.Domain.ProtoBufModels
{
    [ProtoContract]
    public class SalesParent
    {
        public SalesParent()
        {
            ParentAgentList = new List<SalesParent>();
            ChildAgentList = new List<SalesParent>();
            CustomerList = new List<Customer>();
            childiDDic = new Dictionary<int, bool>();
            parentIDDic = new Dictionary<int, bool>();
            customerDic = new Dictionary<int, bool>();
        }

        [ProtoMember(1)]
        public int SParentID { get; set; }
        [ProtoMember(2)]
        public string SParentName { get; set; }
        [ProtoMember(3)]
        public string GeographyCode { get; set; }
        [ProtoMember(4)]
        public int RptParentID { get; set; }
        [ProtoMember(5)]
        public int MasterAgentID { get; set; }
        [ProtoMember(6)]
        public List<SalesParent> ChildAgentList { get; private set; }
        [ProtoMember(7)]
        public List<SalesParent> ParentAgentList { get; private set; }
        [ProtoMember(8)]
        public decimal Amount { get; set; }
        [ProtoMember(9)]
        public decimal DirectCommission { get; set; }
        [ProtoMember(10)]
        public decimal SubCommission { get; set; }
        [ProtoMember(11)]
        public double CommissionRate { get; set; }
        [ProtoMember(12)]
        public double TierCommissionRate { get; set; }
        [ProtoMember(13)]
        public List<Customer> CustomerList { get; private set; }
        private Dictionary<int, bool> childiDDic;
        private Dictionary<int, bool> parentIDDic;
        private Dictionary<int, bool> customerDic;

        public void AddParentAgent(SalesParent o)
        {
            if (!parentIDDic.ContainsKey(o.SParentID))
            {
                parentIDDic[o.SParentID] = true;
                ParentAgentList.Add(o);
            }
        }

        public void AddChildAgent(SalesParent o)
        {
            if (!childiDDic.ContainsKey(o.SParentID))
            {
                childiDDic[o.SParentID] = true;
                o.ParentAgentList.Add(this);
                ChildAgentList.Add(o);
            }
        }

        public void AddCustomer(Customer o)
        {
            if (!customerDic.ContainsKey(o.CustID))
            {
                customerDic[o.CustID] = true;
                CustomerList.Add(o);
            }
        }

        public void AddToSubCommission(decimal comm)
        {
            SubCommission += comm;
        }

        public AgentView GetAgentInfo()
        {
            AgentView o = new AgentView();
            o.AgentID = SParentID;
            o.AgentName = SParentName;
            o.AgentTeam = MasterAgentID.ToString();

            return o;
        }

        public string UID
        {
            get
            {
                return string.Format("{0}|{1}", SParentID, MasterAgentID);
            }
        }

        public decimal TotalCommission
        {
            get
            {
                return DirectCommission + SubCommission;
            }
        }

        public bool IsInternalData
        {
            get
            {
                bool a = false;
                string id = SParentID.ToString();

                if (id.IndexOf("881") == 0)
                    a = true;

                return a;
            }
        }

        public bool IsInternalVoice
        {
            get
            {
                bool a = false;
                string id = SParentID.ToString();

                if (id.IndexOf("222") == 0)
                    a = true;

                return a;
            }
        }
    }
}
