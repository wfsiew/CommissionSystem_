using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommissionSystem.WebUI.Areas.Commission.Models
{
    public class CommissionView
    {
        public Customer Customer { get; set; }
        public decimal SettlementAmount { get; set; }
        public decimal Commission { get; set; }
        public double CommissionRate { get; set; }
        
    }

    public class VoiceCommissionView : CommissionView
    {
        public decimal CallCharge { get; set; }
        public decimal CallChargeIDD { get; set; }
        public decimal CallChargeSTD { get; set; }
        public decimal CallChargeMOB { get; set; }
        public decimal CommissionIDD { get; set; }
        public decimal CommissionSTD { get; set; }
        public decimal CommissionMOB { get; set; }
        public double CommissionRateIDD { get; set; }
        public double CommissionRateSTD { get; set; }
        public double CommissionRateMOB { get; set; }
    }
}