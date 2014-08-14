using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CommissionSystem.WebUI.Helpers;

namespace CommissionSystem.WebUI.Areas.Commission.Models
{
    public class CallRate
    {
        private int idd;
        private double std;
        private double mob;

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
    }
}