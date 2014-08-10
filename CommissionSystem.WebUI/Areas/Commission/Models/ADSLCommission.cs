using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
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
                Queue<List<SalesParent>> qa = new Queue<List<SalesParent>>();
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
                        List<SalesParent> blist = a.ParentAgentList;
                        qa.Clear();

                        if (a.ParentAgentList.Count > 0)
                            qa.Enqueue(a.ParentAgentList);

                        AgentID = a.SParentID;
                        agentid = a.SParentID.ToString();

                        if (!cv.ContainsKey(agentid))
                            cv[agentid] = new List<CommissionView>();

                        if (!av.ContainsKey(agentid))
                            av[agentid] = a.GetAgentInfo();

                        Dictionary<int, Customer> customerDic = GetCustomers();
                        List<CustomerBillingInfo> customerBIlist = GetCustomerBillingInfos();

                        foreach (KeyValuePair<int, Customer> d in customerDic)
                        {
                            Customer customer = d.Value;
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
                                }
                            }

                            decimal amount = GetCustomerSettlementAmount(customer);
                            a.Amount += amount;

                            if (a.IsInternalData)
                            {
                                v.CommissionRate = sf.ADSLInternalSetting.Commission;
                                v.SettlementAmount += amount;

                                av[agentid].TotalSettlement += v.SettlementAmount;

                                v.Commission = sf.ADSLInternalSetting.GetDirectCommission(v.SettlementAmount);
                                av[agentid].TotalCommission += v.Commission;

                                for (int n = 1; n < 3; n++)
                                {
                                    blist = qa.Count > 0 ? qa.Dequeue() : null;
                                    if (blist == null)
                                        break;

                                    for (int z = 0; z < blist.Count; z++)
                                    {
                                        SalesParent b = blist[z];
                                        if (b.ParentAgentList.Count > 0)
                                            qa.Enqueue(b.ParentAgentList);

                                        bagentid = b.SParentID.ToString();
                                        bool parentExist = IsCustomerExist(b.SParentID, custID);

                                        if (!parentExist)
                                            continue;

                                        if (!cv.ContainsKey(bagentid))
                                            cv[bagentid] = new List<CommissionView>();

                                        CommissionView bv = new CommissionView();
                                        bv.Customer = customer;
                                        bv.Commission = sf.ADSLInternalSetting.GetCommission(v.SettlementAmount, n);
                                        bv.CommissionRate = sf.ADSLInternalSetting.GetCommissionRate(n);
                                        bv.SettlementAmount += v.SettlementAmount;
                                        cv[bagentid].Add(bv);

                                        if (!av.ContainsKey(bagentid))
                                            av[bagentid] = b.GetAgentInfo();

                                        av[bagentid].TotalSettlement += bv.SettlementAmount;
                                        av[bagentid].TotalCommission += bv.Commission;
                                    }
                                }
                            }

                            else
                            {
                                v.CommissionRate = sf.ADSLExternalSetting.Commission;
                                v.SettlementAmount += amount;

                                av[agentid].TotalSettlement += v.SettlementAmount;

                                v.Commission = sf.ADSLExternalSetting.GetDirectCommission(v.SettlementAmount);
                                av[agentid].TotalCommission += v.Commission;

                                for (int n = 1; n < 4; n++)
                                {
                                    blist = qa.Count > 0 ? qa.Dequeue() : null;
                                    if (blist == null)
                                        break;

                                    for (int z = 0; z < blist.Count; z++)
                                    {
                                        SalesParent b = blist[z];
                                        if (b.ParentAgentList.Count > 0)
                                            qa.Enqueue(b.ParentAgentList);

                                        bagentid = b.SParentID.ToString();
                                        bool parentExist = IsCustomerExist(b.SParentID, custID);

                                        if (!parentExist)
                                            continue;

                                        if (!cv.ContainsKey(bagentid))
                                            cv[bagentid] = new List<CommissionView>();

                                        CommissionView bv = new CommissionView();
                                        bv.Customer = customer;
                                        bv.Commission = sf.ADSLExternalSetting.GetCommission(v.SettlementAmount, n);
                                        bv.CommissionRate = sf.ADSLExternalSetting.GetCommissionRate(n);
                                        bv.SettlementAmount += v.SettlementAmount;
                                        cv[bagentid].Add(bv);

                                        if (!av.ContainsKey(bagentid))
                                            av[bagentid] = b.GetAgentInfo();

                                        av[bagentid].TotalSettlement += bv.SettlementAmount;
                                        av[bagentid].TotalCommission += bv.Commission;
                                    }
                                }
                            }

                            qa.Clear();

                            if (a.ParentAgentList.Count > 0)
                                qa.Enqueue(a.ParentAgentList);
                        }
                    }
                }

                CommissionViewDic = cv;
                AgentViewList = av.Values.OrderBy(x => x.AgentName).ToList();
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
                sb.Append("select custid, rental, productid, amount, realcommencementdate ")
                    .Append("from customerbillinginfo ")
                    .Append("where custid in (")
                    .Append("select custid from customer where status = 1 and custid in (")
                    .Append("select distinct custid from salesforcedetail where sfid = @sfid)) ");
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
                sb.Append("select custid, name from customer where agentid = @agentid and status = 1 and custid in (")
                    .Append("select custid from customerbillinginfo where productid in (")
                    .Append("select productid from producttypes where description like '%ADSL%'))");
                string q = sb.ToString();

                SqlParameter p = new SqlParameter("@agentid", SqlDbType.Int);
                p.Value = AgentID;
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

        private bool IsCustomerExist(int magentid, int custid)
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