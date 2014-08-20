using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace CommissionSystem.Domain.ProtoBufModels
{
    [ProtoContract]
    [ProtoInclude(5, typeof(VoiceCommissionView))]
    public class CommissionView
    {
        [ProtoMember(1)]
        public Customer Customer { get; set; }
        [ProtoMember(2)]
        public decimal SettlementAmount { get; set; }
        [ProtoMember(3)]
        public decimal Commission { get; set; }
        [ProtoMember(4)]
        public double CommissionRate { get; set; }

    }

    [ProtoContract]
    public class VoiceCommissionView : CommissionView
    {
        [ProtoMember(1)]
        public decimal CallCharge { get; set; }
        [ProtoMember(2)]
        public decimal CallChargeIDD { get; set; }
        [ProtoMember(3)]
        public decimal CallChargeSTD { get; set; }
        [ProtoMember(4)]
        public decimal CallChargeMOB { get; set; }
        [ProtoMember(5)]
        public decimal CommissionIDD { get; set; }
        [ProtoMember(6)]
        public decimal CommissionSTD { get; set; }
        [ProtoMember(7)]
        public decimal CommissionMOB { get; set; }
        [ProtoMember(8)]
        public double CommissionRateIDD { get; set; }
        [ProtoMember(9)]
        public double CommissionRateSTD { get; set; }
        [ProtoMember(10)]
        public double CommissionRateMOB { get; set; }
    }
}
