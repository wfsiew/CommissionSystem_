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

        public void AddChildAgent(Agent o)
        {
            o.ParentAgent = this;
            o.Level = this.Level + 1;
            ChildAgentList.Add(o);
        }
    }
}