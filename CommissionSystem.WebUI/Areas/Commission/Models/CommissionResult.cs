using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommissionSystem.WebUI.Areas.Commission.Models
{
    public class CommissionResult
    {
        public CommissionResult()
        {
            CommissionViewDic = new Dictionary<string, List<CommissionView>>();
            AgentViewList = new List<AgentView>();
        }

        public Dictionary<string, List<CommissionView>> CommissionViewDic { get; set; }
        public List<AgentView> AgentViewList { get; set; }
    }
}