using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace CommissionSystem.Domain.ProtoBufModels
{
    [ProtoContract]
    public class CustomerSettlement
    {
        public CustomerSettlement()
        {
            InvoiceList = new List<Invoice>();
            invoiceDic = new Dictionary<string, bool>();
        }

        [ProtoMember(1)]
        public int SettlementIdx { get; set; }
        [ProtoMember(2)]
        public int CustID { get; set; }
        [ProtoMember(3)]
        public string Comment { get; set; }
        [ProtoMember(4)]
        public decimal Amount { get; set; }
        [ProtoMember(5)]
        public DateTime RealDate { get; set; }
        [ProtoMember(6)]
        public int PaymentType { get; set; }
        [ProtoMember(7)]
        public string Reference { get; set; }
        [ProtoMember(8)]
        public string ORNo { get; set; }
        [ProtoMember(9)]
        public int PaymentMode { get; set; }
        [ProtoMember(10)]
        public decimal CallCharge { get; set; }
        [ProtoMember(11)]
        public List<Invoice> InvoiceList { get; private set; }
        private Dictionary<string, bool> invoiceDic;

        public void AddInvoice(Invoice o)
        {
            if (!invoiceDic.ContainsKey(o.InvoiceNumber))
            {
                invoiceDic[o.InvoiceNumber] = true;
                InvoiceList.Add(o);
            }
        }
    }
}
