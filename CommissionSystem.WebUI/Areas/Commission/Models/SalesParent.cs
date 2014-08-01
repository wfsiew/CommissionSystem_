using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommissionSystem.WebUI.Areas.Commission.Models
{
    public class SalesParent
    {
        public SalesParent()
        {
            CustomerList = new List<Customer>();
        }

        public int SParentID { get; set; }
        public string SParentName { get; set; }
        public string GeographyCode { get; set; }
        public int RptParentID { get; set; }
        public decimal Amount { get; set; }
        public decimal DirectCommission { get; set; }
        public decimal SubCommission { get; set; }
        public double CommissionRate { get; set; }
        public double TierCommissionRate { get; set; }
        public List<Customer> CustomerList { get; private set; }

        public void AddCustomer(Customer o)
        {
            CustomerList.Add(o);
        }

        public decimal TotalCommission
        {
            get
            {
                return DirectCommission + SubCommission;
            }
        }

        public bool IsInternal
        {
            get
            {
                bool a = false;
                string id = SParentID.ToString();

                if (id.IndexOf("881") == 0)
                    a = true;

                return a;
            }
        }
    }
}