﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using NLog;

namespace CommissionSystem.Domain.Models
{
    public class FibrePlusInternal
    {
        public double Commission { get; set; }
        public double Tier1 { get; set; }
        public double Tier2 { get; set; }

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

        public static FibrePlusInternal Load(string path)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                XmlNode n = doc.SelectSingleNode("Comm");
                FibrePlusInternal o = FibrePlusInternal.Load(n);

                return o;
            }

            catch (Exception e)
            {
                Logger.Debug("", e);
                throw e;
            }
        }

        private static FibrePlusInternal Load(XmlNode n)
        {
            try
            {
                string value = n.Attributes["value"].Value;
                string tier1 = n.Attributes["tier1"].Value;
                string tier2 = n.Attributes["tier2"].Value;

                FibrePlusInternal o = new FibrePlusInternal();
                o.Commission = Convert.ToDouble(value);
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

    public class FibrePlusExternal
    {
        public int Type { get; set; }
        public double Commission { get; set; }
        public double Tier1 { get; set; }
        public double Tier2 { get; set; }
        public double Tier3 { get; set; }

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
            decimal x =  Convert.ToDecimal(a) * amt;
            return x;
        }

        public decimal GetDirectCommission(decimal amt)
        {
            double a = Commission;
            decimal x = Convert.ToDecimal(a) * amt;
            return x;
        }

        public static Dictionary<int, FibrePlusExternal> LoadList(string path)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                XmlNodeList x = doc.SelectNodes("CommList/Comm");
                Dictionary<int, FibrePlusExternal> l = new Dictionary<int, FibrePlusExternal>();

                foreach (XmlNode n in x)
                {
                    FibrePlusExternal o = FibrePlusExternal.Load(n);
                    l.Add(o.Type, o);
                }

                return l;
            }

            catch (Exception e)
            {
                logger.Debug("", e);
                throw e;
            }
        }

        public static int GetCommissionType(int n)
        {
            int i = 1;

            if (n >= 11 && n <= 20)
                i = 2;

            else if (i >= 21)
                i = 3;

            return i;
        }

        private static FibrePlusExternal Load(XmlNode n)
        {
            try
            {
                string type = n.Attributes["type"].Value;
                string value = n.Attributes["value"].Value;
                string tier1 = n.Attributes["tier1"].Value;
                string tier2 = n.Attributes["tier2"].Value;
                string tier3 = n.Attributes["tier3"].Value;

                FibrePlusExternal o = new FibrePlusExternal();
                o.Type = Convert.ToInt32(type);
                o.Commission = Convert.ToDouble(value);
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
