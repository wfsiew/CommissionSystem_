using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommissionSystem.WebUI.Areas.Commission.Models
{
    public class SalesParent
    {
        public SalesParent()
        {
            ChildAgentList = new List<SalesParent>();
            CustomerList = new List<Customer>();
        }

        public int SParentID { get; set; }
        public string SParentName { get; set; }
        public string GeographyCode { get; set; }
        public int RptParentID { get; set; }
        public int MasterAgentID { get; set; }
        public List<SalesParent> ChildAgentList { get; private set; }
        public SalesParent ParentAgent { get; private set; }
        public int Level { get; set; }
        public decimal Amount { get; set; }
        public decimal DirectCommission { get; set; }
        public decimal SubCommission { get; set; }
        public double CommissionRate { get; set; }
        public double TierCommissionRate { get; set; }
        public bool IsRoot { get; set; }
        public List<Customer> CustomerList { get; private set; }

        public void AddChildAgent(SalesParent o)
        {
            o.ParentAgent = this;
            o.Level = this.Level + 1;
            ChildAgentList.Add(o);
        }

        public void AddCustomer(Customer o)
        {
            CustomerList.Add(o);
        }

        public void AddToSubCommission(decimal comm)
        {
            SubCommission += comm;
        }

        public string UID
        {
            get
            {
                return string.Format("{0}|{1}", SParentID, Level);
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