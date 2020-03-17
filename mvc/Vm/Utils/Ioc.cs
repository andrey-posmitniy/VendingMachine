using System.Collections.Generic;
using SimpleInjector;

namespace Vm.Utils
{
    /// <summary>
    /// Содержит ссылку на основной IoC-контейнер приложения
    /// </summary>
    public class IoC
    {
        /// <summary>
        /// Основной IoC-контейнер приложения
        /// </summary>
        public static SimpleInjector.Container Container { get; set; }
    }
}
