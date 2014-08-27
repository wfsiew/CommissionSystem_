using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CommissionSystem.Domain.Helpers;
using NLog;

namespace CommissionSystem.Domain.ProtoBufModels
{
    public class CustomerList
    {
        public int CLSFID { get; set; }
        public string CLCustListIndex { get; set; }
        public int CLCustID { get; set; }
    }

    public class CustomerListPackage
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        public int LPSFID { get; set; }
        public string LPCustListIndex { get; set; }
        public string LPCustListDesc { get; set; }
        public string LPCPackID { get; set; }

        public CallRate GetRateVoice()
        {
            CallRate o = new CallRate();
            string a = LPCustListDesc;

            if (string.IsNullOrEmpty(a))
                return null;

            if (a.IndexOf("CP(", StringComparison.OrdinalIgnoreCase) == 0)
            {
                Match idd = GetMatchDecimal(a);
                Match std = idd.NextMatch();
                Match mob = std.NextMatch();

                if (idd.Success && std.Success && mob.Success)
                {
                    o.IDD2 = Utils.GetValue<double>(idd.Value);
                    o.STD = Utils.GetValue<double>(std.Value);
                    o.MOB = Utils.GetValue<double>(mob.Value);
                }

                //else
                //    Logger.Trace("Pattern does not match: {0}", a);
            }

            else if (a.IndexOf("IDDSTDMOB", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                Match m = GetMatchDecimal(a);
                if (m.Success)
                {
                    double v = Utils.GetValue<double>(m.Value);
                    o.IDD2 = v;
                    o.STD = v;
                    o.MOB = v;
                }

                //else
                //    Logger.Trace("Pattern does not match: {0}", a);
            }

            else if (a.IndexOf("IDD(", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                Match m = GetMatchDecimal(a);
                if (m.Success)
                {
                    double v = Utils.GetValue<double>(m.Value);
                    o.IDD2 = v;
                    o.STD = v;
                    o.MOB = v;
                }

                //else
                //    Logger.Trace("Pattern does not match: {0}", a);
            }

            else if (a.IndexOf("over", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                Match m = GetMatchDecimal(a);
                if (m.Success)
                {
                    double v = Utils.GetValue<double>(m.Value);
                    o.IDD2 = v;
                    o.STD = v;
                    o.MOB = v;
                }

                //else
                //    Logger.Trace("Pattern does not match: {0}", a);
            }

            //else
            //    Logger.Trace("Pattern does not match: {0}", a);

            return o;
        }

        public double GetRateData()
        {
            double x = 0;
            string a = LPCustListDesc;

            if (string.IsNullOrEmpty(a))
                return x;

            Match m = GetMatchDecimal(a);
            if (m.Success)
            {
                x = Utils.GetValue<double>(m.Value);
                x *= 0.01;
            }

            return x;
        }

        private Match GetMatchDecimal(string a)
        {
            Match m = Regex.Match(a, @"-?\d+(?:\.\d+)?", RegexOptions.Compiled);
            return m;
        }
    }
}
