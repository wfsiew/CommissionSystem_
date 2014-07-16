using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using NLog;

namespace CommissionSystem.Domain.Models
{
    public class SIPInternal
    {
        public double Rate { get; set; }
        public double Commission { get; set; }
        public double Tier1 { get; set; }
        public double Tier2 { get; set; }

        public static double MaxRate { get; set; }
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        public double GetCommission(double amt, int level)
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

                default:
                    a = Commission;
                    break;
            }

            double x = a * amt;
            return x;
        }

        public static Dictionary<double, SIPInternal> LoadList(string path)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                XmlNodeList x = doc.SelectNodes("RateList/Rate");
                Dictionary<double, SIPInternal> l = new Dictionary<double, SIPInternal>();
                bool first = true;

                foreach (XmlNode n in x)
                {
                    SIPInternal o = SIPInternal.Load(n);
                    if (first)
                    {
                        MaxRate = o.Rate;
                        first = false;
                    }

                    l.Add(o.Rate, o);
                }

                return l;
            }

            catch (Exception e)
            {
                Logger.Debug("", e);
                throw e;
            }
        }

        private static SIPInternal Load(XmlNode n)
        {
            try
            {
                string value = n.Attributes["value"].Value;
                string comm = n.Attributes["comm"].Value;
                string tier1 = n.Attributes["tier1"].Value;
                string tier2 = n.Attributes["tier2"].Value;

                SIPInternal o = new SIPInternal();
                o.Rate = Convert.ToDouble(value);
                o.Commission = Convert.ToDouble(comm);
                o.Tier1 = Convert.ToDouble(tier1);
                o.Tier2 = Convert.ToDouble(tier2);

                return o;
            }

            catch (Exception e)
            {
                Logger.Debug("", e);
                throw e;
            }
        }
    }

    public class SIPExternal
    {
        public double Rate { get; set; }
        public double Commission { get; set; }
        public double Tier1 { get; set; }
        public double Tier2 { get; set; }
        public double Tier3 { get; set; }

        public static double MaxRate;
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public double GetCommission(double amt, int level)
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

            double x = a * amt;
            return x;
        }

        public static Dictionary<double, SIPExternal> LoadList(string path)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                XmlNodeList x = doc.SelectNodes("RateList/Rate");
                Dictionary<double, SIPExternal> l = new Dictionary<double, SIPExternal>();
                bool first = true;

                foreach (XmlNode n in x)
                {
                    SIPExternal o = SIPExternal.Load(n);
                    if (first)
                    {
                        MaxRate = o.Rate;
                        first = false;
                    }

                    l.Add(o.Rate, o);
                }

                return l;
            }

            catch (Exception e)
            {
                logger.Debug("", e);
                throw e;
            }
        }

        private static SIPExternal Load(XmlNode n)
        {
            try
            {
                string value = n.Attributes["value"].Value;
                string comm = n.Attributes["comm"].Value;
                string tier1 = n.Attributes["tier1"].Value;
                string tier2 = n.Attributes["tier2"].Value;
                string tier3 = n.Attributes["tier3"].Value;

                SIPExternal o = new SIPExternal();
                o.Rate = Convert.ToDouble(value);
                o.Commission = Convert.ToDouble(comm);
                o.Tier1 = Convert.ToDouble(tier1);
                o.Tier2 = Convert.ToDouble(tier2);
                o.Tier3 = Convert.ToDouble(tier3);

                return o;
            }

            catch (Exception e)
            {
                logger.Debug("", e);
                throw e;
            }
        }
    }
}
