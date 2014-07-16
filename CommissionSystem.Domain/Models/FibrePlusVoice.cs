using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using NLog;

namespace CommissionSystem.Domain.Models
{
    public class FibrePlusVoiceInternal
    {
        public double Commission { get; set; }
        public double Tier1 { get; set; }
        public double Tier2 { get; set; }

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

        public static FibrePlusVoiceInternal Load(string path)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                XmlNode n = doc.SelectSingleNode("Comm");
                FibrePlusVoiceInternal o = FibrePlusVoiceInternal.Load(n);

                return o;
            }

            catch (Exception e)
            {
                Logger.Debug("", e);
                throw e;
            }
        }

        private static FibrePlusVoiceInternal Load(XmlNode n)
        {
            try
            {
                string value = n.Attributes["value"].Value;
                string tier1 = n.Attributes["tier1"].Value;
                string tier2 = n.Attributes["tier2"].Value;

                FibrePlusVoiceInternal o = new FibrePlusVoiceInternal();
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

    public class FibrePlusVoiceExternal
    {
        public double Commission { get; set; }
        public double Tier1 { get; set; }
        public double Tier2 { get; set; }
        public double Tier3 { get; set; }

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

        public static FibrePlusVoiceExternal Load(string path)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                XmlNode n = doc.SelectSingleNode("Comm");
                FibrePlusVoiceExternal o = FibrePlusVoiceExternal.Load(n);

                return o;
            }

            catch (Exception e)
            {
                logger.Debug("", e);
                throw e;
            }
        }

        private static FibrePlusVoiceExternal Load(XmlNode n)
        {
            try
            {
                string value = n.Attributes["value"].Value;
                string tier1 = n.Attributes["tier1"].Value;
                string tier2 = n.Attributes["tier2"].Value;
                string tier3 = n.Attributes["tier3"].Value;

                FibrePlusVoiceExternal o = new FibrePlusVoiceExternal();
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
