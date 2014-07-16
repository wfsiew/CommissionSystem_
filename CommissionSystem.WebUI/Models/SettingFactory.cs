using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using NLog;
using CommissionSystem.Domain.Models;

namespace CommissionSystem.WebUI.Models
{
    public class SettingFactory
    {
        private static volatile SettingFactory instance;
        private static object syncRoot = new Object();
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        public ADSLInternal ADSLInternalSetting { get; set; }
        public ADSLExternal ADSLExternalSetting { get; set; }
        public CorporateInternetPremiumInternal CorporateInternetPremiumInternalSetting { get; set; }
        public CorporateInternetPremiumInternal CorporateInternetPremiumExternalSetting { get; set; }
        public CorporateInternetProInternal CorporateInternetProInternalSetting { get; set; }
        public CorporateInternetProInternal CorporateInternetProExternalSetting { get; set; }
        public Dictionary<double, DiscountedCallServiceInternal> DiscountedCallServiceInternalSetting { get; set; }
        public Dictionary<double, DiscountedCallServiceExternal> DiscountedCallServiceExternalSetting { get; set; }
        public Dictionary<double, E1Internal> E1InternalSetting { get; set; }
        public Dictionary<double, E1External> E1ExternalSetting { get; set; }
        public FibrePlusInternal FibrePlusInternalSetting { get; set; }
        public Dictionary<int, FibrePlusExternal> FibrePlusExternalSetting { get; set; }
        public FibrePlusVoiceInternal FibrePlusVoiceInternalSetting { get; set; }
        public FibrePlusVoiceExternal FibrePlusVoiceExternalSetting { get; set; }
        public Dictionary<int, IDDInternal> IDDInternalSetting { get; set; }
        public Dictionary<int, IDDExternal> IDDExternalSetting { get; set; }
        public Dictionary<int, MetroEInternal> MetroEInternalSetting { get; set; }
        public Dictionary<int, MetroEInternal> MetroEExternalSetting { get; set; }
        public OneTimeServicesInternal OneTimeServicesInternalSetting { get; set; }
        public OneTimeServicesExternal OneTimeServicesExternalSetting { get; set; }
        public Dictionary<int, RecurringContractInternal> RecurringContractInternalSetting { get; set; }
        public Dictionary<int, RecurringContractExternal> RecurringContractExternalSetting { get; set; }
        public Dictionary<double, SIPInternal> SIPInternalSetting { get; set; }
        public Dictionary<double, SIPExternal> SIPExternalSetting { get; set; }
        public SpeedPlusInternal SpeedPlusInternalSetting { get; set; }
        public Dictionary<int, SpeedPlusExternal> SpeedPlusExternalSetting { get; set; }
        public VSATInternal VSATInternalSetting { get; set; }
        public VSATInternal VSATExternalSetting { get; set; }

        private SettingFactory()
        {
            Load();
        }

        public static SettingFactory Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new SettingFactory();
                    }
                }

                return instance;
            }
        }

        private void Load()
        {
            try
            {
                string path = HttpContext.Current.Server.MapPath("~/App_Data/Setting");
                string file = null;

                file = Path.Combine(path, "adsl/internal.xml");
                ADSLInternalSetting = ADSLInternal.Load(file);

                file = Path.Combine(path, "adsl/external.xml");
                ADSLExternalSetting = ADSLExternal.Load(file);

                file = Path.Combine(path, "corporateinternetpremium/internal.xml");
                CorporateInternetPremiumInternalSetting = CorporateInternetPremiumInternal.Load(file);

                file = Path.Combine(path, "corporateinternetpremium/external.xml");
                CorporateInternetPremiumExternalSetting = CorporateInternetPremiumExternal.Load(file);

                file = Path.Combine(path, "corporateinternetpro/internal.xml");
                CorporateInternetProInternalSetting = CorporateInternetProInternal.Load(file);

                file = Path.Combine(path, "corporateinternetpro/external.xml");
                CorporateInternetProExternalSetting = CorporateInternetProExternal.Load(file);

                file = Path.Combine(path, "discountedcallservice/internal.xml");
                DiscountedCallServiceInternalSetting = DiscountedCallServiceInternal.LoadList(file);

                file = Path.Combine(path, "discountedcallservice/external.xml");
                DiscountedCallServiceExternalSetting = DiscountedCallServiceExternal.LoadList(file);

                file = Path.Combine(path, "e1/internal.xml");
                E1InternalSetting = E1Internal.LoadList(file);

                file = Path.Combine(path, "e1/external.xml");
                E1ExternalSetting = E1External.LoadList(file);

                file = Path.Combine(path, "fibre+/internal.xml");
                FibrePlusInternalSetting = FibrePlusInternal.Load(file);

                file = Path.Combine(path, "fibre+/external.xml");
                FibrePlusExternalSetting = FibrePlusExternal.LoadList(file);

                file = Path.Combine(path, "fibre+voice/internal.xml");
                FibrePlusVoiceInternalSetting = FibrePlusVoiceInternal.Load(file);

                file = Path.Combine(path, "fibre+voice/external.xml");
                FibrePlusVoiceExternalSetting = FibrePlusVoiceExternal.Load(file);

                file = Path.Combine(path, "idd/internal.xml");
                IDDInternalSetting = IDDInternal.LoadList(file);

                file = Path.Combine(path, "idd/external.xml");
                IDDExternalSetting = IDDExternal.LoadList(file);

                file = Path.Combine(path, "metro-e/internal.xml");
                MetroEInternalSetting = MetroEInternal.LoadList(file);

                file = Path.Combine(path, "metro-e/external.xml");
                MetroEExternalSetting = MetroEExternal.LoadList(file);

                file = Path.Combine(path, "onetimeservices/internal.xml");
                OneTimeServicesInternalSetting = OneTimeServicesInternal.Load(file);

                file = Path.Combine(path, "onetimeservices/external.xml");
                OneTimeServicesExternalSetting = OneTimeServicesExternal.Load(file);

                file = Path.Combine(path, "recurringcontract/internal.xml");
                RecurringContractInternalSetting = RecurringContractInternal.LoadList(file);

                file = Path.Combine(path, "recurringcontract/external.xml");
                RecurringContractExternalSetting = RecurringContractExternal.LoadList(file);

                file = Path.Combine(path, "sip/internal.xml");
                SIPInternalSetting = SIPInternal.LoadList(file);

                file = Path.Combine(path, "sip/external.xml");
                SIPExternalSetting = SIPExternal.LoadList(file);

                file = Path.Combine(path, "speed+/internal.xml");
                SpeedPlusInternalSetting = SpeedPlusInternal.Load(file);

                file = Path.Combine(path, "speed+/external.xml");
                SpeedPlusExternalSetting = SpeedPlusExternal.LoadList(file);

                file = Path.Combine(path, "vsat/internal.xml");
                VSATInternalSetting = VSATInternal.Load(file);

                file = Path.Combine(path, "vsat/external.xml");
                VSATExternalSetting = VSATExternal.Load(file);
            }
            
            catch (Exception e)
            {
                Logger.Debug("", e);
                throw e;
            }
        }
    }
}