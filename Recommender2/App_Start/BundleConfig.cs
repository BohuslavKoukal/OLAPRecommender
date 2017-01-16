using System.Web;
using System.Web.Optimization;

namespace Recommender2
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css",
                      "~/Content/grouped-categories.css",
                      "~/Content/modal-dialog.css"));

            bundles.Add(new ScriptBundle("~/bundles/createdataset").Include(
                    "~/Scripts/Custom/create-dataset.js",
                    "~/Scripts/Custom/modal-dialog.js"
                    ));
            bundles.Add(new ScriptBundle("~/bundles/uploaddata").Include(
                    "~/Scripts/Custom/upload-data.js"
                    ));

            bundles.Add(new ScriptBundle("~/bundles/browsecube").Include(
                    "~/Scripts/Charts/highcharts.js",
                    "~/Scripts/Charts/grouped-categories.js",
                    "~/Scripts/Charts/drilldown.js",
                    "~/Scripts/Charts/data.js",
                    "~/Scripts/Custom/column-drilldown.js",
                    "~/Scripts/Custom/chart-details.js",
                    "~/Scripts/Custom/modal-dialog.js"
                    ));
        }
    }
}
