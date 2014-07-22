using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommissionSystem.WebUI.Areas.Commission.Models
{
    public class Agent
    {
        public Agent()
        {
            ChildAgentList = new List<Agent>();
        }

        public int AgentID { get; set; }
        public string AgentName { get; set; }
        public string AgentType { get; set; }
        public string AgentLevel { get; set; }
        public string AgentTeam { get; set; }
        public List<Agent> ChildAgentList { get; private set; }
        public Agent ParentAgent { get; private set; }
        public int Level { get; set; }
        public double OwnCommission { get; set; }
        public double SubCommission { get; set; }
        public double CommissionRate { get; set; }

        public void AddChildAgent(Agent o)
        {
            o.ParentAgent = this;
            o.Level = this.Level + 1;
            ChildAgentList.Add(o);
        }

        public void AddToSubCommission(double comm)
        {
            SubCommission += comm;
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