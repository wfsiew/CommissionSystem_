using System.Web.Mvc;

namespace CommissionSystem.WebUI.Areas.Commission
{
    public class CommissionAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Commission";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Commission_default",
                "Commission/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
