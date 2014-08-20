using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CommissionSystem.Domain.ProtoBufModels
{
    public class Attachment
    {
        public string Filename { get; set; }
        public byte[] Data { get; set; }
    }

    public class EmailInfo
    {
        private IEnumerable<string> toList;
        private IEnumerable<string> ccList;
        private IEnumerable<string> bccList;
        private IEnumerable<Attachment> attList;

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

        public IEnumerable<Attachment> AttList
        {
            get
            {
                if (attList == null)
                    attList = new List<Attachment>();

                return attList;
            }

            set
            {
                attList = value;
            }
        }
    }
}