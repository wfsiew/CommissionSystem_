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
        public int AgentID { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public List<SalesParent> AgentList { get; set; }
        public Dictionary<int, List<SalesParent>> AgentDic { get; set; }
        public Dictionary<string, List<CommissionView>> CommissionViewDic { get; set; }
        public List<AgentView> AgentViewList { get; set; }

        private static Logger Logger = LogManager.GetCurrentClassLogger();

        public ADSLCommission()
        {
            Db = new DbHelper(DbHelper.GetConStr(Constants.RTCBROADBAND_CALLBILLING));
        }

        public void SetCommission()
        {
            string agentid = null;
            string bagentid = null;

            try
            {
                Dictionary<string, List<CommissionView>> cv = new Dictionary<string, List<CommissionView>>();
                Dictionary<string, AgentView> av = new Dictionary<string, AgentView>();
                List<int> levels = AgentDic.Keys.ToList();
                levels.Reverse();
                SettingFactory sf = SettingFactory.Instance;
                Dictionary<int, ProductTypes> productTypeDic = GetProductTypes();

                for (int i = 0; i < levels.Count; i++)
                {
                    int k = levels[i];
                    List<SalesParent> l = AgentDic[k];
                    for (int j = 0; j < l.Count; j++)
                    {
                        SalesParent a = l[j];
                        SalesParent b = a.ParentAgent;
                        AgentID = a.SParentID;
                        agentid = a.SParentID.ToString();

                        if (!cv.ContainsKey(agentid))
                            cv[agentid] = new List<CommissionView>();

                        if (!av.ContainsKey(agentid))
                            av[agentid] = a.GetAgentInfo();

                        Dictionary<int, ADSLCustomer> customerDic = GetCustomers();
                        List<CustomerBillingInfo> customerBIlist = GetCustomerBillingInfos();

                        foreach (KeyValuePair<int, ADSLCustomer> d in customerDic)
                        {
                            ADSLCustomer customer = d.Value;
                            int custID = d.Key;
                            List<CustomerBillingInfo> ebi = customerBIlist.Where(x => x.CustID == custID).ToList();

                            customer.BillingInfoList = ebi;
                            a.AddCustomer(customer);

                            CommissionView v = new CommissionView();
                            v.Customer = customer;
                            cv[agentid].Add(v);

                            foreach (CustomerBillingInfo bi in ebi)
                            {
                                if (productTypeDic.ContainsKey(bi.ProductID))
                                {
                                    ProductTypes productType = productTypeDic[bi.ProductID];
                                    bi.ProductType = productType;
                                    decimal amount = GetCustomerSettlementAmount(customer);
                                    a.Amount += amount;

                                    if (a.IsInternalData)
                                    {
                                        v.CommissionRate = sf.ADSLInternalSetting.Commission;
                                        v.SettlementAmount += amount;

                                        av[agentid].TotalSettlement += v.SettlementAmount;
                                    }

                                    else
                                    {
                                        v.CommissionRate = sf.ADSLExternalSetting.Commission;
                                        v.SettlementAmount += amount;

                                        av[agentid].TotalSettlement += v.SettlementAmount;
                                    }
                                }
                            }

                            if (a.IsInternalData)
                            {
                                v.Commission = sf.ADSLInternalSetting.GetDirectCommission(v.SettlementAmount);
                                av[agentid].TotalCommission += v.Commission;

                                if (b != null && customer.MasterAgentID != 0)
                                {
                                    bagentid = b.SParentID.ToString();

                                    if (!cv.ContainsKey(bagentid))
                                        cv[bagentid] = new List<CommissionView>();

                                    CommissionView bv = new CommissionView();
                                    bv.Customer = customer;
                                    bv.Commission = sf.ADSLInternalSetting.GetCommission(v.SettlementAmount, 0);
                                }
                            }
                        }
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
                    .Append("where custid in (")
                    .Append("select custid from customer where status = 1 and custid in (")
                    .Append("select distinct custid from salesforcedetail where sfid = @sfid)) ")
                    .Append("and productid in (")
                    .Append("select productid from producttypes where description like '%ADSL%')");
                string q = sb.ToString();

                SqlParameter p = new SqlParameter("@sfid", SqlDbType.Int);
                p.Value = AgentID;
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

        private Dictionary<int, ADSLCustomer> GetCustomers()
        {
            Dictionary<int, ADSLCustomer> dic = new Dictionary<int, ADSLCustomer>();
            SqlDataReader rd = null;

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("select distinct c.custid, c.name, sf.magentid from customer c ")
                    .Append("left join salesforcedetail sf on c.custid = sf.custid ")
                    .Append("where c.status = 1 and sf.sfid = @sfid and c.custid in (")
                    .Append("select custid from customerbillinginfo where productid in (")
                    .Append("select productid from producttypes where description like '%ADSL%'))");
                string q = sb.ToString();

                SqlParameter p = new SqlParameter("@sfid", SqlDbType.Int);
                p.Value = AgentID;
                Db.AddParameter(p);

                rd = Db.ExecuteReader(q, CommandType.Text);
                while (rd.Read())
                {
                    ADSLCustomer o = new ADSLCustomer();
                    o.CustID = rd.Get<int>("custid");
                    o.Name = rd.Get("name");
                    o.MasterAgentID = rd.Get<int>("magentid");

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