using System.Web.Mvc;
using Vm.Areas.VendingMachine.Models;
using Vm.Controllers;

namespace Vm.Areas.VendingMachine.Controllers
{
    /// <summary>
    /// Контроллер вендинговой машины
    /// </summary>
    public class VendingMachineController : BaseController
    {
        /// <summary>
        /// Страница отображения состояния вендинговой машины
        /// </summary>
        public ActionResult Index()
        {
            return View(VendineMachineData.Model);
        }


        /// <summary>
        /// Опустить монету в машину
        /// </summary>
        /// <param name="coinValue">Номинал монеты</param>
        [HttpPost]
        public ActionResult ReplenishDeposit(int coinValue)
        {
            VendineMachineData.Model.ReplenishDeposit(coinValue);
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Получить сдачу
        /// </summary>
        [HttpPost]
        public ActionResult GetChange()
        {
            VendineMachineData.Model.GetChange();
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Выдать продукт
        /// </summary>
        [HttpPost]
        public ActionResult IssueProduct(int productId)
        {
            VendineMachineData.Model.IssueProduct(productId);
            return RedirectToAction("Index");
        }
    }
}