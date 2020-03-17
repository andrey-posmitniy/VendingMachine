using System.Web.Optimization;

namespace Vm
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            // Скрипты
            bundles.Add(new ScriptBundle("~/bundles/system-scripts").Include(
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/bootstrap.js"
                ));

            // Стили
            bundles.Add(new StyleBundle("~/bundles/common-styles").Include(
                "~/Content/bootstrap.css",
                "~/Content/Site.css"));
            bundles.Add(new StyleBundle("~/bundles/vendingmachine-styles").Include(
                "~/Areas/VendingMachine/Content/vendingMachine.css"));
        }
    }
}
