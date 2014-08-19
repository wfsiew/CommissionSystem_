using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommissionSystem.Task.Models
{
    public class ProductTypes
    {
        public int ProductID { get; set; }
        public string Description { get; set; }
        public decimal InitialAmount { get; set; }

        public bool IsRebate
        {
            get
            {
                bool a = false;

                if (!string.IsNullOrEmpty(Description) &&
                    Description.IndexOf("Rebate", StringComparison.OrdinalIgnoreCase) >= 0)
                    a = true;

                return a;
            }
        }
    }
}
