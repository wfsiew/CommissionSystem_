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
    public class SpeedPlusController : Controller
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();
        private const string DB = "HSBB_Billing";

        //
        // GET: /Commission/SpeedPlus/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AgentSummary()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AgentSummary(DateTime dateFrom, DateTime dateTo)
        {
            SpeedPlusCommission o = null;

            try
            {
                Dictionary<int, List<Agent>> dic = new Dictionary<int, List<Agent>>();
                List<Agent> l = new List<Agent>();
                GetTopLevelAgents(l, dic);
                o = new SpeedPlusCommission();
                o.AgentDic = dic;
                o.AgentList = l;
                o.DateFrom = dateFrom;
                o.DateTo = dateTo.AddDays(1);
                o.SetCommission();
                ViewBag.list = l;
            }

            catch (Exception e)
            {
                Logger.Debug("", e);
            }

            finally
            {
                if (o != null)
                    o.Dispose();
            }

            return View();
        }

        [HttpPost]
        public ActionResult Commission(FibrePlusRequest req)
        {
            SpeedPlusCommission o = null;
            Dictionary<string, object> r = new Dictionary<string, object>();

            try
            {
                Dictionary<int, List<Agent>> dic = new Dictionary<int, List<Agent>>();
                List<Agent> l = new List<Agent>();
                GetAgentHierarchy(req.AgentID, l, dic);

                DateTime dateFrom = req.DateFrom;
                DateTime dateTo = req.DateTo;

                o = new SpeedPlusCommission();
                o.AgentDic = dic;
                o.AgentList = l;
                o.DateFrom = req.DateFrom;
                o.DateTo = req.DateTo.AddDays(1);
                o.SetCommission();

                Agent a = l.First();

                Dictionary<string, object> m = new Dictionary<string, object>();
                List<int> lk = dic.Keys.ToList();
                lk.Sort();
                for (int i = 0; i < lk.Count; i++)
                {
                    List<Agent> la = dic[lk[i]];
                    object v = la.Select(x => new
                    {
                        AgentID = x.AgentID,
                        AgentName = x.AgentName,
                        AgentTeam = x.AgentTeam,
                        AgentType = x.AgentType,
                        AgentTeamName = x.ParentAgent == null ? "" : x.ParentAgent.AgentName,
                        AgentTeamType = x.ParentAgent == null ? "" : x.ParentAgent.AgentType,
                        Amount = x.Amount,
                        CommissionRate = x.CommissionRate,
                        TierCommissionRate = x.TierCommissionRate,
                        TotalCommission = x.TotalCommission,
                        CustomerList = x.CustomerList
                    });
                    m[lk[i].ToString()] = v;
                }

                List<string> ls = m.Keys.ToList();
                ls.Sort();

                r["success"] = 1;
                r["commission"] = a.TotalCommission;
                r["commissionrate"] = a.CommissionRate;
                r["tiercommissionrate"] = a.TierCommissionRate;
                r["agentlevels"] = ls;
                r["agentlist"] = m;
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
                d = new DbHelper(DbHelper.GetConStr(DB));
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

                d = new DbHelper(DbHelper.GetConStr(DB));
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

        private List<Agent> GetAgents()
        {
            List<Agent> l = new List<Agent>();
            DbHelper d = null;
            SqlDataReader rd = null;

            try
            {
                d = new DbHelper(DbHelper.GetConStr(DB));
                StringBuilder sb = new StringBuilder();
                sb.Append("select distinct a.agentid, a.agentname, a.agenttype, a.agentlevel, a.agentteam from agent a ")
                    .Append("left join customer c on a.agentid = c.agentid ")
                    .Append("where c.customertype in (2, 3) ")
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
