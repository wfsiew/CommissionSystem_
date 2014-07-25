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
    public class FibrePlusCommission : IDisposable
    {
        public DbHelper Db { get; set; }
        public int AgentID { get; set; }
        public string AgentType { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public List<Agent> AgentList { get; set; }
        public Dictionary<int, List<Agent>> AgentDic { get; set; }

        private static Logger Logger = LogManager.GetCurrentClassLogger();

        public FibrePlusCommission()
        {
            Db = new DbHelper(DbHelper.GetConStr(Constants.HSBB_BILLING));
        }

        public void Dispose()
        {
            if (Db != null)
                Db.Dispose();
        }

        private Dictionary<string, object> GetSettlement()
        {
            decimal amt = 0;
            SqlDataReader rd = null;
            Dictionary<string, object> res = new Dictionary<string,object>();
            List<Settlement> l = new List<Settlement>();

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("select cs.custid, cs.comment, cs.amount, cs.realdate, cs.reference, cs.orno, c.name ")
                    .Append("from customersettlement cs ")
                    .Append("left join customer c on cs.custid = c.custid ")
                    .Append("where cs.productid = 0 and cs.paymenttype = 3 ")
                    .Append("and c.customertype = 1 and c.agentid = @agentid ")
                    .Append("and cs.realdate >= @fromdate and cs.realdate < @todate");
                string q = sb.ToString();

                SqlParameter p = new SqlParameter("@agentid", SqlDbType.Int);
                p.Value = AgentID;
                Db.AddParameter(p);

                p = new SqlParameter("fromdate", SqlDbType.DateTime);
                p.Value = DateFrom;
                Db.AddParameter(p);

                p = new SqlParameter("@todate", SqlDbType.DateTime);
                p.Value = DateTo;
                Db.AddParameter(p);

                rd = Db.ExecuteReader(q, CommandType.Text);
                while (rd.Read())
                {
                    Settlement o = new Settlement();
                    o.CustID = rd.Get<int>("custid");
                    o.Comment = rd.Get("comment");
                    o.Amount = rd.Get<decimal>("amount");
                    o.RealDate = rd.Get<DateTime>("realdate");
                    o.Reference = rd.Get("reference");
                    o.ORNo = rd.Get("orno");
                    o.CustName = rd.Get("name");

                    amt += o.Amount;
                    l.Add(o);
                }

                res["amount"] = amt;
                res["settlementlist"] = l;
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

            return res;
        }

        public void SetCommission()
        {
            decimal amt = 0;
            decimal comm = 0;
            SqlDataReader rd = null;

            try
            {
                List<int> levels = AgentDic.Keys.ToList();
                levels.Reverse();
                SettingFactory sf = SettingFactory.Instance;
                Dictionary<int, ProductTypes> productTypeDic = GetProductTypes();

                for (int i = 0; i < levels.Count; i++)
                {
                    int k = levels[i];
                    List<Agent> l = AgentDic[k];
                    for (int j = 0; j < l.Count; j++)
                    {
                        Agent a = l[j];
                        Agent b = a.ParentAgent;
                        AgentID = a.AgentID;
                        Dictionary<int, Customer> customerDic = GetCustomers();
                        List<CustomerBillingInfo> customerBIlist = GetCustomerBillingInfos();

                        foreach (KeyValuePair<int, Customer> d in customerDic)
                        {
                            Customer customer = d.Value;
                            int custID = d.Key;
                            IEnumerable<CustomerBillingInfo> ebi = customerBIlist.Where(x => x.CustID == custID);

                            foreach (CustomerBillingInfo bi in ebi)
                            {
                                if (productTypeDic.ContainsKey(bi.ProductID))
                                {
                                    ProductTypes productType = productTypeDic[bi.ProductID];
                                    decimal amount = GetCustomerSettlementAmount(customer, productType);
                                    amt += amount;
                                }
                            }
                        }

                        if (a.IsInternal)
                        {
                            a.DirectCommission = sf.FibrePlusInternalSetting.GetDirectCommission(amt);
                            a.CommissionRate = sf.FibrePlusInternalSetting.Commission;
                            if (b != null && b.Level > 0)
                            {
                                if (b.IsInternal)
                                {
                                    comm = sf.FibrePlusInternalSetting.GetCommission(amt, b.AgentType);
                                    b.AddToSubCommission(comm);
                                    b.TierCommissionRate = sf.FibrePlusInternalSetting.GetCommissionRate(b.AgentType);
                                }

                                else
                                {
                                    AgentID = b.AgentID;
                                    int numOfCustomers = GetNumOfCustomers();
                                    int type = FibrePlusExternal.GetCommissionType(numOfCustomers);
                                    comm = sf.FibrePlusExternalSetting[type].GetCommission(amt, b.AgentType);
                                    b.AddToSubCommission(comm);
                                    b.TierCommissionRate = sf.FibrePlusExternalSetting[type].GetCommissionRate(b.AgentType);
                                }
                            }
                        }

                        else
                        {
                            int numOfCustomers = GetNumOfCustomers();
                            int type = FibrePlusExternal.GetCommissionType(numOfCustomers);
                            a.DirectCommission = sf.FibrePlusExternalSetting[type].GetDirectCommission(amt);
                            a.CommissionRate = sf.FibrePlusExternalSetting[type].Commission;
                            if (b != null && b.Level > 0)
                            {
                                if (b.IsInternal)
                                {
                                    comm = sf.FibrePlusInternalSetting.GetCommission(amt, b.AgentType);
                                    b.AddToSubCommission(comm);
                                    b.TierCommissionRate = sf.FibrePlusInternalSetting.GetCommissionRate(b.AgentType);
                                }

                                else
                                {
                                    AgentID = b.AgentID;
                                    numOfCustomers = GetNumOfCustomers();
                                    type = FibrePlusExternal.GetCommissionType(numOfCustomers);
                                    comm = sf.FibrePlusExternalSetting[type].GetCommission(amt, b.AgentType);
                                    b.AddToSubCommission(comm);
                                    b.TierCommissionRate = sf.FibrePlusExternalSetting[type].GetCommissionRate(b.AgentType);
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

            finally
            {
                if (rd != null)
                    rd.Dispose();
            }
        }

        public void SetCommission_()
        {
            decimal amt = 0;
            decimal comm = 0;
            SqlDataReader rd = null;

            try
            {
                List<int> levels = AgentDic.Keys.ToList();
                levels.Reverse();
                SettingFactory sf = SettingFactory.Instance;

                for (int i = 0; i < levels.Count; i++)
                {
                    int k = levels[i];
                    List<Agent> l = AgentDic[k];
                    for (int j = 0; j < l.Count; j++)
                    {
                        Agent a = l[j];
                        Agent b = a.ParentAgent;
                        AgentID = a.AgentID;
                        amt = GetAmount();

                        if (a.IsInternal)
                        {
                            a.DirectCommission = sf.FibrePlusInternalSetting.GetDirectCommission(amt);
                            a.CommissionRate = sf.FibrePlusInternalSetting.Commission;
                            if (b != null && b.Level > 0)
                            {
                                if (b.IsInternal)
                                {
                                    comm = sf.FibrePlusInternalSetting.GetCommission(amt, b.AgentType);
                                    b.AddToSubCommission(comm);
                                    b.TierCommissionRate = sf.FibrePlusInternalSetting.GetCommissionRate(b.AgentType);
                                }
                                
                                else
                                {
                                    AgentID = b.AgentID;
                                    int numOfCustomers = GetNumOfCustomers();
                                    int type = FibrePlusExternal.GetCommissionType(numOfCustomers);
                                    comm = sf.FibrePlusExternalSetting[type].GetCommission(amt, b.AgentType);
                                    b.AddToSubCommission(comm);
                                    b.TierCommissionRate = sf.FibrePlusExternalSetting[type].GetCommissionRate(b.AgentType);
                                }
                            }
                        }

                        else
                        {
                            int numOfCustomers = GetNumOfCustomers();
                            int type = FibrePlusExternal.GetCommissionType(numOfCustomers);
                            a.DirectCommission = sf.FibrePlusExternalSetting[type].GetDirectCommission(amt);
                            a.CommissionRate = sf.FibrePlusExternalSetting[type].Commission;
                            if (b != null && b.Level > 0)
                            {
                                if (b.IsInternal)
                                {
                                    comm = sf.FibrePlusInternalSetting.GetCommission(amt, b.AgentType);
                                    b.AddToSubCommission(comm);
                                    b.TierCommissionRate = sf.FibrePlusInternalSetting.GetCommissionRate(b.AgentType);
                                }
                                
                                else
                                {
                                    AgentID = b.AgentID;
                                    numOfCustomers = GetNumOfCustomers();
                                    type = FibrePlusExternal.GetCommissionType(numOfCustomers);
                                    comm = sf.FibrePlusExternalSetting[type].GetCommission(amt, b.AgentType);
                                    b.AddToSubCommission(comm);
                                    b.TierCommissionRate = sf.FibrePlusExternalSetting[type].GetCommissionRate(b.AgentType);
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

            finally
            {
                if (rd != null)
                    rd.Dispose();
            }
        }

        private decimal GetAmount()
        {
            decimal amt = 0;
            SqlDataReader rd = null;

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("select sum(amount) as amount from customersettlement cs ")
                    .Append("left join customer c on cs.custid = c.custid ")
                    .Append("where cs.productid = 0 and cs.paymenttype = 3 ")
                    .Append("and c.customertype = 1 and c.agentid = @agentid ")
                    .Append("and cs.realdate >= @fromdate and cs.realdate < @todate");
                string q = sb.ToString();

                SqlParameter p = new SqlParameter("@agentid", SqlDbType.Int);
                p.Value = AgentID;
                Db.AddParameter(p);

                p = new SqlParameter("fromdate", SqlDbType.DateTime);
                p.Value = DateFrom;
                Db.AddParameter(p);

                p = new SqlParameter("@todate", SqlDbType.DateTime);
                p.Value = DateTo;
                Db.AddParameter(p);

                rd = Db.ExecuteReader(q, CommandType.Text);
                if (rd.Read())
                {
                    amt = rd.Get<decimal>("amount");
                }
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

        private decimal GetCustomerSettlementAmount(Customer customer, ProductTypes productType)
        {
            decimal amt = 0;
            decimal tmpamt = 0;
            SqlDataReader rd = null;

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("select settlementidx, custid, comment, amount, realdate, paymenttype, ")
                    .Append("reference, orno, paymentmode from customersettlement ")
                    .Append("where custid in ")
                    .Append("(select custid from customer where customertype = 1 and agentid = @agentid and custid = @custid) and ")
                    .Append("productid = 1 and paymenttype = 1 and custid = @custid ")
                    .Append("order by settlementidx");
                string q = sb.ToString();

                SqlParameter p = new SqlParameter("@agentid", SqlDbType.Int);
                p.Value = AgentID;
                Db.AddParameter(p);

                p = new SqlParameter("@custid", SqlDbType.Int);
                p.Value = customer.CustID;
                Db.AddParameter(p);

                bool first = true;
                rd = Db.ExecuteReader(q, CommandType.Text);
                for (int i = 0; rd.Read(); i++)
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

                    if (o.RealDate >= DateFrom && o.RealDate < DateTo)
                    {
                        if (first)
                        {
                            first = false;

                            if (o.Amount >= productType.InitialAmount)
                            {
                                amt = productType.InitialAmount;
                                break;
                            }

                            else
                            {
                                tmpamt += o.Amount;
                                continue;
                            }
                        }

                        else
                        {
                            if (tmpamt >= productType.InitialAmount)
                            {
                                amt = productType.InitialAmount;
                                break;
                            }

                            else
                            {
                                tmpamt += o.Amount;
                                continue;
                            }
                        }
                    }

                    else
                    {
                        tmpamt = 0;
                        first = false;
                        break;
                    }
                }
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
                sb.Append("select productid, description, initialamount from producttypes");
                string q = sb.ToString();

                rd = Db.ExecuteReader(q, CommandType.Text);
                while (rd.Read())
                {
                    ProductTypes o = new ProductTypes();
                    o.ProductID = rd.Get<int>("productid");
                    o.Description = rd.Get("description");
                    o.InitialAmount = rd.Get<decimal>("initialamount");

                    dic[o.ProductID] = o;
                }
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
                    .Append("(select custid from customer where customertype = 1 and agentid = @agentid) ")
                    .Append("and productid in ")
                    .Append("(select productid from producttypes where description like '%Mbps%' and ")
                    .Append("initialamount > 0)");
                string q = sb.ToString();

                SqlParameter p = new SqlParameter("@agentid", SqlDbType.Int);
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
                sb.Append("select custid, name, customertype from customer where customertype = 1 and agentid = @agentid");
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
                    o.CustomerType = rd.Get<int>("customertype");

                    dic[o.CustID] = o;
                }
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

        private int GetNumOfCustomers()
        {
            int i = 0;

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("select count(custid) from customer where customertype = 1 and agentid = @agentid");
                string q = sb.ToString();

                SqlParameter p = new SqlParameter("@agentid", SqlDbType.Int);
                p.Value = AgentID;
                Db.AddParameter(p);

                object o = Db.ExecuteScalar(q, CommandType.Text);
                if (o != null)
                    i = Utils.GetValue<int>(o.ToString());
            }

            catch (Exception e)
            {
                Logger.Debug("", e);
                throw e;
            }

            return i;
        }

        private bool IsExternal(string agentTeam)
        {
            bool a = false;

            if ("AG".Equals(agentTeam, StringComparison.OrdinalIgnoreCase) ||
                "AGT".Equals(agentTeam, StringComparison.OrdinalIgnoreCase))
                a = true;

            return a;
        }
    }
}