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
using ProtoBuf;
using PagedList;
using NLog;

namespace CommissionSystem.WebUI.Areas.Commission.Controllers
{
    public class DiscountedCallServiceController : Controller
    {
        private const string COMMISSION_RESULT = "DCS_COMMISSION_RESULT";
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        //
        // GET: /Commission/DiscountedCallService/

        public ActionResult Index()
        {
            ViewBag.Menu = Constants.DISCOUNTED_CALL_SERVICE;
            return View();
        }

        public ActionResult AgentSummary()
        {
            VoiceTask o = null;

            try
            {
                ViewBag.Menu = Constants.AGENT_STRUCTURE_VOICE;
                Dictionary<int, List<SalesParent>> dic = new Dictionary<int, List<SalesParent>>();
                List<SalesParent> l = new List<SalesParent>();
                o = new VoiceTask();
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
                VoiceCommissionResult re = new VoiceCommissionResult();
                VoiceCommissionResult c = new VoiceCommissionResult();

                if (req.Load)
                {
                    c = Session[COMMISSION_RESULT] as VoiceCommissionResult;
                }

                else
                {
                    string file = GetFile(req.DateFrom);
                    if (string.IsNullOrEmpty(file))
                        throw new UIException(string.Format("The Commission for {0:MMMM yyyy} is not available yet, please contact the respective personel to generate the commission", req.DateFrom));

                    fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
                    re = Serializer.Deserialize<VoiceCommissionResult>(fs);
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

                re = new VoiceCommissionResult();

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

        public FileContentResult DownLoad(string from, string to)
        {
            FileContentResult f = null;
            string contentType = "application/vnd.ms-excel";

            DateTime dateFrom = Utils.GetDateTimeFMT(from);
            DateTime dateTo = Utils.GetDateTimeFMT(to);

            VoiceCommissionResult c = Session[COMMISSION_RESULT] as VoiceCommissionResult;
            Attachment att = c.GetVoiceCommissionResultData(dateFrom, dateTo);

            f = File(att.Data, contentType, att.Filename);

            return f;
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
                        "roshana.bedah@redtone.com",
                        "chinchin.lee@redtone.com"
                    };

                EmailInfo emailInfo = new EmailInfo
                {
                    ToList = l,
                    DisplayName = "Discounted Call Service Commission",
                    Subject = "REDtone Discounted Call Service Commission"
                };

                VoiceCommissionResult c = Session[COMMISSION_RESULT] as VoiceCommissionResult;

                if (c == null)
                    throw new UIException("There is no commission result");

                ViewData["DateFrom"] = Utils.FormatDateTime(req.DateFrom);
                ViewData["DateTo"] = Utils.FormatDateTime(req.DateTo);

                Attachment att = c.GetVoiceCommissionResultData(req.DateFrom, req.DateTo);

                if (att != null)
                    emailInfo.AttList = new List<Attachment> { att };

                new CommissionMailController().CommissionNotificationEmail(c, emailInfo, ViewData,
                    CommissionMailController.COMMISSIONNOTIFICATION_VOICE).DeliverAsync();

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
            string file = Path.Combine(c, string.Format("voice/dcs/{0:yyyy}/{1:MM}/CommResult_.bin", dt, dt));

            if (!System.IO.File.Exists(file))
            {
                file = null;
            }

            return file;
        }

        private List<SalesParent> GetAgents()
        {
            List<SalesParent> l = new List<SalesParent>();
            DbHelper d = null;
            SqlDataReader rd = null;

            try
            {
                d = new DbHelper(DbHelper.GetConStr(Constants.CALLBILLING2));
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
