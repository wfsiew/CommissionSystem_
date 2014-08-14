using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using NLog;

namespace CommissionSystem.Domain.Models
{
    public class IDDInternal
    {
        public int IDD { get; set; }
        public double Commission { get; set; }
        public double Tier1 { get; set; }
        public double Tier2 { get; set; }

        public static int MaxRate;
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

        public static Dictionary<int, IDDInternal> LoadList(string path)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                XmlNodeList x = doc.SelectNodes("IDDList/IDD");
                Dictionary<int, IDDInternal> l = new Dictionary<int, IDDInternal>();
                bool first = true;

                foreach (XmlNode n in x)
                {
                    IDDInternal o = IDDInternal.Load(n);
                    if (first)
                    {
                        MaxRate = o.IDD;
                        first = false;
                    }

                    l.Add(o.IDD, o);
                }

                return l;
            }

            catch (Exception e)
            {
                Logger.Debug("", e);
                throw e;
            }
        }

        private static IDDInternal Load(XmlNode n)
        {
            try
            {
                string value = n.Attributes["value"].Value;
                string comm = n.Attributes["comm"].Value;
                string tier1 = n.Attributes["tier1"].Value;
                string tier2 = n.Attributes["tier2"].Value;

                IDDInternal o = new IDDInternal();
                o.IDD = Convert.ToInt32(value);
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

    public class IDDExternal
    {
        public int IDD { get; set; }
        public double Commission { get; set; }
        public double Tier1 { get; set; }
        public double Tier2 { get; set; }
        public double Tier3 { get; set; }

        public static int MaxRate;
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

        public static Dictionary<int, IDDExternal> LoadList(string path)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                XmlNodeList x = doc.SelectNodes("IDDList/IDD");
                Dictionary<int, IDDExternal> l = new Dictionary<int, IDDExternal>();
                bool first = true;

                foreach (XmlNode n in x)
                {
                    IDDExternal o = IDDExternal.Load(n);
                    if (first)
                    {
                        MaxRate = o.IDD;
                        first = false;
                    }

                    l.Add(o.IDD, o);
                }

                return l;
            }

            catch (Exception e)
            {
                logger.Debug("", e);
                throw e;
            }
        }

        private static IDDExternal Load(XmlNode n)
        {
            try
            {
                string value = n.Attributes["value"].Value;
                string comm = n.Attributes["comm"].Value;
                string tier1 = n.Attributes["tier1"].Value;
                string tier2 = n.Attributes["tier2"].Value;
                string tier3 = n.Attributes["tier3"].Value;

                IDDExternal o = new IDDExternal();
                o.IDD = Convert.ToInt32(value);
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
