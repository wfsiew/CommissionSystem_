using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace CommissionSystem.Domain.ProtoBufModels
{
    [ProtoContract]
    public class AgentView
    {
        [ProtoMember(1)]
        public int AgentID { get; set; }
        [ProtoMember(2)]
        public string AgentName { get; set; }
        [ProtoMember(3)]
        public string AgentType { get; set; }
        [ProtoMember(4)]
        public string AgentLevel { get; set; }
        [ProtoMember(5)]
        public string AgentTeam { get; set; }
        [ProtoMember(6)]
        public int Level { get; set; }
        [ProtoMember(7)]
        public decimal TotalSettlement { get; set; }
        [ProtoMember(8)]
        public decimal TotalCommission { get; set; }
    }
}
