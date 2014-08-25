using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommissionSystem.WebUI.Helpers
{
    public class UIHelper
    {
        public static string GetCorporateMenuCss(dynamic x)
        {
            string a = null;

            if (x == Constants.DISCOUNTED_CALL_SERVICE ||
                x == Constants.SIP ||
                x == Constants.E1 ||
                x == Constants.CORPORATE_DATA)
                a = "active";

            return a;
        }

        public static string GetCorporateVoiceMenuCss(dynamic x)
        {
            string a = null;

            if (x == Constants.DISCOUNTED_CALL_SERVICE ||
                x == Constants.SIP ||
                x == Constants.E1)
                a = "active";

            return a;
        }

        public static string GetAgentMenuCss(dynamic x)
        {
            string a = null;

            if (x == Constants.AGENT_STRUCTURE_DATA ||
                x == Constants.AGENT_STRUCTURE_VOICE ||
                x == Constants.AGENT_STRUCTURE_FIBREPLUS ||
                x == Constants.AGENT_STRUCTURE_SPEEDPLUS)
                a = "active";

            return a;
        }

        public static string GetAgentCorporateVoiceMenuCss(dynamic x)
        {
            string a = null;

            if (x == Constants.AGENT_STRUCTURE_VOICE)
                a = "active";

            return a;
        }

        public static string GetAgentCorporateDataMenuCss(dynamic x)
        {
            string a = null;

            if (x == Constants.AGENT_STRUCTURE_DATA)
                a = "active";

            return a;
        }

        public static string GetAgentFibrePlusMenuCss(dynamic x)
        {
            string a = null;

            if (x == Constants.AGENT_STRUCTURE_FIBREPLUS)
                a = "active";

            return a;
        }

        public static string GetAgentSpeedPlusMenuCss(dynamic x)
        {
            string a = null;

            if (x == Constants.AGENT_STRUCTURE_SPEEDPLUS)
                a = "active";

            return a;
        }
    }
}