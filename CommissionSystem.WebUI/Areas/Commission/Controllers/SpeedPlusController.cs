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

        [HttpPost]
        public ActionResult Commission(FibrePlusRequest req)
        {
            SpeedPlusCommission o = null;
            Dictionary<string, object> r = new Dictionary<string, object>();

            try
            {
                DateTime dateFrom = req.DateFrom;
                DateTime dateTo = req.DateTo;

                o = new SpeedPlusCommission();
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
