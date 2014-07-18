using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using CommissionSystem.WebUI.Models;
using CommissionSystem.WebUI.Models.FibrePlus;
using CommissionSystem.WebUI.Helpers;
using NLog;

namespace CommissionSystem.WebUI.Controllers
{
    public class FibrePlusController : Controller
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();
        private const int CUSTOMER_TYPE = 1;
        private const string DB = "HSBB_Billing";

        [HttpPost]
        public ActionResult Commission(int agentID, string from, string to, int agentLevel = -1)
        {
            FibrePlusCommission o = null;
            Dictionary<string, object> r = new Dictionary<string, object>();

            try
            {
                DateTime dateFrom = Utils.GetDateTimeFMT(from);
                DateTime dateTo = Utils.GetDateTimeFMT(to);

                o = new FibrePlusCommission();
                o.DateFrom = new DateTime(dateFrom.Year, dateFrom.Month, dateFrom.Day);

                DateTime _dateTo = new DateTime(dateTo.Year, dateTo.Month, dateTo.Day);
                _dateTo = _dateTo.AddDays(1);
                o.DateTo = _dateTo;

                o.AgentID = agentID;
                o.AgentLevel = agentLevel;
                double comm = o.GetCommission();

                r["success"] = 1;
                r["commission"] = comm;
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

        //
        // GET: /FibrePlus/

        public ActionResult Index()
        {
            FibrePlusCommission o = null;

            try
            {
                DateTime dateFrom = new DateTime(2014, 6, 1);
                DateTime dateTo = new DateTime(2014, 7, 1);
                o = new FibrePlusCommission();
                o.AgentID = 5800014;
                o.AgentLevel = 4;
                o.DateFrom = dateFrom;
                o.DateTo = dateTo;
                double comm = o.GetCommission();
                ViewBag.comm = comm;
            }

            catch (Exception e)
            {
                Logger.Debug("", e);
                throw e;
            }

            finally
            {
                if (o != null)
                    o.Dispose();
            }
            
            return View();
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
                    .Append("where c.customertype = 1 ")
                    .Append("order by a.agentname");
                string q = sb.ToString();

                rd = d.ExecuteReader(q, CommandType.Text);
                while (rd.Read())
                {
                    Agent a = new Agent();
                    object o = rd["agentid"];
                    if (o != DBNull.Value)
                        a.AgentID = Utils.GetValue<int>(o.ToString());

                    o = rd["agentname"];
                    if (o != DBNull.Value)
                        a.AgentName = Utils.GetValue(o.ToString());

                    o = rd["agenttype"];
                    if (o != DBNull.Value)
                        a.AgentType = Utils.GetValue(o.ToString());

                    o = rd["agentlevel"];
                    if (o != DBNull.Value)
                        a.AgentLevel = Utils.GetValue(o.ToString());

                    o = rd["agentteam"];
                    if (o != DBNull.Value)
                        a.AgentTeam = Utils.GetValue(o.ToString());

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
