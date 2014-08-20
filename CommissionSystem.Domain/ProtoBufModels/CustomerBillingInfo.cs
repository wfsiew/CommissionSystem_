using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace CommissionSystem.Domain.ProtoBufModels
{
    [ProtoContract]
    public class CustomerBillingInfo
    {
        [ProtoMember(1)]
        public int CustID { get; set; }
        [ProtoMember(2)]
        public decimal Rental { get; set; }
        [ProtoMember(3)]
        public int ProductID { get; set; }
        [ProtoMember(4)]
        public decimal Amount { get; set; }
        [ProtoMember(5)]
        public DateTime RealCommencementDate { get; set; }
        [ProtoMember(6)]
        public DateTime? RealCommencementEndDate { get; set; }
        [ProtoMember(7)]
        public ProductTypes ProductType { get; set; }
    }
}
