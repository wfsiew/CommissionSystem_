using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ProtoBuf;

namespace CommissionSystem.Domain.ProtoBufModels
{
    [ProtoContract]
    public class Agent
    {
        public Agent()
        {
            ChildAgentList = new List<Agent>();
            CustomerList = new List<Customer>();
        }

        [ProtoMember(1)]
        public int AgentID { get; set; }
        [ProtoMember(2)]
        public string AgentName { get; set; }
        [ProtoMember(3)]
        public string AgentType { get; set; }
        [ProtoMember(4)]
        public string AgentLevel { get; set; }
        [ProtoMember(5)]
        public string AgentTeam { get; set; }
        [ProtoMember(6)]
        public List<Agent> ChildAgentList { get; private set; }
        [ProtoMember(7)]
        public Agent ParentAgent { get; private set; }
        [ProtoMember(8)]
        public int Level { get; set; }
        [ProtoMember(9)]
        public decimal Amount { get; set; }
        [ProtoMember(10)]
        public decimal DirectCommission { get; set; }
        [ProtoMember(11)]
        public decimal SubCommission { get; set; }
        [ProtoMember(12)]
        public double CommissionRate { get; set; }
        [ProtoMember(13)]
        public double TierCommissionRate { get; set; }
        [ProtoMember(14)]
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