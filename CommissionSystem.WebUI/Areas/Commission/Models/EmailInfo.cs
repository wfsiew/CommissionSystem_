using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommissionSystem.WebUI.Areas.Commission.Models
{
    public class EmailInfo
    {
        private IEnumerable<string> toList;
        private IEnumerable<string> ccList;
        private IEnumerable<string> bccList;

        public string DisplayName { get; set; }
        public string Subject { get; set; }

        public IEnumerable<string> ToList
        {
            get
            {
                if (toList == null)
                    toList = new List<string>();

                return toList;
            }

            set
            {
                toList = value;
            }
        }

        public IEnumerable<string> CcList
        {
            get
            {
                if (ccList == null)
                    ccList = new List<string>();

                return ccList;
            }

            set
            {
                ccList = value;
            }
        }

        public IEnumerable<string> BccList
        {
            get
            {
                if (bccList == null)
                    bccList = new List<string>();

                return bccList;
            }

            set
            {
                bccList = value;
            }
        }
    }
}