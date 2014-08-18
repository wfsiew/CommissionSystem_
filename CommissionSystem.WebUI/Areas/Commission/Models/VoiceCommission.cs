using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommissionSystem.WebUI.Models;
using CommissionSystem.WebUI.Helpers;
using CommissionSystem.Domain.Models;
using NLog;

namespace CommissionSystem.WebUI.Areas.Commission.Models
{
    public class VoiceCommission : IDisposable
    {
        public DbHelper Db { get; set; }
        public int AgentID { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public List<SalesParent> AgentList { get; set; }
        public Dictionary<int, List<SalesParent>> AgentDic { get; set; }
        public Dictionary<string, List<VoiceCommissionView>> CommissionViewDic { get; set; }
        public List<AgentView> AgentViewList { get; set; }

        private Regex IDDRegex { get; set; }
        private Regex STDMOBRegex { get; set; }
        private Regex STDRegex { get; set; }
        private Regex MOBRegex { get; set; }
        private Regex NumRegex { get; set; }

        private static Logger Logger = LogManager.GetCurrentClassLogger();

        public VoiceCommission()
        {
            IDDRegex = new Regex(@"IDD_\d+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            STDMOBRegex = new Regex(@"STDMOB\d+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            STDRegex = new Regex(@"STD\d+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            MOBRegex = new Regex(@"MOB\d+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            NumRegex = new Regex(@"\d+", RegexOptions.Compiled);

            Db = new DbHelper(DbHelper.GetConStr(Constants.CALLBILLING2));
        }

        public void Dispose()
        {
            if (Db != null)
                Db.Dispose();
        }

        protected List<Invoice> GetCustomerInvoice(Customer customer)
        {
            List<Invoice> l = new List<Invoice>();
            SqlDataReader rd = null;

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("select i.custid, i.invoicenumber, i.callcharge, i.callchargesidd, i.callchargesstd, i.callchargesmob, ")
                    .Append("i.totalcurrentcharge, i.realinvoicedate, csa.settlementidx from invoice i ")
                    .Append("left join customersettlementassigned csa on i.invoicenumber = csa.invoiceno ")
                    .Append("where i.custid = @custid and csa.settlementidx in (")
                    .Append("select settlementidx from customersettlement where custid = @custid and realdate >= @datefrom and realdate < @dateto)");
                string q = sb.ToString();
                SqlParameter p = new SqlParameter("@custid", SqlDbType.Int);
                p.Value = customer.CustID;
                Db.AddParameter(p);

                p = new SqlParameter("@datefrom", SqlDbType.DateTime);
                p.Value = DateFrom;
                Db.AddParameter(p);

                p = new SqlParameter("@dateto", SqlDbType.DateTime);
                p.Value = DateTo;
                Db.AddParameter(p);

                rd = Db.ExecuteReader(q, CommandType.Text);
                while (rd.Read())
                {
                    Invoice o = new Invoice();
                    o.CustID = rd.Get<int>("custid");
                    o.InvoiceNumber = rd.Get("invoicenumber");
                    o.CallCharge = rd.Get<decimal>("callcharge");
                    o.CallChargesIDD = rd.Get<decimal>("callchargesidd");
                    o.CallChargesSTD = rd.Get<decimal>("callchargesstd");
                    o.CallChargesMOB = rd.Get<decimal>("callchargesmob");
                    o.TotalCurrentCharge = rd.Get<decimal>("totalcurrentcharge");
                    o.InvoiceDate = rd.GetDateTime("realinvoicedate");
                    o.SettlementIdx = rd.Get<int>("settlementidx");

                    l.Add(o);
                }

                rd.Close();
            }

            catch (Exception e)
            {
                Logger.Debug("", e);
                throw e;
            }

            finally
            {
                if (rd != null)
                    rd.Dispose();
            }

            return l;
        }

        protected decimal GetCustomerSettlementAmount(Customer customer, List<Invoice> l, CallCharge c)
        {
            decimal amt = 0;
            SqlDataReader rd = null;
            Dictionary<string, bool> m = new Dictionary<string, bool>();

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("select settlementidx, custid, comment, amount, realdate, paymenttype, ")
                    .Append("reference, orno, paymentmode from customersettlement ")
                    .Append("where paymenttype <> 2 and custid = @custid and ")
                    .Append("realdate >= @datefrom and realdate < @dateto");
                string q = sb.ToString();
                SqlParameter p = new SqlParameter("@custid", SqlDbType.Int);
                p.Value = customer.CustID;
                Db.AddParameter(p);

                p = new SqlParameter("@datefrom", SqlDbType.DateTime);
                p.Value = DateFrom;
                Db.AddParameter(p);

                p = new SqlParameter("@dateto", SqlDbType.DateTime);
                p.Value = DateTo;
                Db.AddParameter(p);

                rd = Db.ExecuteReader(q, CommandType.Text);
                while (rd.Read())
                {
                    CustomerSettlement o = new CustomerSettlement();
                    o.SettlementIdx = rd.Get<int>("settlementidx");
                    o.CustID = rd.Get<int>("custid");
                    o.Comment = rd.Get("comment");
                    o.Amount = rd.Get<decimal>("amount");
                    o.RealDate = rd.GetDateTime("realdate");
                    o.PaymentType = rd.Get<int>("paymenttype");
                    o.Reference = rd.Get("reference");
                    o.ORNo = rd.Get("orno");
                    o.PaymentMode = rd.Get<int>("paymentmode");

                    var invoiceList = l.Where(x => x.SettlementIdx == o.SettlementIdx);

                    if (invoiceList.Count() > 0)
                    {
                        foreach (Invoice i in invoiceList)
                        {
                            DateTime dt = i.InvoiceDate.AddDays(Constants.MAX_INVOICE_DAY);
                            if (o.RealDate <= dt)
                            {
                                amt += o.Amount;
                                o.AddInvoice(i);
                                customer.AddSettlement(o);

                                if (!m.ContainsKey(i.InvoiceNumber))
                                {
                                    m[i.InvoiceNumber] = true;
                                    o.CallCharge += i.CallCharge;
                                    c.Total += i.CallCharge;
                                    c.IDD += i.CallChargesIDD;
                                    c.STD += i.CallChargesSTD;
                                    c.MOB += i.CallChargesMOB;
                                }
                            }
                        }
                    }
                }

                rd.Close();
            }

            catch (Exception e)
            {
                Logger.Debug("", e);
                throw e;
            }

            finally
            {
                if (rd != null)
                    rd.Dispose();
            }

            return amt;
        }

        protected Dictionary<int, ProductTypes> GetProductTypes()
        {
            Dictionary<int, ProductTypes> dic = new Dictionary<int, ProductTypes>();
            SqlDataReader rd = null;

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("select productid, description from producttypes");
                string q = sb.ToString();

                rd = Db.ExecuteReader(q, CommandType.Text);
                while (rd.Read())
                {
                    ProductTypes o = new ProductTypes();
                    o.ProductID = rd.Get<int>("productid");
                    o.Description = rd.Get("description");

                    dic[o.ProductID] = o;
                }

                rd.Close();
            }

            catch (Exception e)
            {
                Logger.Debug("", e);
                throw e;
            }

            finally
            {
                if (rd != null)
                    rd.Dispose();
            }

            return dic;
        }

        protected CallRate GetIDDSTDMOBRate(string s)
        {
            CallRate o = new CallRate();

            try
            {
                Match idd = IDDRegex.Match(s);
                Match stdmob = STDMOBRegex.Match(s);
                Match std = STDRegex.Match(s);
                Match mob = MOBRegex.Match(s);

                if (idd.Success)
                {
                    Match x = NumRegex.Match(idd.Value);
                    o.IDD = Utils.GetValue<int>(x.Value);
                }
                    
                if (stdmob.Success)
                {
                    Match x = NumRegex.Match(stdmob.Value);
                    o.STD = Utils.GetValue<int>(x.Value);
                    o.MOB = o.STD;
                }

                if (std.Success)
                {
                    Match x = NumRegex.Match(std.Value);
                    o.STD = Utils.GetValue<int>(x.Value);
                }
                    
                if (mob.Success)
                {
                    Match x = NumRegex.Match(mob.Value);
                    o.MOB = Utils.GetValue<int>(x.Value);
                }
            }

            catch (Exception e)
            {
                Logger.Debug("", e);
                throw e;
            }

            return o;
        }

        protected string GetRatePlanDescription(Customer customer)
        {
            string a = "";
            SqlDataReader rd = null;

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("select [desc] from masterrateplan where rateplanid = @rateplanid");
                string q = sb.ToString();
                SqlParameter p = new SqlParameter("@rateplanid", SqlDbType.Int);
                p.Value = customer.RatePlanID;
                Db.AddParameter(p);

                rd = Db.ExecuteReader(q, CommandType.Text);
                if (rd.Read())
                {
                    a = rd.Get("desc");
                }

                rd.Close();
            }


            catch (Exception e)
            {
                Logger.Debug("", e);
                throw e;
            }

            finally
            {
                if (rd != null)
                    rd.Dispose();
            }

            return a;
        }

        protected bool IsCustomerExist(int magentid, int custid)
        {
            bool a = false;
            SqlDataReader rd = null;

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("select top 1 custid from salesforcedetail where magentid = @magentid and custid = @custid");
                string q = sb.ToString();

                SqlParameter p = new SqlParameter("@magentid", magentid);
                p.Value = magentid;
                Db.AddParameter(p);

                p = new SqlParameter("@custid", SqlDbType.Int);
                p.Value = custid;
                Db.AddParameter(p);

                rd = Db.ExecuteReader(q, CommandType.Text);
                if (rd.Read())
                    a = true;

                rd.Close();
            }

            catch (Exception e)
            {
                Logger.Debug("", e);
                throw e;
            }

            finally
            {
                if (rd != null)
                    rd.Dispose();
            }

            return a;
        }
    }
}