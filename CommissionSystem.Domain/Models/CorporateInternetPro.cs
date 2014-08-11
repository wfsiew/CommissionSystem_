using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using NLog;

namespace CommissionSystem.Domain.Models
{
    public class CorporateInternetProInternal
    {
        public double Commission { get; set; }
        public double Tier1 { get; set; }
        public double Tier2 { get; set; }
        public double Tier3 { get; set; }

        private static Logger Logger = LogManager.GetCurrentClassLogger();

        public double GetCommissionRate(int level)
        {
            double a = 0;

            switch (level)
            {
                case 1:
                    a = Tier1;
                    break;

                case 2:
                    a = Tier2;
                    break;

                case 3:
                    a = Tier3;
                    break;

                default:
                    a = Commission;
                    break;
            }

            return a;
        }

        public decimal GetCommission(decimal amt, int level)
        {

            double a = GetCommissionRate(level);
            decimal x = Convert.ToDecimal(a) * amt;
            return x;
        }

        public decimal GetDirectCommission(decimal amt)
        {
            double a = Commission;
            decimal x = Convert.ToDecimal(a) * amt;
            return x;
        }

        public static CorporateInternetProInternal Load(string path)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                XmlNode n = doc.SelectSingleNode("Comm");
                CorporateInternetProInternal o = CorporateInternetProInternal.Load(n);

                return o;
            }

            catch (Exception e)
            {
                Logger.Debug("", e);
                throw e;
            }
        }

        private static CorporateInternetProInternal Load(XmlNode n)
        {
            try
            {
                string value = n.Attributes["value"].Value;
                string tier1 = n.Attributes["tier1"].Value;
                string tier2 = n.Attributes["tier2"].Value;
                string tier3 = n.Attributes["tier3"].Value;

                CorporateInternetProInternal o = new CorporateInternetProInternal();
                o.Commission = Convert.ToDouble(value);
                o.Tier1 = Convert.ToDouble(tier1);
                o.Tier2 = Convert.ToDouble(tier2);
                o.Tier3 = Convert.ToDouble(tier3);

                return o;
            }

            catch (Exception e)
            {
                Logger.Debug("", e);
                throw e;
            }
        }
    }

    public class CorporateInternetProExternal : CorporateInternetProInternal
    {
    }
}
