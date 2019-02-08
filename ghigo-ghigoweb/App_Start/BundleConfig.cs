using System.Web;
using System.Web.Optimization;

namespace GhigoWeb
{
    public class BundleConfig
    {
        // Per ulteriori informazioni sul Bundling, visitare il sito Web all'indirizzo http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-{version}.js",
                        "~/Scripts/jquery-ui-i18n.js",
                        "~/Scripts/jquery.treegrid.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.unobtrusive*",
                        "~/Scripts/jquery.validate*"));

            // Utilizzare la versione di sviluppo di Modernizr per eseguire attività di sviluppo e formazione. Successivamente, quando si è
            // pronti per passare alla produzione, utilizzare lo strumento di compilazione disponibile all'indirizzo http://modernizr.com per selezionare solo i test necessari.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new StyleBundle("~/Content/css/kickstart-css").Include(
                //"~/Content/site.css",
                        "~/Content/css/kickstart.css",
                        "~/Content/css/kickstart-buttons.css",
                        "~/Content/css/kickstart-forms.css",
                        "~/Content/css/kickstart-menus.css",
                        "~/Content/css/kickstart-grid.css",
                        "~/Content/css/kickstart-icons.css",
                        "~/Content/css/jquery.fancybox-1.3.4.css",
                        "~/Content/css/prettify.css",
                        "~/Content/css/chosen.css",
                        "~/Content/css/tiptip.css"));

            bundles.Add(new StyleBundle("~/Content/site-css").Include(
                //"~/Content/site.css",
                        "~/Content/style.css",
                        "~/Content/toastr.css",                        
                        "~/Content/Gridmvc.css",
                        "~/Content/jquery.treegrid.css"));

            bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
                        "~/Content/themes/base/jquery.ui.core.css",
                        "~/Content/themes/base/jquery.ui.resizable.css",
                        "~/Content/themes/base/jquery.ui.selectable.css",
                        "~/Content/themes/base/jquery.ui.accordion.css",
                        "~/Content/themes/base/jquery.ui.autocomplete.css",
                        "~/Content/themes/base/jquery.ui.button.css",
                        "~/Content/themes/base/jquery.ui.dialog.css",
                        "~/Content/themes/base/jquery.ui.slider.css",
                        "~/Content/themes/base/jquery.ui.tabs.css",
                        "~/Content/themes/base/jquery.ui.datepicker.css",
                        "~/Content/themes/base/jquery.ui.progressbar.css",
                        "~/Content/themes/base/jquery.ui.theme.css"));

            bundles.Add(new ScriptBundle("~/bundles/kickstart").Include(
                        "~/Scripts/prettify.js",
                        "~/Scripts/kickstart.js"));

            // toastr
            bundles.Add(new ScriptBundle("~/bundles/toastr").Include(
                        "~/Scripts/toastr.js"));

            // gridmvc
            bundles.Add(new ScriptBundle("~/bundles/gridmvc").Include(
                        "~/Scripts/gridmvc.js"));
        }
    }
}