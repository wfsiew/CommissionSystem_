using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using NLog;

namespace CommissionSystem.Domain.Models
{
    public class MetroEInternal
    {
        public int Type { get; set; }
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

        public double GetCommission(double amt, int level)
        {
            double a = GetCommissionRate(level);
            double x = a * amt;
            return x;
        }

        public static Dictionary<int, MetroEInternal> LoadList(string path)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                XmlNodeList x = doc.SelectNodes("CommList/Comm");
                Dictionary<int, MetroEInternal> l = new Dictionary<int, MetroEInternal>();

                foreach (XmlNode n in x)
                {
                    MetroEInternal o = MetroEInternal.Load(n);
                    l.Add(o.Type, o);
                }

                return l;
            }

            catch (Exception e)
            {
                Logger.Debug("", e);
                throw e;
            }
        }

        private static MetroEInternal Load(XmlNode n)
        {
            try
            {
                string type = n.Attributes["type"].Value;
                string value = n.Attributes["value"].Value;
                string tier1 = n.Attributes["tier1"].Value;
                string tier2 = n.Attributes["tier2"].Value;
                string tier3 = n.Attributes["tier3"].Value;

                MetroEInternal o = new MetroEInternal();
                o.Type = Convert.ToInt32(type);
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

    public class MetroEExternal : MetroEInternal
    {
    }
}
