using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommissionSystem.WebUI.Areas.Commission.Models
{
    public class Agent
    {
        public int AgentID { get; set; }
        public string AgentName { get; set; }
        public string AgentType { get; set; }
        public string AgentLevel { get; set; }
        public string AgentTeam { get; set; }
    }
}