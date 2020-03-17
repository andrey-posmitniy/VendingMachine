using System.Web.Mvc;

namespace Vm.Controllers
{
    public class HomeController : Controller
    {

        public ActionResult Index()
        {
            return RedirectToAction("Index", "VendingMachine", new { area = "VendingMachine" });
        }
    }
}