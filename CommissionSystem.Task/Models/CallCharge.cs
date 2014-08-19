using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace CommissionSystem.Task.Models
{
    [ProtoContract]
    public class CallCharge
    {
        [ProtoMember(1)]
        public decimal Total { get; set; }
        [ProtoMember(2)]
        public decimal IDD { get; set; }
        [ProtoMember(3)]
        public decimal STD { get; set; }
        [ProtoMember(4)]
        public decimal MOB { get; set; }
    }
}
