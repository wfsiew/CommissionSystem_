using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CommissionSystem.Domain.Models;
using CommissionSystem.WebUI.Models;

namespace CommissionSystem.WebUI.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            var x = SettingFactory.Instance;
            var v = x.DiscountedCallServiceInternalSetting[0.17];
            double a = v.GetCommission(1000, 0);
            ViewBag.value = a;
            return View(v);
        }

    }
}
