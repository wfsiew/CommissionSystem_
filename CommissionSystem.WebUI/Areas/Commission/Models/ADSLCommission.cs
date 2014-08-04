using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using CommissionSystem.WebUI.Models;
using CommissionSystem.WebUI.Helpers;
using CommissionSystem.Domain.Models;
using NLog;

namespace CommissionSystem.WebUI.Areas.Commission.Models
{
    public class ADSLCommission : IDisposable
    {
        public DbHelper Db { get; set; }
        public int SParentID { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public List<SalesParent> SalesParentList { get; set; }

        private static Logger Logger = LogManager.GetCurrentClassLogger();

        public ADSLCommission()
        {
            Db = new DbHelper(DbHelper.GetConStr(Constants.RTCBROADBAND_CALLBILLING));
        }

        public void SetCommission()
        {
            try
            {
                SettingFactory sf = SettingFactory.Instance;
                Dictionary<int, ProductTypes> productTypeDic = GetProductTypes();

                List<SalesParent> l = SalesParentList;
                for (int i = 0; i < l.Count; i++)
                {
                    SalesParent a = l[i];
                    SParentID = a.SParentID;
                    Dictionary<int, Customer> customerDic = GetCustomers();
                    List<CustomerBillingInfo> customerBIlist = GetCustomerBillingInfos();

                    foreach (KeyValuePair<int, Customer> d in customerDic)
                    {
                        Customer customer = d.Value;
                        int custID = d.Key;
                        List<CustomerBillingInfo> ebi = customerBIlist.Where(x => x.CustID == custID).ToList();

                        customer.BillingInfoList = ebi;
                        a.AddCustomer(customer);

                        foreach (CustomerBillingInfo bi in ebi)
                        {
                            if (productTypeDic.ContainsKey(bi.ProductID))
                            {
                                ProductTypes productType = productTypeDic[bi.ProductID];
                                bi.ProductType = productType;
                                decimal amount = GetCustomerSettlementAmount(customer);
                                a.Amount += amount;
                            }
                        }
                    }

                    if (a.IsInternalData)
                    {
                        a.DirectCommission = sf.ADSLInternalSetting.GetDirectCommission(a.Amount);
                        a.CommissionRate = sf.ADSLInternalSetting.Commission;
                    }

                    else
                    {
                        a.DirectCommission = sf.ADSLExternalSetting.GetDirectCommission(a.Amount);
                        a.CommissionRate = sf.ADSLExternalSetting.Commission;
                    }
                }
            }

            catch (Exception e)
            {
                Logger.Debug("", e);
                throw e;
            }
        }

        public void Dispose()
        {
            if (Db != null)
                Db.Dispose();
        }

        private decimal GetCustomerSettlementAmount(Customer customer)
        {
            decimal amt = 0;
            SqlDataReader rd = null;

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("select settlementidx, custid, comment, amount, realdate, paymenttype, ")
                    .Append("reference, orno, paymentmode from customersettlement ")
                    .Append("where paymenttype = 1 and custid = @custid and ")
                    .Append("productid <> 0 and paymenttype <> 3 and productid <> 41 and ")
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

                    amt += o.Amount;
                    customer.AddSettlement(o);
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

        private Dictionary<int, ProductTypes> GetProductTypes()
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

        private List<CustomerBillingInfo> GetCustomerBillingInfos()
        {
            List<CustomerBillingInfo> l = new List<CustomerBillingInfo>();
            SqlDataReader rd = null;

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("select custid, rental, productid, amount, realcommencementdate, realcommencementenddate ")
                    .Append("from customerbillinginfo ")
                    .Append("where custid in ")
                    .Append("(select custid from customer where status = 1 and agentid = @agentid) ")
                    .Append("and productid in ")
                    .Append("(select productid from producttypes where description like '%ADSL%')");
                string q = sb.ToString();

                SqlParameter p = new SqlParameter("@agentid", SqlDbType.Int);
                p.Value = SParentID;
                Db.AddParameter(p);

                rd = Db.ExecuteReader(q, CommandType.Text);
                while (rd.Read())
                {
                    CustomerBillingInfo o = new CustomerBillingInfo();
                    o.CustID = rd.Get<int>("custid");
                    o.Rental = rd.Get<decimal>("rental");
                    o.ProductID = rd.Get<int>("productid");
                    o.Amount = rd.Get<decimal>("amount");
                    o.RealCommencementDate = rd.GetDateTime("realcommencementdate");
                    o.RealCommencementEndDate = rd.GetDateTime("realcommencementenddate");

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

        private Dictionary<int, Customer> GetCustomers()
        {
            Dictionary<int, Customer> dic = new Dictionary<int, Customer>();
            SqlDataReader rd = null;

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("select custid, name from customer where status = 1 and agentid = @agentid and custid in (")
                    .Append("select custid from customerbillinginfo where productid in (")
                    .Append("select productid from producttypes where description like '%ADSL%'))");
                string q = sb.ToString();

                SqlParameter p = new SqlParameter("@agentid", SqlDbType.Int);
                p.Value = SParentID;
                Db.AddParameter(p);

                rd = Db.ExecuteReader(q, CommandType.Text);
                while (rd.Read())
                {
                    Customer o = new Customer();
                    o.CustID = rd.Get<int>("custid");
                    o.Name = rd.Get("name");

                    dic[o.CustID] = o;
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
    }
}