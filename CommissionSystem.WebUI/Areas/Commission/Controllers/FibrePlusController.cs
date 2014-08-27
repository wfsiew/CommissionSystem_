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
    public class FibrePlusController : Controller
    {
        private const string COMMISSION_RESULT = "FIBRE+_COMMISSION_RESULT";
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        //
        // GET: /Commission/FibrePlus/

        public ActionResult Index()
        {
            ViewBag.Menu = Constants.FIBREPLUS;
            return View();
        }

        public ActionResult AgentSummary()
        {
            FibrePlusTask o = null;

            try
            {
                ViewBag.Menu = Constants.AGENT_STRUCTURE_FIBREPLUS;
                Dictionary<int, List<Agent>> dic = new Dictionary<int, List<Agent>>();
                List<Agent> l = new List<Agent>();
                o = new FibrePlusTask();
                o.GetTopLevelAgents(l, dic);
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
                        List<AgentView> agentViewList = re.AgentViewList.OrderBy(x => x.AgentID).ToList();
                        c = re;
                        c.AgentViewList = agentViewList;
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
                    DisplayName = "Fibre+ Commission",
                    Subject = "REDtone Fibre+ Commission"
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

        private string GetFile(DateTime dt)
        {
            string c = HttpContext.Server.MapPath("~/result");
            string file = Path.Combine(c, string.Format("fibre+/{0:yyyy}/{1:MM}/CommResult.bin", dt, dt));

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
