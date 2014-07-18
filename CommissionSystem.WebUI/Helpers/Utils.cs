using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Globalization;

namespace CommissionSystem.WebUI.Helpers
{
    public class Utils
    {
        public const string DATE_FMT = "yyyy-MM-dd";

        public static DateTime GetDateTimeFMT(string q)
        {
            DateTime dt = default(DateTime);

            if (string.IsNullOrEmpty(q))
                return dt;

            dt = DateTime.ParseExact(q, DATE_FMT, CultureInfo.InvariantCulture);

            return dt;
        }

        public static T GetValue<T>(string p, T k = default(T)) where T : struct
        {
            T x = k;

            if (string.IsNullOrEmpty(p))
                return x;

            try
            {
                object o = Convert.ChangeType(p, typeof(T));
                x = (T)o;
            }

            catch (Exception ex)
            {
                throw ex;
            }

            return x;
        }

        public static string GetValue(string p, string k = "")
        {
            if (string.IsNullOrEmpty(p))
                return k;

            return p;
        }
    }
}