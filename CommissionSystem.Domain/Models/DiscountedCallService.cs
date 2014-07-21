
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using NLog;

namespace CommissionSystem.Domain.Models
{
    public class DiscountedCallServiceInternal
    {
        public double Rate { get; set; }
        public double Commission { get; set; }
        public double Tier1 { get; set; }
        public double Tier2 { get; set; }

        public static double MaxRate;
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

        public static Dictionary<double, DiscountedCallServiceInternal> LoadList(string path)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                XmlNodeList x = doc.SelectNodes("RateList/Rate");
                Dictionary<double, DiscountedCallServiceInternal> l = new Dictionary<double, DiscountedCallServiceInternal>();
                bool first = true;

                foreach (XmlNode n in x)
                {
                    DiscountedCallServiceInternal o = DiscountedCallServiceInternal.Load(n);
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

        private static DiscountedCallServiceInternal Load(XmlNode n)
        {
            try
            {
                string value = n.Attributes["value"].Value;
                string comm = n.Attributes["comm"].Value;
                string tier1 = n.Attributes["tier1"].Value;
                string tier2 = n.Attributes["tier2"].Value;

                DiscountedCallServiceInternal o = new DiscountedCallServiceInternal();
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

    public class DiscountedCallServiceExternal
    {
        public double Rate { get; set; }
        public double Commission { get; set; }
        public double Tier1 { get; set; }
        public double Tier2 { get; set; }
        public double Tier3 { get; set; }

        public static double MaxRate;
        private static Logger logger = LogManager.GetCurrentClassLogger();

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

        public static Dictionary<double, DiscountedCallServiceExternal> LoadList(string path)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                XmlNodeList x = doc.SelectNodes("RateList/Rate");
                Dictionary<double, DiscountedCallServiceExternal> l = new Dictionary<double, DiscountedCallServiceExternal>();
                bool first = true;

                foreach (XmlNode n in x)
                {
                    DiscountedCallServiceExternal o = DiscountedCallServiceExternal.Load(n);
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

        private static DiscountedCallServiceExternal Load(XmlNode n)
        {
            try
            {
                string value = n.Attributes["value"].Value;
                string comm = n.Attributes["comm"].Value;
                string tier1 = n.Attributes["tier1"].Value;
                string tier2 = n.Attributes["tier2"].Value;
                string tier3 = n.Attributes["tier3"].Value;

                DiscountedCallServiceExternal o = new DiscountedCallServiceExternal();
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
