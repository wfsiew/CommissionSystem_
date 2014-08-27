using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using CommissionSystem.Domain.ProtoBufModels;
using CommissionSystem.Domain.Helpers;
using CommissionSystem.Task.Helpers;
using NLog;

namespace CommissionSystem.Task.Models
{
    public class DataTask : IDisposable
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

        public DataTask()
        {
            Db = new DbHelper(DbHelper.GetConStr(Constants.RTCBROADBAND_CALLBILLING));
        }

        public void Dispose()
        {
            if (Db != null)
                Db.Dispose();
        }

        public void Run_()
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

                Dictionary<string, List<CommissionView>> cv = new Dictionary<string, List<CommissionView>>();
                Dictionary<string, AgentView> av = new Dictionary<string, AgentView>();
                List<int> levels = AgentDic.Keys.ToList();
                levels.Reverse();
                SettingFactory sf = SettingFactory.Instance;
                Dictionary<int, ProductTypes> productTypeDic = GetProductTypes();
                Dictionary<string, bool> t = new Dictionary<string, bool>();
                Dictionary<int, bool> ad = new Dictionary<int, bool>();
                Dictionary<int, List<CustomerList>> customerListDic = new Dictionary<int, List<CustomerList>>();
                Dictionary<int, List<CustomerListPackage>> customerListPackageDic = new Dictionary<int, List<CustomerListPackage>>();

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
                            cv[agentid] = new List<CommissionView>();

                        if (!av.ContainsKey(agentid))
                            av[agentid] = a.GetAgentInfo();

                        if (ad.ContainsKey(AgentID))
                            continue;

                        Dictionary<int, Customer> customerDic = GetCustomers();
                        List<CustomerBillingInfo> customerBIlist = GetCustomerBillingInfos();
                        ad[AgentID] = true;

                        if (!customerListDic.ContainsKey(a.SParentID))
                        {
                            List<CustomerList> customerListList = GetCustomerList(a.SParentID);
                            customerListDic[a.SParentID] = customerListList;
                        }

                        if (!customerListPackageDic.ContainsKey(a.SParentID))
                        {
                            List<CustomerListPackage> customerListPackageList = GetCustomerListPackage(a.SParentID);
                            customerListPackageDic[a.SParentID] = customerListPackageList;
                        }

                        foreach (KeyValuePair<int, Customer> d in customerDic)
                        {
                            string uid = string.Format("{0}-{1}", a.SParentID, d.Key);
                            if (t.ContainsKey(uid))
                                break;

                            else
                                t[uid] = true;

                            Customer customer = d.Value;
                            int custID = d.Key;

                            List<CustomerList> customerListList = customerListDic[AgentID];
                            CustomerList customerList = customerListList.Find(x => x.CLCustID == custID);

                            if (customerList == null)
                                continue;

                            List<CustomerListPackage> customerListPackageList = customerListPackageDic[AgentID];
                            CustomerListPackage customerListPackage = customerListPackageList.Find(x => x.LPCustListIndex == customerList.CLCustListIndex);

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

                            List<Invoice> invoiceList = GetCustomerInvoice(customer);
                            decimal amount = GetCustomerSettlementAmount(customer, invoiceList);
                            a.Amount += amount;

                            v.CommissionRate = customerListPackage.GetRateData();
                            v.SettlementAmount += amount;

                            av[agentid].TotalSettlement += v.SettlementAmount;

                            v.Commission = v.SettlementAmount * Convert.ToDecimal(v.CommissionRate);
                            if (customer.Status != 1)
                                v.Commission = 0;

                            av[agentid].TotalCommission += v.Commission;
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

                Dictionary<string, List<CommissionView>> cv = new Dictionary<string, List<CommissionView>>();
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
                            cv[agentid] = new List<CommissionView>();

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

                            List<Invoice> invoiceList = GetCustomerInvoice(customer);
                            decimal amount = GetCustomerSettlementAmount(customer, invoiceList);
                            a.Amount += amount;

                            if (a.IsInternalData)
                            {
                                v.CommissionRate = sf.ADSLInternalSetting.Commission;
                                v.SettlementAmount += amount;

                                av[agentid].TotalSettlement += v.SettlementAmount;

                                v.Commission = sf.ADSLInternalSetting.GetDirectCommission(v.SettlementAmount);
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
                                            cv[bagentid] = new List<CommissionView>();

                                        CommissionView bv = new CommissionView();
                                        bv.Customer = customer;
                                        bv.Commission = sf.ADSLInternalSetting.GetCommission(v.SettlementAmount, n);
                                        if (customer.Status != 1)
                                            bv.Commission = 0;

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
                                            cv[bagentid] = new List<CommissionView>();

                                        CommissionView bv = new CommissionView();
                                        bv.Customer = customer;
                                        bv.Commission = sf.ADSLExternalSetting.GetCommission(v.SettlementAmount, n);
                                        if (customer.Status != 1)
                                            bv.Commission = 0;

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

        public void GetTopLevelAgents(List<SalesParent> l, Dictionary<int, List<SalesParent>> dic)
        {
            SqlDataReader rd = null;

            try
            {
                Dictionary<int, SalesParent> m = GetAgents_();
                StringBuilder sb = new StringBuilder();
                sb.Append("select distinct sfid, magentid from salesforcedetail ")
                    .Append("where magentid = 0 and sfid <> 0 and sfid in ")
                    .Append("(select sparentid from salesparent where sparentname not like 'XX%')");
                string q = sb.ToString();

                rd = Db.ExecuteReader(q, CommandType.Text);
                while (rd.Read())
                {
                    int sfid = rd.Get<int>("sfid");
                    int magentid = rd.Get<int>("magentid");

                    SalesParent a = m[sfid];
                    l.Add(a);
                }

                rd.Close();
                AddAgentsToDic(dic, l, 0);
                GetChildAgents(l, dic, m, 0);
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

        private List<Invoice> GetCustomerInvoice(Customer customer)
        {
            List<Invoice> l = new List<Invoice>();
            SqlDataReader rd = null;

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("select i.custid, i.invoicenumber, i.totalcurrentcharge, i.realinvoicedate, csa.settlementidx from invoice i ")
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

        private decimal GetCustomerSettlementAmount(Customer customer, List<Invoice> l)
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

                    var invoiceList = l.Where(x => x.SettlementIdx == o.SettlementIdx);

                    if (invoiceList.Count() > 0)
                    {
                        foreach (Invoice i in invoiceList)
                        {
                            DateTime dt = i.InvoiceDate.AddDays(Constants.MAX_INVOICE_DAY);
                            if (o.RealDate <= dt)
                            {
                                if (!customer.HasSettlement(o))
                                    amt += o.Amount;

                                customer.AddSettlement(o);
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
            Dictionary<string, bool> t = new Dictionary<string, bool>();
            SqlDataReader rd = null;

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("select custid, rental, productid, amount, realcommencementdate ")
                    .Append("from customerbillinginfo ")
                    .Append("where custid in (")
                    .Append("select custid from customer where agentid = @agentid) ")
                    .Append("and custid in (")
                    .Append("select distinct custid from customersettlement where paymenttype = 1 and ")
                    .Append("productid <> 0 and paymenttype <> 3 and productid <> 41 and ")
                    .Append("realdate >= @datefrom and realdate < @dateto) ")
                    .Append("and custid in (")
                    .Append("select clcustid from customerlist where clsfid = @agentid)");
                string q = sb.ToString();

                SqlParameter p = new SqlParameter("@agentid", SqlDbType.Int);
                p.Value = AgentID;
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
                sb.Append("select custid, name, billingday, status from customer where agentid = @agentid and custid in (")
                    .Append("select distinct custid from customersettlement where paymenttype = 1 and ")
                    .Append("productid <> 0 and paymenttype <> 3 and productid <> 41 and ")
                    .Append("realdate >= @datefrom and realdate < @dateto) and custid in (")
                    .Append("select clcustid from customerlist where clsfid = @agentid)");
                string q = sb.ToString();

                SqlParameter p = new SqlParameter("@agentid", SqlDbType.Int);
                p.Value = AgentID;
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

        private List<CustomerList> GetCustomerList(int agentID)
        {
            List<CustomerList> l = new List<CustomerList>();
            SqlDataReader rd = null;

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("select * from customerlist where clsfid = @agentid");
                string q = sb.ToString();

                SqlParameter p = new SqlParameter("@agentid", SqlDbType.Int);
                p.Value = agentID;
                Db.AddParameter(p);

                rd = Db.ExecuteReader(q, CommandType.Text);
                while (rd.Read())
                {
                    CustomerList o = new CustomerList();
                    o.CLSFID = rd.Get<int>("CLSFID");
                    o.CLCustListIndex = rd.Get("CLCustListIndex");
                    o.CLCustID = rd.Get<int>("CLCustID");

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

        private List<CustomerListPackage> GetCustomerListPackage(int agentID)
        {
            List<CustomerListPackage> l = new List<CustomerListPackage>();
            SqlDataReader rd = null;

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("select * from customerlistpackage where lpsfid = @agentid");
                string q = sb.ToString();

                SqlParameter p = new SqlParameter("@agentid", SqlDbType.Int);
                p.Value = agentID;
                Db.AddParameter(p);

                rd = Db.ExecuteReader(q, CommandType.Text);
                while (rd.Read())
                {
                    CustomerListPackage o = new CustomerListPackage();
                    o.LPSFID = rd.Get<int>("LPSFID");
                    o.LPCustListIndex = rd.Get("LPCustListIndex");
                    o.LPCustListDesc = rd.Get("LPCustListDesc");
                    o.LPCPackID = rd.Get("LPCPackID");

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

        private void GetAgentHierarchy(int agentID, List<SalesParent> l, Dictionary<int, List<SalesParent>> dic)
        {
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
                StringBuilder sb = new StringBuilder();
                sb.Append("select distinct sfid, magentid from salesforcedetail ")
                    .Append("where sfid = @sfid and sfid in ")
                    .Append("(select sparentid from salesparent where sparentname not like 'XX%')");
                string q = sb.ToString();

                SqlParameter p = new SqlParameter("@sfid", SqlDbType.Int);
                p.Value = agentID;
                Db.AddParameter(p);

                rd = Db.ExecuteReader(q, CommandType.Text);
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
                GetChildAgents(l, dic, m, 0);
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

        private void GetChildAgents(List<SalesParent> parentList, Dictionary<int, List<SalesParent>> dic,
            Dictionary<int, SalesParent> m, int parentLevel)
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
                        Db.AddParameter(p);

                        rd = Db.ExecuteReader(q, CommandType.Text);
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
            SqlDataReader rd = null;

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("select distinct sparentid, sparentname, geographycode, rptparentid from salesparent ")
                    .Append("where sparentname not like 'XX%' ")
                    .Append("order by sparentname");
                string q = sb.ToString();

                rd = Db.ExecuteReader(q, CommandType.Text);
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
            }

            return m;
        }
    }
}
