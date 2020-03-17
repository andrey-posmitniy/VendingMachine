using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vm.Areas.VendingMachine.Models.GetChangeAlgorithms
{
    /// <summary>
    /// Интерфейс классов подбора сдачи
    /// </summary>
    public interface IGetChangeAlgorithm
    {
        /// <summary>
        /// Подобрать сдачу минимальным количеством монет
        /// </summary>
        List<CoinsArray> Process(VendingMachineModel model);
    }
}