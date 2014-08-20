using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace CommissionSystem.Domain.ProtoBufModels
{
    [ProtoContract]
    public class Invoice
    {
        [ProtoMember(1)]
        public int CustID { get; set; }
        [ProtoMember(2)]
        public string InvoiceNumber { get; set; }
        [ProtoMember(3)]
        public decimal CallCharge { get; set; }
        [ProtoMember(4)]
        public decimal TotalCurrentCharge { get; set; }
        [ProtoMember(5)]
        public decimal CallChargesIDD { get; set; }
        [ProtoMember(6)]
        public decimal CallChargesSTD { get; set; }
        [ProtoMember(7)]
        public decimal CallChargesMOB { get; set; }
        [ProtoMember(8)]
        public DateTime InvoiceDate { get; set; }
        [ProtoMember(9)]
        public int SettlementIdx { get; set; }
    }
}
