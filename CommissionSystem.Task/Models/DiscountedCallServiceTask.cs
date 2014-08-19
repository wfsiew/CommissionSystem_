using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using CommissionSystem.Domain.Models;
using CommissionSystem.Task.Helpers;
using NLog;

namespace CommissionSystem.Task.Models
{
    public class DiscountedCallServiceTask : VoiceTask
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        public void Run()
        {
            string agentid = null;
            string bagentid = null;

            try
            {
                Dictionary<int, List<SalesParent>> dic = new Dictionary<int, List<SalesParent>>();
                List<SalesParent> agentlist = new List<SalesParent>();
                GetAgentHierarchy(0, agentlist, dic);

                AgentDic = dic;
                AgentList = agentlist;

                Dictionary<string, List<VoiceCommissionView>> cv = new Dictionary<string, List<VoiceCommissionView>>();
                Dictionary<string, AgentView> av = new Dictionary<string, AgentView>();
                Queue<List<SalesParent>> qa = new Queue<List<SalesParent>>();
                List<int> levels = AgentDic.Keys.ToList();
                levels.Reverse();
                SettingFactory sf = SettingFactory.Instance;
                Dictionary<int, ProductTypes> productTypeDic = GetProductTypes();
                Dictionary<string, bool> t = new Dictionary<string, bool>();
                Dictionary<int, bool> ad = new Dictionary<int, bool>();

                for (int i = 0; i < levels.Count; i++)
                {
                    int k = levels[i];
                    List<SalesParent> l = AgentDic[k];
                    for (int j = 0; j < l.Count; j++)
                    {
                        SalesParent a = l[j];
                        List<SalesParent> blist = a.ParentAgentList;

                        AgentID = a.SParentID;
                        agentid = a.SParentID.ToString();

                        if (!cv.ContainsKey(agentid))
                            cv[agentid] = new List<VoiceCommissionView>();

                        if (!av.ContainsKey(agentid))
                            av[agentid] = a.GetAgentInfo();

                        if (ad.ContainsKey(AgentID))
                            continue;

                        Dictionary<int, Customer> customerDic = GetCustomers();
                        List<CustomerBillingInfo> customerBIlist = GetCustomerBillingInfos();
                        ad[AgentID] = true;

                        foreach (KeyValuePair<int, Customer> d in customerDic)
                        {
                            string uid = string.Format("{0}-{1}", a.SParentID, d.Key);
                            if (t.ContainsKey(uid))
                                break;

                            else
                                t[uid] = true;

                            qa.Clear();

                            if (a.ParentAgentList.Count > 0)
                                qa.Enqueue(a.ParentAgentList);

                            Customer customer = d.Value;
                            int custID = d.Key;
                            List<CustomerBillingInfo> ebi = customerBIlist.Where(x => x.CustID == custID).ToList();

                            customer.BillingInfoList = ebi;
                            a.AddCustomer(customer);

                            VoiceCommissionView v = new VoiceCommissionView();
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

                            CallCharge callCharge = new CallCharge();
                            List<Invoice> invoiceList = GetCustomerInvoice(customer);
                            decimal amount = GetCustomerSettlementAmount(customer, invoiceList, callCharge);
                            a.Amount += amount;

                            string desc = GetRatePlanDescription(customer);
                            CallRate cr = GetIDDSTDMOBRate(desc);

                            int iddrate = 0;
                            double stdrate = 0;
                            double mobrate = 0;

                            if (a.IsInternalVoice)
                            {
                                iddrate = cr.IDD > IDDInternal.MaxRate ? IDDInternal.MaxRate : cr.IDD;
                                stdrate = cr.STD > DiscountedCallServiceInternal.MaxRate ? DiscountedCallServiceInternal.MaxRate : cr.STD;
                                mobrate = cr.MOB > DiscountedCallServiceInternal.MaxRate ? DiscountedCallServiceInternal.MaxRate : cr.MOB;

                                if (!sf.IDDInternalSetting.ContainsKey(iddrate))
                                    iddrate = 0;

                                if (!sf.DiscountedCallServiceInternalSetting.ContainsKey(stdrate))
                                    stdrate = 0;

                                if (!sf.DiscountedCallServiceInternalSetting.ContainsKey(mobrate))
                                    mobrate = 0;

                                v.CommissionRateIDD = iddrate > 0 ? sf.IDDInternalSetting[iddrate].Commission : 0;
                                v.CommissionRateSTD = stdrate > 0 ? sf.DiscountedCallServiceInternalSetting[stdrate].Commission : 0;
                                v.CommissionRateMOB = mobrate > 0 ? sf.DiscountedCallServiceInternalSetting[mobrate].Commission : 0;

                                v.CallCharge += callCharge.Total;
                                v.CallChargeIDD += callCharge.IDD;
                                v.CallChargeSTD += callCharge.STD;
                                v.CallChargeMOB += callCharge.MOB;

                                av[agentid].TotalSettlement += v.CallCharge;

                                v.CommissionIDD = iddrate > 0 ? sf.IDDInternalSetting[iddrate].GetDirectCommission(v.CallChargeIDD) : 0;
                                v.CommissionSTD = stdrate > 0 ? sf.DiscountedCallServiceInternalSetting[stdrate].GetDirectCommission(v.CallChargeSTD) : 0;
                                v.CommissionMOB = mobrate > 0 ? sf.DiscountedCallServiceInternalSetting[mobrate].GetDirectCommission(v.CallChargeMOB) : 0;

                                v.Commission = v.CommissionIDD + v.CommissionSTD + v.CommissionMOB;
                                if (customer.Status != 1)
                                    v.Commission = 0;

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
                                            cv[bagentid] = new List<VoiceCommissionView>();

                                        VoiceCommissionView bv = new VoiceCommissionView();
                                        bv.Customer = customer;

                                        bv.CommissionRateIDD = iddrate > 0 ? sf.IDDInternalSetting[iddrate].GetCommissionRate(n) : 0;
                                        bv.CommissionRateSTD = stdrate > 0 ? sf.DiscountedCallServiceInternalSetting[stdrate].GetCommissionRate(n) : 0;
                                        bv.CommissionRateMOB = mobrate > 0 ? sf.DiscountedCallServiceInternalSetting[mobrate].GetCommissionRate(n) : 0;

                                        bv.CallCharge += v.CallCharge;
                                        bv.CallChargeIDD += v.CallChargeIDD;
                                        bv.CallChargeSTD += v.CallChargeSTD;
                                        bv.CallChargeMOB += v.CallChargeMOB;

                                        bv.CommissionIDD = iddrate > 0 ? sf.IDDInternalSetting[iddrate].GetCommission(v.CallChargeIDD, n) : 0;
                                        bv.CommissionSTD = stdrate > 0 ? sf.DiscountedCallServiceInternalSetting[stdrate].GetCommission(v.CallChargeSTD, n) : 0;
                                        bv.CommissionMOB = mobrate > 0 ? sf.DiscountedCallServiceInternalSetting[mobrate].GetCommission(v.CallChargeMOB, n) : 0;

                                        bv.Commission = bv.CommissionIDD + bv.CommissionSTD + bv.CommissionMOB;
                                        if (customer.Status != 1)
                                            bv.Commission = 0;

                                        cv[bagentid].Add(bv);

                                        if (!av.ContainsKey(bagentid))
                                            av[bagentid] = b.GetAgentInfo();

                                        av[bagentid].TotalSettlement += bv.CallCharge;
                                        av[bagentid].TotalCommission += bv.Commission;
                                    }
                                }
                            }

                            else
                            {
                                iddrate = cr.IDD > IDDExternal.MaxRate ? IDDExternal.MaxRate : cr.IDD;
                                stdrate = cr.STD > DiscountedCallServiceExternal.MaxRate ? DiscountedCallServiceExternal.MaxRate : cr.STD;
                                mobrate = cr.MOB > DiscountedCallServiceExternal.MaxRate ? DiscountedCallServiceExternal.MaxRate : cr.MOB;

                                if (!sf.IDDExternalSetting.ContainsKey(iddrate))
                                    iddrate = 0;

                                if (!sf.DiscountedCallServiceExternalSetting.ContainsKey(stdrate))
                                    stdrate = 0;

                                if (!sf.DiscountedCallServiceExternalSetting.ContainsKey(mobrate))
                                    mobrate = 0;

                                v.CommissionRateIDD = iddrate > 0 ? sf.IDDExternalSetting[iddrate].Commission : 0;
                                v.CommissionRateSTD = stdrate > 0 ? sf.DiscountedCallServiceExternalSetting[stdrate].Commission : 0;
                                v.CommissionRateMOB = mobrate > 0 ? sf.DiscountedCallServiceExternalSetting[mobrate].Commission : 0;

                                v.CallCharge += callCharge.Total;
                                v.CallChargeIDD += callCharge.IDD;
                                v.CallChargeSTD += callCharge.STD;
                                v.CallChargeMOB += callCharge.MOB;

                                av[agentid].TotalSettlement += v.CallCharge;

                                v.CommissionIDD = iddrate > 0 ? sf.IDDExternalSetting[iddrate].GetDirectCommission(v.CallChargeIDD) : 0;
                                v.CommissionSTD = stdrate > 0 ? sf.DiscountedCallServiceExternalSetting[stdrate].GetDirectCommission(v.CallChargeSTD) : 0;
                                v.CommissionMOB = mobrate > 0 ? sf.DiscountedCallServiceExternalSetting[mobrate].GetDirectCommission(v.CallChargeMOB) : 0;

                                v.Commission = v.CommissionIDD + v.CommissionSTD + v.CommissionMOB;
                                if (customer.Status != 1)
                                    v.Commission = 0;

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
                                            cv[bagentid] = new List<VoiceCommissionView>();

                                        VoiceCommissionView bv = new VoiceCommissionView();
                                        bv.Customer = customer;

                                        bv.CommissionRateIDD = iddrate > 0 ? sf.IDDExternalSetting[iddrate].GetCommissionRate(n) : 0;
                                        bv.CommissionRateSTD = stdrate > 0 ? sf.DiscountedCallServiceExternalSetting[stdrate].GetCommissionRate(n) : 0;
                                        bv.CommissionRateMOB = mobrate > 0 ? sf.DiscountedCallServiceExternalSetting[mobrate].GetCommissionRate(n) : 0;

                                        bv.CallCharge += v.CallCharge;
                                        bv.CallChargeIDD += v.CallChargeIDD;
                                        bv.CallChargeSTD += v.CallChargeSTD;
                                        bv.CallChargeMOB += v.CallChargeMOB;

                                        bv.CommissionIDD = iddrate > 0 ? sf.IDDExternalSetting[iddrate].GetCommission(v.CallChargeIDD, n) : 0;
                                        bv.CommissionSTD = stdrate > 0 ? sf.DiscountedCallServiceExternalSetting[stdrate].GetCommission(v.CallChargeSTD, n) : 0;
                                        bv.CommissionMOB = mobrate > 0 ? sf.DiscountedCallServiceExternalSetting[mobrate].GetCommission(v.CallChargeMOB, n) : 0;

                                        bv.Commission = bv.CommissionIDD + bv.CommissionSTD + bv.CommissionMOB;
                                        if (customer.Status != 1)
                                            bv.Commission = 0;

                                        cv[bagentid].Add(bv);

                                        if (!av.ContainsKey(bagentid))
                                            av[bagentid] = b.GetAgentInfo();

                                        av[bagentid].TotalSettlement += bv.CallCharge;
                                        av[bagentid].TotalCommission += bv.Commission;
                                    }
                                }
                            }
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

        private List<CustomerBillingInfo> GetCustomerBillingInfos()
        {
            List<CustomerBillingInfo> l = new List<CustomerBillingInfo>();
            Dictionary<string, bool> t = new Dictionary<string, bool>();
            SqlDataReader rd = null;

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("select custid, rental, productid, amount, realcommencementdate ")
                    .Append("from customerbillinginfo ")
                    .Append("where custid in (")
                    .Append("select custid from customer where agentid = @agentid and serviceid = 13 and ")
                    .Append("name not like @keyword) ")
                    .Append("and custid in (")
                    .Append("select custid from customersettlement where paymenttype <> 2 and ")
                    .Append("realdate >= @datefrom and realdate < @dateto)");
                //.Append("dateadd(month, contractperiod, realcommencementdate) > current_timestamp");
                string q = sb.ToString();

                SqlParameter p = new SqlParameter("@agentid", SqlDbType.Int);
                p.Value = AgentID;
                Db.AddParameter(p);

                p = new SqlParameter("@keyword", SqlDbType.VarChar);
                p.Value = "%Leased Line%";
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
                    CustomerBillingInfo o = new CustomerBillingInfo();
                    o.CustID = rd.Get<int>("custid");
                    o.Rental = rd.Get<decimal>("rental");
                    o.ProductID = rd.Get<int>("productid");
                    o.Amount = rd.Get<decimal>("amount");
                    o.RealCommencementDate = rd.GetDateTime("realcommencementdate");

                    string uid = string.Format("{0}-{1}", o.CustID, o.ProductID);

                    if (!t.ContainsKey(uid))
                    {
                        t[uid] = true;
                        l.Add(o);
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

            return l;
        }

        private Dictionary<int, Customer> GetCustomers()
        {
            Dictionary<int, Customer> dic = new Dictionary<int, Customer>();
            SqlDataReader rd = null;

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("select custid, name, rateplanid, billingday, status from customer where agentid = @agentid and serviceid = 13 and ")
                    .Append("name not like @keyword and custid in (")
                    .Append("select custid from customersettlement where paymenttype <> 2 and ")
                    .Append("realdate >= @datefrom and realdate < @dateto)");
                string q = sb.ToString();

                SqlParameter p = new SqlParameter("@agentid", SqlDbType.Int);
                p.Value = AgentID;
                Db.AddParameter(p);

                p = new SqlParameter("@keyword", SqlDbType.VarChar);
                p.Value = "%Leased Line%";
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
                    Customer o = new Customer();
                    o.CustID = rd.Get<int>("custid");
                    o.Name = rd.Get("name");
                    o.RatePlanID = rd.Get<int>("rateplanid");
                    o.BillingDay = rd.Get<int>("billingday");
                    o.Status = rd.Get<int>("status");

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

        private void GetTopLevelAgents(List<SalesParent> l, Dictionary<int, List<SalesParent>> dic)
        {
            DbHelper d = null;
            SqlDataReader rd = null;

            try
            {
                Dictionary<int, SalesParent> m = GetAgents_();
                d = new DbHelper(DbHelper.GetConStr(Constants.CALLBILLING2));
                StringBuilder sb = new StringBuilder();
                sb.Append("select distinct sfid, magentid from salesforcedetail ")
                    .Append("where magentid = 0 and sfid <> 0 and sfid in ")
                    .Append("(select sparentid from salesparent where sparentname not like 'XX%')");
                string q = sb.ToString();

                rd = d.ExecuteReader(q, CommandType.Text);
                while (rd.Read())
                {
                    int sfid = rd.Get<int>("sfid");
                    int magentid = rd.Get<int>("magentid");

                    SalesParent a = m[sfid];
                    l.Add(a);
                }

                rd.Close();
                AddAgentsToDic(dic, l, 0);
                GetChildAgents(l, dic, m, d, 0);
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

                if (d != null)
                    d.Dispose();
            }
        }

        private void GetAgentHierarchy(int agentID, List<SalesParent> l, Dictionary<int, List<SalesParent>> dic)
        {
            DbHelper d = null;
            SqlDataReader rd = null;

            try
            {
                if (agentID == 0)
                {
                    GetTopLevelAgents(l, dic);
                    return;
                }

                Dictionary<int, bool> t = new Dictionary<int, bool>();
                Dictionary<int, SalesParent> m = GetAgents_();
                d = new DbHelper(DbHelper.GetConStr(Constants.CALLBILLING2));
                StringBuilder sb = new StringBuilder();
                sb.Append("select distinct sfid, magentid from salesforcedetail ")
                    .Append("where sfid = @sfid and sfid in ")
                    .Append("(select sparentid from salesparent where sparentname not like 'XX%')");
                string q = sb.ToString();

                SqlParameter p = new SqlParameter("@sfid", SqlDbType.Int);
                p.Value = agentID;
                d.AddParameter(p);

                rd = d.ExecuteReader(q, CommandType.Text);
                while (rd.Read())
                {
                    int sfid = rd.Get<int>("sfid");
                    int magentid = rd.Get<int>("magentid");

                    SalesParent a = m[sfid];

                    if (m.ContainsKey(magentid))
                    {
                        SalesParent b = m[magentid];
                        b.AddChildAgent(a);
                    }

                    if (!t.ContainsKey(sfid))
                    {
                        t[sfid] = true;
                        l.Add(a);
                    }
                }

                rd.Close();
                AddAgentsToDic(dic, l, 0);
                GetChildAgents(l, dic, m, d, 0);
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

                if (d != null)
                    d.Dispose();
            }
        }

        private void GetChildAgents(List<SalesParent> parentList, Dictionary<int, List<SalesParent>> dic,
            Dictionary<int, SalesParent> m, DbHelper d, int parentLevel)
        {
            SqlDataReader rd = null;
            int level = 0;

            try
            {
                Stack<List<SalesParent>> st = new Stack<List<SalesParent>>();
                Stack<int> sl = new Stack<int>();
                st.Push(parentList);
                sl.Push(parentLevel);

                while (st.Count > 0)
                {
                    List<SalesParent> lp = st.Pop();
                    level = sl.Pop();

                    for (int i = 0; i < lp.Count; i++)
                    {
                        SalesParent parent = lp[i];
                        List<SalesParent> l = new List<SalesParent>();
                        Dictionary<int, bool> t = new Dictionary<int, bool>();
                        StringBuilder sb = new StringBuilder();
                        sb.Append("select distinct sfid, magentid from salesforcedetail ")
                            .Append("where magentid = @magentid and sfid in ")
                            .Append("(select sparentid from salesparent where sparentname not like 'XX%')");
                        string q = sb.ToString();

                        SqlParameter p = new SqlParameter("@magentid", SqlDbType.Int);
                        p.Value = parent.SParentID;
                        d.AddParameter(p);

                        rd = d.ExecuteReader(q, CommandType.Text);
                        while (rd.Read())
                        {
                            int sfid = rd.Get<int>("sfid");
                            int magentid = rd.Get<int>("magentid");

                            SalesParent a = m[sfid];

                            parent.AddChildAgent(a);

                            if (!t.ContainsKey(sfid))
                            {
                                t[sfid] = true;
                                l.Add(a);
                            }
                        }

                        rd.Close();
                        AddAgentsToDic(dic, l, level + 1);

                        if (l.Count > 0)
                        {
                            st.Push(l);
                            sl.Push(level + 1);
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

        private void AddAgentsToDic(Dictionary<int, List<SalesParent>> dic, List<SalesParent> l, int level)
        {
            if (l == null)
                return;

            if (l.Count < 1)
                return;

            if (dic.ContainsKey(level))
            {
                List<SalesParent> la = dic[level];
                la.AddRange(l);
                dic[level] = la;
            }

            else
            {
                dic[level] = l;
            }
        }

        private Dictionary<int, SalesParent> GetAgents_()
        {
            Dictionary<int, SalesParent> m = new Dictionary<int, SalesParent>();
            DbHelper d = null;
            SqlDataReader rd = null;

            try
            {
                d = new DbHelper(DbHelper.GetConStr(Constants.CALLBILLING2));
                StringBuilder sb = new StringBuilder();
                sb.Append("select distinct sparentid, sparentname, geographycode, rptparentid from salesparent ")
                    .Append("where sparentname not like 'XX%' ")
                    .Append("order by sparentname");
                string q = sb.ToString();

                rd = d.ExecuteReader(q, CommandType.Text);
                while (rd.Read())
                {
                    SalesParent a = new SalesParent();
                    a.SParentID = rd.Get<int>("sparentid");
                    a.SParentName = rd.Get("sparentname");
                    a.GeographyCode = rd.Get("geographycode");
                    a.RptParentID = rd.Get<int>("rptparentid");

                    m[a.SParentID] = a;
                }

                rd.Close();
            }

            catch (Exception e)
            {
                Logger.Debug("", e);
            }

            finally
            {
                if (rd != null)
                    rd.Dispose();

                if (d != null)
                    d.Dispose();
            }

            return m;
        }
    }
}
