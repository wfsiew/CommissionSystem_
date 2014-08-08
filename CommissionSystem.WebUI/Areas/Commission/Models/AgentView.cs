using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommissionSystem.WebUI.Areas.Commission.Models
{
    public class AgentView
    {
        public int AgentID { get; set; }
        public string AgentName { get; set; }
        public string AgentType { get; set; }
        public string AgentLevel { get; set; }
        public string AgentTeam { get; set; }
        public int Level { get; set; }
        public decimal TotalSettlement { get; set; }
        public decimal TotalCommission { get; set; }
    }
}