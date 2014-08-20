using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Data;
using System.IO;
using System.Data.SqlClient;
using CommissionSystem.WebUI.Models;
using CommissionSystem.WebUI.Areas.Commission.Models;
using CommissionSystem.WebUI.Helpers;
using CommissionSystem.Domain.ProtoBufModels;
using CommissionSystem.Domain.Helpers;
using PagedList;
using ProtoBuf;
using NLog;

namespace CommissionSystem.WebUI.Areas.Commission.Controllers
{
    public class DataController : Controller
    {
        private const string COMMISSION_RESULT = "DATA_COMMISSION_RESULT";
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        //
        // GET: /Commission/Data/

        public ActionResult Index()
        {
            ViewBag.Menu = Constants.CORPORATE_DATA;
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
            Dictionary<string, object> r = new Dictionary<string, object>();
            FileStream fs = null;

            try
            {
                CommissionResult re = new CommissionResult();
                CommissionResult c = new CommissionResult();

                if (req.Load)
                {
                    c = Session[COMMISSION_RESULT] as CommissionResult;
                }

                else
                {
                    string file = GetFile(req.DateFrom);
                    if (string.IsNullOrEmpty(file))
                        throw new UIException(string.Format("The Commission for {0:MMMM yyyy} is not available yet, please contact the respective personel to generate the commission", req.DateFrom));

                    fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
                    re = Serializer.Deserialize<CommissionResult>(fs);
                }

                if (!req.Load)
                {
                    if (req.AgentID != 0)
                    {
                        if (re.CommissionViewDic.Keys.Count > 0)
                        {
                            var k = re.CommissionViewDic.Where(x => x.Key == req.AgentID.ToString()).First();
                            c.CommissionViewDic[k.Key] = k.Value;
                            c.AgentViewList.Add(re.AgentViewList.Where(x => x.AgentID == req.AgentID).First());
                        }
                    }

                    else
                    {
                        c = re;
                    }

                    Session[COMMISSION_RESULT] = c;
                }

                re = new CommissionResult();

                int pageSize = Constants.PAGE_SIZE;
                int pageNumber = (req.Page ?? 1);

                var l = c.AgentViewList.ToPagedList(pageNumber, pageSize);
                foreach (AgentView k in l)
                {
                    re.CommissionViewDic[k.AgentID.ToString()] = c.CommissionViewDic[k.AgentID.ToString()];
                }

                re.AgentViewList = l.ToList();
                Pager pager = new Pager(l.TotalItemCount, l.PageNumber, l.PageSize);

                r["success"] = 1;
                r["result"] = c;
                r["pager"] = pager;
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
                if (fs != null)
                    fs.Dispose();
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
                    DisplayName = "Corporate Data Commission",
                    Subject = "REDtone Corporate Data Commission"
                };

                CommissionResult c = Session[COMMISSION_RESULT] as CommissionResult;

                if (c == null)
                    throw new UIException("There is no commission result");

                ViewData["DateFrom"] = Utils.FormatDateTime(req.DateFrom);
                ViewData["DateTo"] = Utils.FormatDateTime(req.DateTo);

                Attachment att = c.GetDataCommissionResultData(req.DateFrom, req.DateTo);

                if (att != null)
                    emailInfo.AttList = new List<Attachment> { att };

                new CommissionMailController().CommissionNotificationEmail(c, emailInfo, ViewData,
                    CommissionMailController.COMMISSIONNOTIFICATION_DATA).DeliverAsync();

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

        private string GetFile(DateTime dt)
        {
            string c = HttpContext.Server.MapPath("~/result");
            string file = Path.Combine(c, string.Format("data/{0:yyyy}/{1:MM}/CommResult.bin", dt, dt));

            if (!System.IO.File.Exists(file))
            {
                file = null;
            }

            return file;
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
