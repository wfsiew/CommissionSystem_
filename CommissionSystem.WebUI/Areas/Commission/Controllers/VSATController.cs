using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using CommissionSystem.WebUI.Models;
using CommissionSystem.WebUI.Areas.Commission.Models;
using CommissionSystem.WebUI.Helpers;
using NLog;

namespace CommissionSystem.WebUI.Areas.Commission.Controllers
{
    public class VSATController : Controller
    {
        private const string COMMISSION_RESULT = "VSAT_COMMISSION_RESULT";
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        //
        // GET: /Commission/VSAT/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AgentSummary()
        {
            try
            {
                Dictionary<int, List<SalesParent>> dic = new Dictionary<int, List<SalesParent>>();
                List<SalesParent> l = new List<SalesParent>();
                GetTopLevelAgents(l, dic);
                ViewBag.list = l;
            }

            catch (Exception e)
            {
                Logger.Debug("", e);
            }

            return View();
        }

        [HttpPost]
        public ActionResult Commission(FibrePlusRequest req)
        {
            VSATCommission o = null;
            Dictionary<string, object> r = new Dictionary<string, object>();

            try
            {
                Dictionary<int, List<SalesParent>> dic = new Dictionary<int, List<SalesParent>>();
                List<SalesParent> l = new List<SalesParent>();
                GetAgentHierarchy(req.AgentID, l, dic);

                DateTime dateFrom = req.DateFrom;
                DateTime dateTo = req.DateTo;

                o = new VSATCommission();
                o.AgentDic = dic;
                o.AgentList = l;
                o.DateFrom = req.DateFrom;
                o.DateTo = req.DateTo.AddDays(1);
                o.SetCommission();

                CommissionResult c = new CommissionResult();

                if (req.AgentID != 0)
                {
                    var k = o.CommissionViewDic.Where(x => x.Key == req.AgentID.ToString()).First();
                    c.CommissionViewDic[k.Key] = k.Value;
                    c.AgentViewList.Add(o.AgentViewList.Where(x => x.AgentID == req.AgentID).First());
                }

                else
                {
                    c.CommissionViewDic = o.CommissionViewDic;
                    c.AgentViewList = o.AgentViewList;
                }

                r["success"] = 1;
                r["result"] = c;
                Session[COMMISSION_RESULT] = c;
            }

            catch (Exception e)
            {
                Logger.Debug("", e);
                r["error"] = 1;
                r["message"] = e.StackTrace;
            }

            finally
            {
                if (o != null)
                    o.Dispose();
            }

            return Json(r, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Mail(FibrePlusRequest req)
        {
            Dictionary<string, object> r = new Dictionary<string, object>();

            try
            {
                List<string> l = new List<string>()
                    {
                        "siewwingfei@hotmail.com",
                        "roshana.bedah@redtone.com"
                    };

                EmailInfo emailInfo = new EmailInfo
                {
                    ToList = l,
                    DisplayName = "V-SAT Commission",
                    Subject = "REDtone V-SAT Commission"
                };

                CommissionResult c = Session[COMMISSION_RESULT] as CommissionResult;

                if (c == null)
                    throw new UIException("There is no commission result");

                ViewData["DateFrom"] = Utils.FormatDateTime(req.DateFrom);
                ViewData["DateTo"] = Utils.FormatDateTime(req.DateTo);

                Attachment att = c.GetADSLCommissionResultData(req.DateFrom, req.DateTo);

                if (att != null)
                    emailInfo.AttList = new List<Attachment> { att };

                new CommissionMailController().CommissionNotificationEmail(c, emailInfo, ViewData,
                    CommissionMailController.COMMISSIONNOTIFICATION_ADSL).DeliverAsync();

                r["success"] = 1;
            }

            catch (UIException e)
            {
                r["error"] = 1;
                r["message"] = e.Message;
            }

            catch (Exception e)
            {
                Logger.Debug("", e);
                r["error"] = 1;
                r["message"] = e.StackTrace;
            }

            return Json(r, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Agents()
        {
            List<SalesParent> l = GetAgents();
            return Json(l, JsonRequestBehavior.AllowGet);
        }

        private void GetTopLevelAgents(List<SalesParent> l, Dictionary<int, List<SalesParent>> dic)
        {
            DbHelper d = null;
            SqlDataReader rd = null;

            try
            {
                Dictionary<int, SalesParent> m = GetAgents_();
                d = new DbHelper(DbHelper.GetConStr(Constants.RTCBROADBAND_CALLBILLING));
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
                d = new DbHelper(DbHelper.GetConStr(Constants.RTCBROADBAND_CALLBILLING));
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

        private void GetAgents(int agentID, List<SalesParent> l)
        {
            DbHelper d = null;
            SqlDataReader rd = null;

            try
            {
                if (agentID == 0)
                {
                    l = GetAgents();
                    return;
                }

                d = new DbHelper(DbHelper.GetConStr(Constants.RTCBROADBAND_CALLBILLING));
                StringBuilder sb = new StringBuilder();
                sb.Append("select sparentid, sparentname, geographycode, rptparentid from salesparent ")
                    .Append("where sparentid = @sparentid");
                string q = sb.ToString();

                SqlParameter p = new SqlParameter("@sparentid", SqlDbType.Int);
                p.Value = agentID;
                d.AddParameter(p);

                rd = d.ExecuteReader(q, CommandType.Text);
                while (rd.Read())
                {
                    SalesParent a = new SalesParent();
                    a.SParentID = rd.Get<int>("sparentid");
                    a.SParentName = rd.Get("sparentname");
                    a.GeographyCode = rd.Get("geographycode");
                    a.RptParentID = rd.Get<int>("rptparentid");

                    l.Add(a);
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

                if (d != null)
                    d.Dispose();
            }
        }

        private Dictionary<int, SalesParent> GetAgents_()
        {
            Dictionary<int, SalesParent> m = new Dictionary<int, SalesParent>();
            DbHelper d = null;
            SqlDataReader rd = null;

            try
            {
                d = new DbHelper(DbHelper.GetConStr(Constants.RTCBROADBAND_CALLBILLING));
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

        private List<SalesParent> GetAgents()
        {
            List<SalesParent> l = new List<SalesParent>();
            DbHelper d = null;
            SqlDataReader rd = null;

            try
            {
                d = new DbHelper(DbHelper.GetConStr(Constants.RTCBROADBAND_CALLBILLING));
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

                    l.Add(a);
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

            return l;
        }
    }
}
