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
    public class FibrePlusController : Controller
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();
        private const string DB = "HSBB_Billing";

        //
        // GET: /Commission/FibrePlus/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AllAgents()
        {
            try
            {
                List<Agent> l = GetTopLevelAgents();
                return View(l);
            }

            catch (Exception e)
            {
                Logger.Debug("", e);
                throw e;
            }
        }

        [HttpPost]
        public ActionResult Commission(FibrePlusRequest req)
        {
            FibrePlusCommission o = null;
            Dictionary<string, object> r = new Dictionary<string, object>();

            try
            {
                DateTime dateFrom = req.DateFrom;
                DateTime dateTo = req.DateTo;

                o = new FibrePlusCommission();
                o.DateFrom = req.DateFrom;
                o.DateTo = req.DateTo.AddDays(1);

                o.AgentID = req.AgentID;
                o.AgentLevel = req.AgentLevel == null ? -1 : req.AgentLevel.Value;
                r = o.GetCommission();

                r["success"] = 1;
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

        private List<Agent> GetTopLevelAgents(Dictionary<int, List<Agent>> dic)
        {
            List<Agent> l = new List<Agent>();
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
                    a.AgentType = rd.Get("agenttype");
                    a.AgentLevel = rd.Get("agentlevel");
                    a.AgentTeam = rd.Get("agentteam");
                    a.Level = 0;

                    l.Add(a);
                }

                rd.Close();
                dic[0] = l;
                GetChildAgents(l, d, dic);
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

            return l;
        }

        private void GetChildAgents(List<Agent> parentList, DbHelper d, Dictionary<int, List<Agent>> dic)
        {
            SqlDataReader rd = null;

            try
            {
                Stack<List<Agent>> st = new Stack<List<Agent>>();
                st.Push(parentList);

                while (st.Count > 0)
                {
                    List<Agent> lp = st.Pop();

                    for (int i = 0; i < lp.Count; i++)
                    {
                        Agent parent = parentList[i];
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
                            a.AgentType = rd.Get("agenttype");
                            a.AgentLevel = rd.Get("agentlevel");
                            a.AgentTeam = rd.Get("agentteam");

                            parent.AddChildAgent(a);

                            l.Add(a);
                        }

                        rd.Close();
                        List<Agent> la = dic[parent.Level + 1];
                        if (la == null)
                            dic[parent.Level + 1] = l;

                        else
                        {
                            la.AddRange(l);
                            dic[parent.Level + 1] = la;
                        }

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

        private void GetChildAgents_(List<Agent> parentList, DbHelper d)
        {
            List<Agent> l = new List<Agent>();
            SqlDataReader rd = null;

            try
            {
                for (int i = 0; i < parentList.Count; i++)
                {
                    Agent parent = parentList[i];
                    StringBuilder sb = new StringBuilder();
                    sb.Append("select distinct a.agentid, a.agentname, a.agenttype, a.agentlevel, a.agentteam from agent a ")
                        .Append("left join customer c on a.agentid = c.agentid ")
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
                        a.AgentType = rd.Get("agenttype");
                        a.AgentLevel = rd.Get("agentlevel");
                        a.AgentTeam = rd.Get("agentteam");

                        parent.AddChildAgent(a);

                        l.Add(a);
                    }

                    rd.Close();
                    GetChildAgents_(l, d);
                }
            }

            catch (Exception e)
            {
                Logger.Debug("", e);
                throw e;
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
                    .Append("where c.customertype = 1 ")
                    .Append("order by a.agentname");
                string q = sb.ToString();

                rd = d.ExecuteReader(q, CommandType.Text);
                while (rd.Read())
                {
                    Agent a = new Agent();
                    a.AgentID = rd.Get<int>("agentid");
                    a.AgentName = rd.Get("agentname");
                    a.AgentType = rd.Get("agenttype");
                    a.AgentLevel = rd.Get("agentlevel");
                    a.AgentTeam = rd.Get("agentteam");

                    l.Add(a);
                }
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
