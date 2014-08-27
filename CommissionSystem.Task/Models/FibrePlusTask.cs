using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using CommissionSystem.Domain.Models;
using CommissionSystem.Domain.ProtoBufModels;
using CommissionSystem.Domain.Helpers;
using CommissionSystem.Task.Helpers;
using NLog;

namespace CommissionSystem.Task.Models
{
    public class FibrePlusTask : IDisposable
    {
        public DbHelper Db { get; set; }
        public int AgentID { get; set; }
        public string AgentType { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public List<Agent> AgentList { get; set; }
        public Dictionary<int, List<Agent>> AgentDic { get; set; }
        public Dictionary<string, List<CommissionView>> CommissionViewDic { get; set; }
        public List<AgentView> AgentViewList { get; set; }

        private static Logger Logger = LogManager.GetCurrentClassLogger();

        public FibrePlusTask()
        {
            Db = new DbHelper(DbHelper.GetConStr(Constants.HSBB_BILLING));
        }

        public void Dispose()
        {
            if (Db != null)
                Db.Dispose();
        }

        public void Run()
        {
            string agentid = null;
            string bagentid = null;

            try
            {
                Dictionary<int, List<Agent>> dic = new Dictionary<int, List<Agent>>();
                List<Agent> agentlist = new List<Agent>();
                GetAgentHierarchy(0, agentlist, dic);

                AgentDic = dic;
                AgentList = agentlist;

                Dictionary<string, List<CommissionView>> cv = new Dictionary<string, List<CommissionView>>();
                Dictionary<string, AgentView> av = new Dictionary<string, AgentView>();
                Queue<Agent> qa = new Queue<Agent>();
                List<int> levels = AgentDic.Keys.ToList();
                levels.Reverse();
                SettingFactory sf = SettingFactory.Instance;
                Dictionary<int, ProductTypes> productTypeDic = GetProductTypes();
                Dictionary<int, int> numCustDic = new Dictionary<int, int>();
                Dictionary<string, bool> t = new Dictionary<string, bool>();
                Dictionary<int, bool> ad = new Dictionary<int, bool>();

                for (int i = 0; i < levels.Count; i++)
                {
                    int k = levels[i];
                    List<Agent> l = AgentDic[k];
                    for (int j = 0; j < l.Count; j++)
                    {
                        Agent a = l[j];
                        Agent b = a.ParentAgent;
                        AgentID = a.AgentID;
                        agentid = a.AgentID.ToString();

                        if (!cv.ContainsKey(agentid))
                            cv[agentid] = new List<CommissionView>();

                        if (!av.ContainsKey(agentid))
                            av[agentid] = a.GetAgentInfo();

                        if (ad.ContainsKey(AgentID))
                            continue;

                        Dictionary<int, Customer> customerDic = GetCustomers();
                        List<CustomerBillingInfo> customerBIlist = GetCustomerBillingInfos();

                        foreach (KeyValuePair<int, Customer> d in customerDic)
                        {
                            string uid = string.Format("{0}-{1}", a.AgentID, d.Key);
                            if (t.ContainsKey(uid))
                                break;

                            else
                                t[uid] = true;

                            qa.Clear();

                            if (a.ParentAgent != null)
                                qa.Enqueue(a.ParentAgent);

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
                                    decimal amount = GetCustomerSettlementAmount(customer, productType);
                                    a.Amount += amount;

                                    if (a.IsInternal)
                                    {
                                        v.CommissionRate = sf.FibrePlusInternalSetting.Commission;
                                        v.SettlementAmount += amount;

                                        av[agentid].TotalSettlement += v.SettlementAmount;
                                    }

                                    else
                                    {
                                        if (!numCustDic.ContainsKey(a.AgentID))
                                            numCustDic[a.AgentID] = GetNumOfCustomers();

                                        int numOfCustomers = numCustDic[a.AgentID];
                                        int type = FibrePlusExternal.GetCommissionType(numOfCustomers);
                                        v.CommissionRate = sf.FibrePlusExternalSetting[type].Commission;
                                        v.SettlementAmount += amount;

                                        av[agentid].TotalSettlement += v.SettlementAmount;
                                    }
                                }
                            }

                            if (a.IsInternal)
                            {
                                v.Commission = sf.FibrePlusInternalSetting.GetDirectCommission(v.SettlementAmount);
                                av[agentid].TotalCommission += v.Commission;

                                for (int n = 1; n < 3; n++)
                                {
                                    b = qa.Count() > 0 ? qa.Dequeue() : null;
                                    if (b == null)
                                        break;

                                    if (b.ParentAgent != null)
                                        qa.Enqueue(b.ParentAgent);

                                    bagentid = b.AgentID.ToString();

                                    if (!cv.ContainsKey(bagentid))
                                        cv[bagentid] = new List<CommissionView>();

                                    CommissionView bv = new CommissionView();
                                    bv.Customer = customer;
                                    bv.Commission = sf.FibrePlusInternalSetting.GetCommission(v.SettlementAmount, n);
                                    bv.CommissionRate = sf.FibrePlusInternalSetting.GetCommissionRate(n);
                                    bv.SettlementAmount += v.SettlementAmount;
                                    cv[bagentid].Add(bv);

                                    if (!av.ContainsKey(bagentid))
                                        av[bagentid] = b.GetAgentInfo();

                                    av[bagentid].TotalSettlement += bv.SettlementAmount;
                                    av[bagentid].TotalCommission += bv.Commission;
                                }
                            }

                            else
                            {
                                if (!numCustDic.ContainsKey(a.AgentID))
                                    numCustDic[a.AgentID] = GetNumOfCustomers();

                                int numOfCustomers = numCustDic[a.AgentID];
                                int type = FibrePlusExternal.GetCommissionType(numOfCustomers);
                                v.Commission = sf.FibrePlusExternalSetting[type].GetDirectCommission(v.SettlementAmount);
                                av[agentid].TotalCommission += v.Commission;

                                for (int n = 1; n < 4; n++)
                                {
                                    b = qa.Count() > 0 ? qa.Dequeue() : null;
                                    if (b == null)
                                        break;

                                    if (b.ParentAgent != null)
                                        qa.Enqueue(b.ParentAgent);

                                    bagentid = b.AgentID.ToString();

                                    if (!cv.ContainsKey(bagentid))
                                        cv[bagentid] = new List<CommissionView>();

                                    CommissionView bv = new CommissionView();
                                    type = FibrePlusExternal.GetCommissionType(numOfCustomers);
                                    bv.Customer = customer;
                                    bv.Commission = sf.FibrePlusExternalSetting[type].GetCommission(v.SettlementAmount, n);
                                    bv.CommissionRate = sf.FibrePlusExternalSetting[type].GetCommissionRate(n);
                                    bv.SettlementAmount += v.SettlementAmount;
                                    cv[bagentid].Add(bv);

                                    if (!av.ContainsKey(bagentid))
                                        av[bagentid] = b.GetAgentInfo();

                                    av[bagentid].TotalSettlement += bv.SettlementAmount;
                                    av[bagentid].TotalCommission += bv.Commission;
                                }
                            }
                        }

                        //if (a.IsInternal)
                        //{
                        //    a.DirectCommission = sf.FibrePlusInternalSetting.GetDirectCommission(a.Amount);
                        //    a.CommissionRate = sf.FibrePlusInternalSetting.Commission;
                        //    if (b != null && b.Level > 0)
                        //    {
                        //        comm = sf.FibrePlusInternalSetting.GetCommission(a.Amount, b.AgentType);
                        //        rate = sf.FibrePlusInternalSetting.GetCommissionRate(b.AgentType);
                        //        b.AddToSubCommission(comm);

                        //        //if (b.IsInternal)
                        //        //{
                        //        //    comm = sf.FibrePlusInternalSetting.GetCommission(a.Amount, b.AgentType);
                        //        //    b.AddToSubCommission(comm);
                        //        //    b.TierCommissionRate = sf.FibrePlusInternalSetting.GetCommissionRate(b.AgentType);
                        //        //}

                        //        //else
                        //        //{
                        //        //    AgentID = b.AgentID;
                        //        //    int numOfCustomers = GetNumOfCustomers();
                        //        //    int type = FibrePlusExternal.GetCommissionType(numOfCustomers);
                        //        //    comm = sf.FibrePlusExternalSetting[type].GetCommission(a.Amount, b.AgentType);
                        //        //    b.AddToSubCommission(comm);
                        //        //    b.TierCommissionRate = sf.FibrePlusExternalSetting[type].GetCommissionRate(b.AgentType);
                        //        //}
                        //    }
                        //}

                        //else
                        //{
                        //    int numOfCustomers = GetNumOfCustomers();
                        //    int type = FibrePlusExternal.GetCommissionType(numOfCustomers);
                        //    a.DirectCommission = sf.FibrePlusExternalSetting[type].GetDirectCommission(a.Amount);
                        //    a.CommissionRate = sf.FibrePlusExternalSetting[type].Commission;
                        //    if (b != null && b.Level > 0)
                        //    {
                        //        type = FibrePlusExternal.GetCommissionType(numOfCustomers);
                        //        comm = sf.FibrePlusExternalSetting[type].GetCommission(a.Amount, b.AgentType);
                        //        rate = sf.FibrePlusExternalSetting[type].GetCommissionRate(b.AgentType);
                        //        b.AddToSubCommission(comm);

                        //        //if (b.IsInternal)
                        //        //{
                        //        //    comm = sf.FibrePlusInternalSetting.GetCommission(a.Amount, b.AgentType);
                        //        //    b.AddToSubCommission(comm);
                        //        //    b.TierCommissionRate = sf.FibrePlusInternalSetting.GetCommissionRate(b.AgentType);
                        //        //}

                        //        //else
                        //        //{
                        //        //    AgentID = b.AgentID;
                        //        //    numOfCustomers = GetNumOfCustomers();
                        //        //    type = FibrePlusExternal.GetCommissionType(numOfCustomers);
                        //        //    comm = sf.FibrePlusExternalSetting[type].GetCommission(a.Amount, b.AgentType);
                        //        //    b.AddToSubCommission(comm);
                        //        //    b.TierCommissionRate = sf.FibrePlusExternalSetting[type].GetCommissionRate(b.AgentType);
                        //        //}
                        //    }
                        //}
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

        public void GetTopLevelAgents(List<Agent> l, Dictionary<int, List<Agent>> dic)
        {
            SqlDataReader rd = null;

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("select distinct a.agentid, a.agentname, a.agenttype, a.agentlevel, a.agentteam from agent a ")
                    .Append("where a.agenttype = 'Master' ")
                    .Append("order by a.agentname");
                string q = sb.ToString();

                rd = Db.ExecuteReader(q, CommandType.Text);
                while (rd.Read())
                {
                    Agent a = new Agent();
                    a.AgentID = rd.Get<int>("agentid");
                    a.AgentName = rd.Get("agentname");
                    a.AgentType = rd.Get("agenttype", "AGT");
                    a.AgentLevel = rd.Get("agentlevel");
                    a.AgentTeam = rd.Get("agentteam");
                    a.Level = 0;

                    l.Add(a);
                }

                rd.Close();
                AddAgentsToDic(dic, l, 0);
                GetChildAgents(l, dic);
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

        private decimal GetCustomerSettlementAmount(Customer customer, ProductTypes productType)
        {
            decimal amt = 0;
            decimal tmpamt = 0;
            SqlDataReader rd = null;

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("select top 20 settlementidx, custid, comment, amount, realdate, paymenttype, ")
                    .Append("reference, orno, paymentmode from customersettlement ")
                    .Append("where custid in ")
                    .Append("(select custid from customer where customertype = 1 and status = 1 and agentid = @agentid and custid = @custid) and ")
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

                            if (productType.IsRebate)
                            {
                                amt = productType.InitialAmount;
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

                    if (o.IsRebate)
                        o.InitialAmount *= -1;

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
                    .Append("(select custid from customer where customertype = 1 and status = 1 and agentid = @agentid) ")
                    .Append("and productid in ")
                    .Append("(select productid from producttypes where (description like '%Mbps%' or description like '%Rebate%') and ")
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
                sb.Append("select custid, name, customertype from customer where customertype = 1 and status = 1 and agentid = @agentid ")
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
                sb.Append("select count(custid) from customer where customertype = 1 and status = 1 and agentid = @agentid");
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

        private void GetAgentHierarchy(int agentID, List<Agent> l, Dictionary<int, List<Agent>> dic)
        {
            SqlDataReader rd = null;

            try
            {
                if (agentID == 0)
                {
                    GetTopLevelAgents(l, dic);
                    return;
                }

                StringBuilder sb = new StringBuilder();
                sb.Append("select a.agentid, a.agentname, a.agenttype, a.agentlevel, a.agentteam, ")
                    .Append("b.agentid as [agentteamid], b.agentname as [agentteamname], b.agenttype as [agentteamtype], b.agentlevel as [agentteamlevel] ")
                    .Append("from agent a ")
                    .Append("left join agent b on a.agentteam = b.agentid ")
                    .Append("where a.agentid = @agentid");
                string q = sb.ToString();

                SqlParameter p = new SqlParameter("@agentid", SqlDbType.Int);
                p.Value = agentID;
                Db.AddParameter(p);

                rd = Db.ExecuteReader(q, CommandType.Text);
                while (rd.Read())
                {
                    Agent a = new Agent();
                    a.AgentID = rd.Get<int>("agentid");
                    a.AgentName = rd.Get("agentname");
                    a.AgentType = rd.Get("agenttype", "AGT");
                    a.AgentLevel = rd.Get("agentlevel");
                    a.AgentTeam = rd.Get("agentteam");

                    Agent b = new Agent();
                    b.AgentID = rd.Get<int>("agentteamid");
                    b.AgentName = rd.Get("agentteamname");
                    b.AgentType = rd.Get("agentteamtype");
                    b.AgentLevel = rd.Get("agentteamlevel");
                    b.Level = 0;

                    b.AddChildAgent(a);

                    l.Add(a);
                }

                rd.Close();
                AddAgentsToDic(dic, l, 1);
                GetChildAgents(l, dic);
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

        private void GetChildAgents(List<Agent> parentList, Dictionary<int, List<Agent>> dic)
        {
            SqlDataReader rd = null;

            try
            {
                Stack<List<Agent>> st = new Stack<List<Agent>>();
                st.Push(parentList);
                Dictionary<int, int> k = new Dictionary<int, int>();

                while (st.Count > 0)
                {
                    List<Agent> lp = st.Pop();

                    for (int i = 0; i < lp.Count; i++)
                    {
                        Agent parent = lp[i];
                        List<Agent> l = new List<Agent>();
                        StringBuilder sb = new StringBuilder();
                        sb.Append("select distinct a.agentid, a.agentname, a.agenttype, a.agentlevel, a.agentteam from agent a ")
                            .Append("where a.agentteam = @agentteam ")
                            .Append("order by a.agentname");
                        string q = sb.ToString();

                        SqlParameter p = new SqlParameter("@agentteam", SqlDbType.VarChar);
                        p.Value = parent.AgentID;
                        Db.AddParameter(p);

                        rd = Db.ExecuteReader(q, CommandType.Text);
                        while (rd.Read())
                        {
                            Agent a = new Agent();
                            a.AgentID = rd.Get<int>("agentid");
                            a.AgentName = rd.Get("agentname");
                            a.AgentType = rd.Get("agenttype", "AGT");
                            a.AgentLevel = rd.Get("agentlevel");
                            a.AgentTeam = rd.Get("agentteam");

                            if (k.ContainsKey(a.AgentID))
                                continue;

                            parent.AddChildAgent(a);

                            l.Add(a);

                            k.Add(a.AgentID, a.AgentID);
                        }

                        rd.Close();
                        AddAgentsToDic(dic, l, parent.Level + 1);

                        if (l.Count > 0)
                            st.Push(l);
                    }
                }
            }

            catch (Exception e)
            {
                Logger.Debug("", e);
                throw e;
            }
        }

        private void AddAgentsToDic(Dictionary<int, List<Agent>> dic, List<Agent> l, int level)
        {
            if (l == null)
                return;

            if (l.Count < 1)
                return;

            if (dic.ContainsKey(level))
            {
                List<Agent> la = dic[level];
                la.AddRange(l);
                dic[level] = la;
            }

            else
            {
                dic[level] = l;
            }
        }
    }
}
