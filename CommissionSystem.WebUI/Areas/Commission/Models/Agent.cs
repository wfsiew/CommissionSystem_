using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CommissionSystem.Domain.ProtoBufModels;

namespace CommissionSystem.WebUI.Areas.Commission.Models
{
    public class Agent
    {
        public Agent()
        {
            ChildAgentList = new List<Agent>();
            CustomerList = new List<Customer>();
        }

        public int AgentID { get; set; }
        public string AgentName { get; set; }
        public string AgentType { get; set; }
        public string AgentLevel { get; set; }
        public string AgentTeam { get; set; }
        public List<Agent> ChildAgentList { get; private set; }
        public Agent ParentAgent { get; private set; }
        public int Level { get; set; }
        public decimal Amount { get; set; }
        public decimal DirectCommission { get; set; }
        public decimal SubCommission { get; set; }
        public double CommissionRate { get; set; }
        public double TierCommissionRate { get; set; }
        public List<Customer> CustomerList { get; private set; }

        public void AddChildAgent(Agent o)
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

        public decimal TotalCommission
        {
            get
            {
                return DirectCommission + SubCommission;
            }
        }

        public AgentView GetAgentInfo()
        {
            AgentView o = new AgentView();
            o.AgentID = AgentID;
            o.AgentLevel = AgentLevel;
            o.AgentName = AgentName;
            o.AgentTeam = AgentTeam;
            o.AgentType = AgentType;

            return o;
        }

        public bool IsInternal
        {
            get
            {
                bool a = false;

                if ("master".Equals(AgentType, StringComparison.OrdinalIgnoreCase) ||
                    "SD".Equals(AgentType, StringComparison.OrdinalIgnoreCase) ||
                    "SM".Equals(AgentType, StringComparison.OrdinalIgnoreCase) ||
                    "SE".Equals(AgentType, StringComparison.OrdinalIgnoreCase) ||
                    "NSE".Equals(AgentType, StringComparison.OrdinalIgnoreCase))
                    a = true;

                return a;
            }
        }
    }
}