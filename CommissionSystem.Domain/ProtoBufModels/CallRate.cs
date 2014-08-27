using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace CommissionSystem.Domain.ProtoBufModels
{
    [ProtoContract]
    public class CallRate
    {
        [ProtoMember(1)]
        private int idd;
        [ProtoMember(2)]
        private double std;
        [ProtoMember(3)]
        private double mob;
        [ProtoMember(4)]
        private double idd2;

        public int IDD
        {
            get
            {
                return idd;
            }

            set
            {
                idd = value;
            }
        }

        public double STD
        {
            get
            {
                return std;
            }

            set
            {
                std = value * 0.01;
            }
        }

        public double MOB
        {
            get
            {
                return mob;
            }

            set
            {
                mob = value * 0.01;
            }
        }

        public double IDD2
        {
            get
            {
                return idd2;
            }

            set
            {
                idd2 = value * 0.01;
            }
        }
    }
}
