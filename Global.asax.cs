using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Configuration;

namespace HRMS
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // ClosedXML is compiled against DocumentFormat.OpenXml 3.1.1 but bin ships 3.5.1. The
            // Web.config bindingRedirect covers that, but deployed servers have run with a stale
            // Web.config (HTTP 500 on Excel export), so bind any requested version to the bin copy here.
            AppDomain.CurrentDomain.AssemblyResolve += ResolveOpenXml;

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        private static System.Reflection.Assembly ResolveOpenXml(object sender, ResolveEventArgs args)
        {
            var name = new System.Reflection.AssemblyName(args.Name).Name;
            if (name != "DocumentFormat.OpenXml" && name != "DocumentFormat.OpenXml.Framework")
                return null;

            var loaded = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == name);
            if (loaded != null)
                return loaded;

            var path = System.IO.Path.Combine(HttpRuntime.BinDirectory, name + ".dll");
            return System.IO.File.Exists(path) ? System.Reflection.Assembly.LoadFrom(path) : null;
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
 {
     string user = HttpContext.Current.Request.QueryString["MId"];
     var url = HttpContext.Current.Request.Url;
     string subFolder = ConfigurationManager.AppSettings["AppSubfolder"] ?? "";
     if (!string.IsNullOrEmpty(subFolder))
     {
         if (url.Host.Contains("localhost") && url.AbsolutePath.StartsWith("/" + subFolder))
         {
             string newUrl = url.AbsoluteUri.Replace("/" + subFolder, "");
             HttpContext.Current.Response.Redirect(newUrl, true);
         }
     }
 }

        // Controllers of reports implemented in this app (menu items mapped to them in VBMENU_DONE
        // carry MId=REPRT). Add a new ported report's controller name here.
        private static readonly string[] NativeReportControllers = { "fullsstockactual", "stockreport" };

        private static bool IsNativeReport(string lowerPath)
        {
            var seg = (lowerPath ?? "").Trim('/').Split('/');
            return seg.Length > 0 && Array.IndexOf(NativeReportControllers, seg[0]) >= 0;
        }

        protected void Application_AcquireRequestState(object sender, EventArgs e)
        {
            var context = HttpContext.Current;
            string user = HttpContext.Current.Request.QueryString["MId"];
            string currentPath = context.Request.Url.AbsolutePath.ToLower();

            // "REPRT" is the menu group of the Enquiries-and-reports items still served by the
            // legacy report host (ReportsRedirect). Reports ported into this app open here instead.
            if (user == "REPRT" && !IsNativeReport(currentPath))
            {
                string applicationPath = context.Request.ApplicationPath;
                string redirectUrl;
                if (string.IsNullOrEmpty(applicationPath) || applicationPath == "/")
                {
                    redirectUrl =
                        $"/ReportsRedirect/OpenPage?url={HttpUtility.UrlEncode(currentPath)}";
                }
                else
                {
                    redirectUrl =
                        $"{applicationPath.TrimEnd('/')}/ReportsRedirect/OpenPage" +
                        $"?url={HttpUtility.UrlEncode(currentPath)}";
                }
                //string redirectUrl = $"/ReportsRedirect/OpenPage?url={currentPath}";
                context.Response.Redirect(redirectUrl, true);
            }
        }

        protected void Application_PostResolveRequestCache(object sender, EventArgs e)
        {
            var context = HttpContext.Current;
            var routeData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(context));

            if (routeData == null) return;

            string module = routeData.Values["module"]?.ToString();
            string session = routeData.Values["session"]?.ToString();

            if (!string.IsNullOrEmpty(module) && !string.IsNullOrEmpty(session))
            {
                string currentPath = context.Request.Url.AbsolutePath.ToLower();

                if (!currentPath.Contains("receivedata"))
                {
                    string redirectUrl = "/" + module + "/Home/ReceiveData/" + session;
                    context.Response.Redirect(redirectUrl, true);
                }
            }
        }

        //protected void Application_Error()
        //{
        //    Exception exception = Server.GetLastError();
        //    Response.Clear();
        //    HttpException httpException = exception as HttpException;
        //    if (httpException != null)
        //    {
        //        string action;
        //        switch (httpException.GetHttpCode())
        //        {
        //            case 404:
        //                // page not found
        //                action = "HttpError404";
        //                break;
        //            case 500:
        //                // server error
        //                action = "HttpError500";
        //                break;
        //            default:
        //                action = "General";
        //                break;
        //        }
        //        // clear error on server
        //        Server.ClearError();
        //        //Response.Redirect(String.Format("~/Error/{0}/?message={1}", action, exception.Message));
        //    }
        //    else
        //    {
        //        Response.Redirect("~/Auth/Login");
        //    }
        //}
    }
}
