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
    public class DiscountedCallServiceCommission : VoiceCommission
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        public DiscountedCallServiceCommission() : base()
        {
        }

        public void SetCommission()
        {
            string agentid = null;
            string bagentid = null;

            try
            {
                Dictionary<string, List<VoiceCommissionView>> cv = new Dictionary<string, List<VoiceCommissionView>>();
                Dictionary<string, AgentView> av = new Dictionary<string, AgentView>();
                Queue<List<SalesParent>> qa = new Queue<List<SalesParent>>();
                List<int> levels = AgentDic.Keys.ToList();
                levels.Reverse();
                SettingFactory sf = SettingFactory.Instance;
                Dictionary<int, ProductTypes> productTypeDic = GetProductTypes();
                Dictionary<string, bool> t = new Dictionary<string, bool>();

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

                        Dictionary<int, Customer> customerDic = GetCustomers();
                        List<CustomerBillingInfo> customerBIlist = GetCustomerBillingInfos();

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

                            if (!a.CustomerList.Exists(x => x.CustID == customer.CustID))
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

                            CallCharge callCharge = GetCustomerInvoice(customer);
                            decimal amount = GetCustomerSettlementAmount(customer);
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

                                v.CommissionRateIDD = sf.IDDInternalSetting[iddrate].Commission;
                                v.CommissionRateSTD = sf.DiscountedCallServiceInternalSetting[stdrate].Commission;
                                v.CommissionRateMOB = sf.DiscountedCallServiceInternalSetting[mobrate].Commission;

                                v.CallCharge += callCharge.Total;
                                v.CallChargeIDD += callCharge.IDD;
                                v.CallChargeSTD += callCharge.STD;
                                v.CallChargeMOB += callCharge.MOB;

                                av[agentid].TotalSettlement += v.CallCharge;

                                v.CommissionIDD = sf.IDDInternalSetting[iddrate].GetDirectCommission(v.CallChargeIDD);
                                v.CommissionSTD = sf.DiscountedCallServiceInternalSetting[stdrate].GetDirectCommission(v.CallChargeSTD);
                                v.CommissionMOB = sf.DiscountedCallServiceInternalSetting[mobrate].GetDirectCommission(v.CallChargeMOB);

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

                                        bv.CommissionRateIDD = sf.IDDInternalSetting[iddrate].GetCommissionRate(n);
                                        bv.CommissionRateSTD = sf.DiscountedCallServiceInternalSetting[stdrate].GetCommissionRate(n);
                                        bv.CommissionRateMOB = sf.DiscountedCallServiceInternalSetting[mobrate].GetCommissionRate(n);

                                        bv.CallCharge += v.CallCharge;
                                        bv.CallChargeIDD += v.CallChargeIDD;
                                        bv.CallChargeSTD += v.CallChargeSTD;
                                        bv.CallChargeMOB += v.CallChargeMOB;

                                        bv.CommissionIDD = sf.IDDInternalSetting[iddrate].GetCommission(v.CallChargeIDD, n);
                                        bv.CommissionSTD = sf.DiscountedCallServiceInternalSetting[stdrate].GetCommission(v.CallChargeSTD, n);
                                        bv.CommissionMOB = sf.DiscountedCallServiceInternalSetting[mobrate].GetCommission(v.CallChargeMOB, n);

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

                                v.CommissionRateIDD = sf.IDDExternalSetting[iddrate].Commission;
                                v.CommissionRateSTD = sf.DiscountedCallServiceExternalSetting[stdrate].Commission;
                                v.CommissionRateMOB = sf.DiscountedCallServiceExternalSetting[mobrate].Commission;

                                v.CallCharge += callCharge.Total;
                                v.CallChargeIDD += callCharge.IDD;
                                v.CallChargeSTD += callCharge.STD;
                                v.CallChargeMOB += callCharge.MOB;

                                av[agentid].TotalSettlement += v.CallCharge;

                                v.CommissionIDD = sf.IDDExternalSetting[iddrate].GetDirectCommission(v.CallChargeIDD);
                                v.CommissionSTD = sf.DiscountedCallServiceExternalSetting[stdrate].GetDirectCommission(v.CallChargeSTD);
                                v.CommissionMOB = sf.DiscountedCallServiceExternalSetting[mobrate].GetDirectCommission(v.CallChargeMOB);

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

                                        bv.CommissionRateIDD = sf.IDDExternalSetting[iddrate].GetCommissionRate(n);
                                        bv.CommissionRateSTD = sf.DiscountedCallServiceExternalSetting[stdrate].GetCommissionRate(n);
                                        bv.CommissionRateMOB = sf.DiscountedCallServiceExternalSetting[mobrate].GetCommissionRate(n);

                                        bv.CallCharge += v.CallCharge;
                                        bv.CallChargeIDD += v.CallChargeIDD;
                                        bv.CallChargeSTD += v.CallChargeSTD;
                                        bv.CallChargeMOB += v.CallChargeMOB;

                                        bv.CommissionIDD = sf.IDDExternalSetting[iddrate].GetCommission(v.CallChargeIDD, n);
                                        bv.CommissionSTD = sf.DiscountedCallServiceExternalSetting[stdrate].GetCommission(v.CallChargeSTD, n);
                                        bv.CommissionMOB = sf.DiscountedCallServiceExternalSetting[mobrate].GetCommission(v.CallChargeMOB, n);

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
                    .Append("name not like @keyword)");
                //.Append("dateadd(month, contractperiod, realcommencementdate) > current_timestamp");
                string q = sb.ToString();

                SqlParameter p = new SqlParameter("@agentid", SqlDbType.Int);
                p.Value = AgentID;
                Db.AddParameter(p);

                p = new SqlParameter("@keyword", SqlDbType.VarChar);
                p.Value = "%Leased Line%";
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
                    .Append("name not like @keyword");
                string q = sb.ToString();

                SqlParameter p = new SqlParameter("@agentid", SqlDbType.Int);
                p.Value = AgentID;
                Db.AddParameter(p);

                p = new SqlParameter("@keyword", SqlDbType.VarChar);
                p.Value = "%Leased Line%";
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
    }
}