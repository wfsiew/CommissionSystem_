using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommissionSystem.WebUI.Helpers
{
    public class UIHelperr
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
    }
}