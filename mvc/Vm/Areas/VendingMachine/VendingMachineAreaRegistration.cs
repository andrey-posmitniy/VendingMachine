using System.Web.Mvc;

namespace Vm.Areas.VendingMachine
{
    public class VendingMachineAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "VendingMachine";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            //context.MapRoute("convictions", "convictions", new { controller = "Convictions", action = "Index", id = UrlParameter.Optional });
            //context.MapRoute("convictionsSave", "save", new { controller = "Convictions", action = "Save", id = UrlParameter.Optional });
            context.MapRoute(
                "VendingMachine_default",
                "VendingMachine/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}