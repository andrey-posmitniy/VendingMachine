using System.Reflection;
using System.Web.Mvc;
using SimpleInjector;
using SimpleInjector.Integration.Web.Mvc;
using Vm.Areas.VendingMachine.Models.GetChangeAlgorithms;
using Vm.Utils;

namespace Vm
{
    /// <summary>
    /// Конфигуратор IoC
    /// </summary>
    public class IocConfig
    {
        public static void Register()
        {
            var container = new Container();

            // Алгоритм подбора сдачи
            //container.Register<IGetChangeAlgorithm, GreedyGetChangeAlgorithm>(); // Жадный алгоритм
            container.Register<IGetChangeAlgorithm, SmartGetChangeAlgorithm>(); // Умный алгоритм (с полным переборов вариантов)

            // This is an extension method from the integration package.
            container.RegisterMvcControllers(Assembly.GetExecutingAssembly());
            // This is an extension method from the integration package as well.
            container.RegisterMvcIntegratedFilterProvider();
            container.Verify();
            DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(container));
            IoC.Container = container;
        }
    }
}