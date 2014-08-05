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
    public class SpeedPlusCommission : IDisposable
    {
        public DbHelper Db { get; set; }
        public int AgentID { get; set; }
        public string AgentType { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public List<Agent> AgentList { get; set; }
        public Dictionary<int, List<Agent>> AgentDic { get; set; }

        private static Logger Logger = LogManager.GetCurrentClassLogger();

        public SpeedPlusCommission()
        {
            Db = new DbHelper(DbHelper.GetConStr(Constants.HSBB_BILLING));
        }

        public void SetCommission()
        {
            decimal comm = 0;

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
                            List<CustomerBillingInfo> ebi = customerBIlist.Where(x => x.CustID == custID).ToList();

                            customer.BillingInfoList = ebi;
                            a.AddCustomer(customer);

                            foreach (CustomerBillingInfo bi in ebi)
                            {
                                if (productTypeDic.ContainsKey(bi.ProductID))
                                {
                                    ProductTypes productType = productTypeDic[bi.ProductID];
                                    bi.ProductType = productType;
                                    decimal amount = GetCustomerSettlementAmount(customer, productType);
                                    a.Amount += amount;
                                }
                            }
                        }

                        if (a.IsInternal)
                        {
                            a.DirectCommission = sf.SpeedPlusInternalSetting.GetDirectCommission(a.Amount);
                            a.CommissionRate = sf.SpeedPlusInternalSetting.Commission;
                            if (b != null && b.Level > 0)
                            {
                                comm = sf.SpeedPlusInternalSetting.GetCommission(a.Amount, b.AgentType);
                                b.AddToSubCommission(comm);
                                b.TierCommissionRate = sf.SpeedPlusInternalSetting.GetCommissionRate(b.AgentType);

                                //if (b.IsInternal)
                                //{
                                //    comm = sf.SpeedPlusInternalSetting.GetCommission(a.Amount, b.AgentType);
                                //    b.AddToSubCommission(comm);
                                //    b.TierCommissionRate = sf.SpeedPlusInternalSetting.GetCommissionRate(b.AgentType);
                                //}

                                //else
                                //{
                                //    AgentID = b.AgentID;
                                //    int numOfCustomers = GetNumOfCustomers();
                                //    int type = SpeedPlusExternal.GetCommissionType(numOfCustomers);
                                //    comm = sf.SpeedPlusExternalSetting[type].GetCommission(a.Amount, b.AgentType);
                                //    b.AddToSubCommission(comm);
                                //    b.TierCommissionRate = sf.SpeedPlusExternalSetting[type].GetCommissionRate(b.AgentType);
                                //}
                            }
                        }

                        else
                        {
                            int numOfCustomers = GetNumOfCustomers();
                            int type = SpeedPlusExternal.GetCommissionType(numOfCustomers);
                            a.DirectCommission = sf.SpeedPlusExternalSetting[type].GetDirectCommission(a.Amount);
                            a.CommissionRate = sf.SpeedPlusExternalSetting[type].Commission;
                            if (b != null && b.Level > 0)
                            {
                                AgentID = b.AgentID;
                                numOfCustomers = GetNumOfCustomers();
                                type = SpeedPlusExternal.GetCommissionType(numOfCustomers);
                                comm = sf.SpeedPlusExternalSetting[type].GetCommission(a.Amount, b.AgentType);
                                b.AddToSubCommission(comm);
                                b.TierCommissionRate = sf.SpeedPlusExternalSetting[type].GetCommissionRate(b.AgentType);

                                //if (b.IsInternal)
                                //{
                                //    comm = sf.SpeedPlusInternalSetting.GetCommission(a.Amount, b.AgentType);
                                //    b.AddToSubCommission(comm);
                                //    b.TierCommissionRate = sf.SpeedPlusInternalSetting.GetCommissionRate(b.AgentType);
                                //}

                                //else
                                //{
                                //    AgentID = b.AgentID;
                                //    numOfCustomers = GetNumOfCustomers();
                                //    type = SpeedPlusExternal.GetCommissionType(numOfCustomers);
                                //    comm = sf.SpeedPlusExternalSetting[type].GetCommission(a.Amount, b.AgentType);
                                //    b.AddToSubCommission(comm);
                                //    b.TierCommissionRate = sf.SpeedPlusExternalSetting[type].GetCommissionRate(b.AgentType);
                                //}
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
                            a.DirectCommission = sf.SpeedPlusInternalSetting.GetDirectCommission(amt);
                            a.CommissionRate = sf.SpeedPlusInternalSetting.Commission;
                            if (b != null && b.Level > 0)
                            {
                                if (b.IsInternal)
                                {
                                    comm = sf.SpeedPlusInternalSetting.GetCommission(amt, b.AgentType);
                                    b.AddToSubCommission(comm);
                                    b.TierCommissionRate = sf.SpeedPlusInternalSetting.GetCommissionRate(b.AgentType);
                                }

                                else
                                {
                                    AgentID = b.AgentID;
                                    int numOfCustomers = GetNumOfCustomers();
                                    int type = SpeedPlusExternal.GetCommissionType(numOfCustomers);
                                    comm = sf.SpeedPlusExternalSetting[type].GetCommission(amt, b.AgentType);
                                    b.AddToSubCommission(comm);
                                    b.TierCommissionRate = sf.SpeedPlusExternalSetting[type].GetCommissionRate(b.AgentType);
                                }
                            }
                        }

                        else
                        {
                            int numOfCustomers = GetNumOfCustomers();
                            int type = SpeedPlusExternal.GetCommissionType(numOfCustomers);
                            a.DirectCommission = sf.SpeedPlusExternalSetting[type].GetDirectCommission(amt);
                            a.CommissionRate = sf.SpeedPlusExternalSetting[type].Commission;
                            if (b != null && b.Level > 0)
                            {
                                if (b.IsInternal)
                                {
                                    comm = sf.SpeedPlusInternalSetting.GetCommission(amt, b.AgentType);
                                    b.AddToSubCommission(comm);
                                    b.TierCommissionRate = sf.SpeedPlusInternalSetting.GetCommissionRate(b.AgentType);
                                }

                                else
                                {
                                    AgentID = b.AgentID;
                                    numOfCustomers = GetNumOfCustomers();
                                    type = SpeedPlusExternal.GetCommissionType(numOfCustomers);
                                    comm = sf.SpeedPlusExternalSetting[type].GetCommission(amt, b.AgentType);
                                    b.AddToSubCommission(comm);
                                    b.TierCommissionRate = sf.SpeedPlusExternalSetting[type].GetCommissionRate(b.AgentType);
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

        public void Dispose()
        {
            if (Db != null)
                Db.Dispose();
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
                    .Append("and c.customertype in (2, 3) and c.status = 1 and c.agentid = @agentid ")
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
                    .Append("(select custid from customer where customertype in (2, 3) and status = 1 and agentid = @agentid and custid = @custid) and ")
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

                    if (o.RealDate >= DateFrom && o.RealDate < DateTo)
                    {
                        if (first)
                        {
                            first = false;

                            if (!string.IsNullOrEmpty(productType.Description) &&
                                productType.Description.IndexOf("Rebate", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                amt = productType.InitialAmount * -1;
                                break;
                            }

                            if (o.Amount >= productType.InitialAmount)
                            {
                                amt = productType.InitialAmount;
                                customer.AddSettlement(o);
                                break;
                            }

                            else
                            {
                                tmpamt += o.Amount;
                                customer.AddSettlement(o);
                                continue;
                            }
                        }

                        else
                        {
                            if (tmpamt >= productType.InitialAmount)
                            {
                                amt = productType.InitialAmount;
                                customer.AddSettlement(o);
                                break;
                            }

                            else
                            {
                                tmpamt += o.Amount;
                                customer.AddSettlement(o);
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
                    .Append("(select custid from customer where customertype in (2, 3) and status = 1 and agentid = @agentid) ")
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
                sb.Append("select custid, name, customertype from customer where customertype in (2, 3) and status = 1 and agentid = @agentid ")
                    .Append("order by name");
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

        private int GetNumOfCustomers()
        {
            int i = 0;

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("select count(custid) from customer where customertype in (2, 3) and status = 1 and agentid = @agentid");
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
    }
}