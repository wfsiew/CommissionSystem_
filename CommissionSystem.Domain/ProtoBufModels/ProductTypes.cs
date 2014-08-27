using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace CommissionSystem.Domain.ProtoBufModels
{
    [ProtoContract]
    public class ProductTypes
    {
        [ProtoMember(1)]
        public int ProductID { get; set; }
        [ProtoMember(2)]
        public string Description { get; set; }
        [ProtoMember(3)]
        public decimal InitialAmount { get; set; }
        [ProtoMember(4)]
        public short ChargeType { get; set; }

        public bool IsRebate
        {
            get
            {
                bool a = false;

                if (!string.IsNullOrEmpty(Description) &&
                    Description.IndexOf("Rebate", StringComparison.OrdinalIgnoreCase) >= 0)
                    a = true;

                if (ChargeType == -1 && ProductID != 8)
                    a = true;

                return a;
            }
        }
    }
}
