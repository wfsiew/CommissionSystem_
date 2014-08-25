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
using CommissionSystem.Task.Models;
using PagedList;
using OfficeOpenXml;
using ProtoBuf;
using NLog;

namespace CommissionSystem.WebUI.Areas.Commission.Controllers
{
    public class SpeedPlusController : Controller
    {
        private const string COMMISSION_RESULT = "SPEED+_COMMISSION_RESULT";
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        //
        // GET: /Commission/SpeedPlus/

        public ActionResult Index()
        {
            ViewBag.Menu = Constants.SPEEDPLUS;
            return View();
        }

        public ActionResult AgentSummary()
        {
            try
            {
                ViewBag.Menu = Constants.AGENT_STRUCTURE_SPEEDPLUS;
                Dictionary<int, List<Agent>> dic = new Dictionary<int, List<Agent>>();
                List<Agent> l = new List<Agent>();
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
                            if (re.CommissionViewDic.ContainsKey(req.AgentID.ToString()))
                            {
                                var k = re.CommissionViewDic.Where(x => x.Key == req.AgentID.ToString()).First();
                                c.CommissionViewDic[k.Key] = k.Value;
                                c.AgentViewList.Add(re.AgentViewList.Where(x => x.AgentID == req.AgentID).First());
                            }
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
                r["result"] = re;
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
                        "chinchin.lee@redtone.com"
                    };

                EmailInfo emailInfo = new EmailInfo
                {
                    ToList = l,
                    DisplayName = "Speed+ Commission",
                    Subject = "REDtone Speed+ Commission"
                };

                CommissionResult c = Session[COMMISSION_RESULT] as CommissionResult;

                if (c == null)
                    throw new UIException("There is no commission result");

                ViewData["DateFrom"] = Utils.FormatDateTime(req.DateFrom);
                ViewData["DateTo"] = Utils.FormatDateTime(req.DateTo);

                Attachment att = c.GetFibrePlusCommissionResultData(req.DateFrom, req.DateTo);

                if (att != null)
                    emailInfo.AttList = new List<Attachment> { att };

                new CommissionMailController().CommissionNotificationEmail(c, emailInfo, ViewData, 
                    CommissionMailController.COMMISSIONNOTIFICATION_FIBREPLUS).DeliverAsync();

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
            List<Agent> l = GetAgents();
            return Json(l, JsonRequestBehavior.AllowGet);
        }

        private void GetTopLevelAgents(List<Agent> l, Dictionary<int, List<Agent>> dic)
        {
            DbHelper d = null;
            SqlDataReader rd = null;

            try
            {
                d = new DbHelper(DbHelper.GetConStr(Constants.HSBB_BILLING));
                StringBuilder sb = new StringBuilder();
                sb.Append("select distinct a.agentid, a.agentname, a.agenttype, a.agentlevel, a.agentteam from agent a ")
                    .Append("where a.agenttype = 'Master' ")
                    .Append("order by a.agentname");
                string q = sb.ToString();

                rd = d.ExecuteReader(q, CommandType.Text);
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
                GetChildAgents(l, dic, d);
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

        private void GetAgentHierarchy(int agentID, List<Agent> l, Dictionary<int, List<Agent>> dic)
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

                d = new DbHelper(DbHelper.GetConStr(Constants.HSBB_BILLING));
                StringBuilder sb = new StringBuilder();
                sb.Append("select a.agentid, a.agentname, a.agenttype, a.agentlevel, a.agentteam, ")
                    .Append("b.agentid as [agentteamid], b.agentname as [agentteamname], b.agenttype as [agentteamtype], b.agentlevel as [agentteamlevel] ")
                    .Append("from agent a ")
                    .Append("left join agent b on a.agentteam = b.agentid ")
                    .Append("where a.agentid = @agentid");
                string q = sb.ToString();

                SqlParameter p = new SqlParameter("@agentid", SqlDbType.Int);
                p.Value = agentID;
                d.AddParameter(p);

                rd = d.ExecuteReader(q, CommandType.Text);
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
                GetChildAgents(l, dic, d);
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

        private void GetChildAgents(List<Agent> parentList, Dictionary<int, List<Agent>> dic, DbHelper d)
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
                        d.AddParameter(p);

                        rd = d.ExecuteReader(q, CommandType.Text);
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

        private string GetFile(DateTime dt)
        {
            string c = HttpContext.Server.MapPath("~/result");
            string file = Path.Combine(c, string.Format("speed+/{0:yyyy}/{1:MM}/CommResult.bin", dt, dt));

            if (!System.IO.File.Exists(file))
            {
                file = null;
            }

            return file;
        }

        private List<Agent> GetAgents()
        {
            List<Agent> l = new List<Agent>();
            DbHelper d = null;
            SqlDataReader rd = null;

            try
            {
                d = new DbHelper(DbHelper.GetConStr(Constants.HSBB_BILLING));
                StringBuilder sb = new StringBuilder();
                sb.Append("select distinct a.agentid, a.agentname, a.agenttype, a.agentlevel, a.agentteam from agent a ")
                    .Append("order by a.agentname");
                string q = sb.ToString();

                rd = d.ExecuteReader(q, CommandType.Text);
                while (rd.Read())
                {
                    Agent a = new Agent();
                    a.AgentID = rd.Get<int>("agentid");
                    a.AgentName = rd.Get("agentname");
                    a.AgentType = rd.Get("agenttype", "AGT");
                    a.AgentLevel = rd.Get("agentlevel");
                    a.AgentTeam = rd.Get("agentteam");

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
