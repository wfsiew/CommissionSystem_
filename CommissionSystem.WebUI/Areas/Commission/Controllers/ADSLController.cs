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
    public class ADSLController : Controller
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        //
        // GET: /Commission/ADSL/

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Commission(FibrePlusRequest req)
        {
            ADSLCommission o = null;
            Dictionary<string, object> r = new Dictionary<string, object>();

            try
            {
                List<SalesParent> l = new List<SalesParent>();
                GetAgents(req.AgentID, l);

                DateTime dateFrom = req.DateFrom;
                DateTime dateTo = req.DateTo;

                o = new ADSLCommission();
                o.SalesParentList = l;
                o.DateFrom = req.DateFrom;
                o.DateTo = req.DateTo.AddDays(1);
                o.SetCommission();

                r["success"] = 1;
                r["agentlist"] = l;
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
            List<SalesParent> l = GetAgents();
            return Json(l, JsonRequestBehavior.AllowGet);
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
