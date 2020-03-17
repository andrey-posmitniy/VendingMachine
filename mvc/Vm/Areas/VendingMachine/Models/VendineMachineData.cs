using System;

namespace Vm.Areas.VendingMachine.Models
{
    /// <summary>
    /// Класс для хранения статического экземпляра модели
    /// </summary>
    public static class VendineMachineData
    {
        private static readonly Lazy<VendingMachineModel> vendingMachineModel = new Lazy<VendingMachineModel>(() => new VendingMachineModel());
        public static VendingMachineModel Model
        {
            get { return vendingMachineModel.Value; }
        }
    }
}