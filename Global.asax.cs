using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Configuration;
using PacketTea.Utility;

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
     PacketTea.Utility.UiLanguage.Apply(new HttpRequestWrapper(HttpContext.Current.Request));
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

        // `lowerPath` is the request's AbsolutePath, which on the server carries the IIS virtual
        // application prefix ("/pt/fullsstockactual/index"); locally the app sits at the site root
        // ("/fullsstockactual/index"). Strip the application path first, or the controller is never
        // the first segment on the server and every native report is bounced to ReportsRedirect (404).
        private static bool IsNativeReport(string lowerPath, string applicationPath)
        {
            var path = lowerPath ?? "";
            var app = (applicationPath ?? "").TrimEnd('/').ToLowerInvariant();
            if (app.Length > 0 && (path == app || path.StartsWith(app + "/")))
                path = path.Substring(app.Length);
            var seg = path.Trim('/').Split('/');
            return seg.Length > 0 && Array.IndexOf(NativeReportControllers, seg[0]) >= 0;
        }

        protected void Application_AcquireRequestState(object sender, EventArgs e)
        {
            var context = HttpContext.Current;
            string user = HttpContext.Current.Request.QueryString["MId"];
            string currentPath = context.Request.Url.AbsolutePath.ToLower();

            // "REPRT" is the menu group of the Enquiries-and-reports items still served by the
            // legacy report host (ReportsRedirect). Reports ported into this app open here instead.
            string applicationPath = context.Request.ApplicationPath;
            if (user == "REPRT" && !IsNativeReport(currentPath, applicationPath))
            {
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

        /// <summary>
        /// Last-chance handler for errors that never reach GlobalExceptionFilter:
        /// unknown URLs (404), errors in the Application_* events above, HTTP
        /// modules, request validation, etc. Logs with a reference id and answers
        /// with JSON (AJAX callers) or a minimal self-contained HTML page.
        /// </summary>
        protected void Application_Error(object sender, EventArgs e)
        {
            Exception ex = Server.GetLastError();
            if (ex == null)
            {
                return;
            }
            if (ex is HttpUnhandledException && ex.InnerException != null)
            {
                ex = ex.InnerException;
            }

            var http = new HttpContextWrapper(Context);
            RouteData routeData = null;
            try
            {
                routeData = RouteTable.Routes.GetRouteData(http);
            }
            catch
            {
                // Route lookup is only for the log line.
            }

            string errorId = ErrorReporting.Log(ex, http, routeData);
            string message = ErrorReporting.UserMessage(ex, http, errorId);
            int statusCode = ErrorReporting.GetStatusCode(ex);

            try
            {
                Server.ClearError();
                Response.Clear();
                Response.TrySkipIisCustomErrors = true;
                Response.StatusCode = statusCode;

                if (ErrorReporting.WantsJson(http.Request))
                {
                    Response.ContentType = "application/json";
                    Response.Write(Newtonsoft.Json.JsonConvert.SerializeObject(new { success = false, message = message, errorId = errorId }));
                }
                else
                {
                    string home = VirtualPathUtility.ToAbsolute("~/");
                    Response.ContentType = "text/html";
                    Response.Write(
                        "<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>Error " + statusCode + "</title></head>" +
                        "<body style=\"font-family:Segoe UI,Arial,sans-serif;background:#0028a9;color:#fff;text-align:center;padding:10vh 16px\">" +
                        "<h1 style=\"font-size:4rem;font-weight:300;margin:0\">" + statusCode + "</h1>" +
                        "<p style=\"max-width:640px;margin:16px auto;font-family:Consolas,monospace\">" + HttpUtility.HtmlEncode(message) + "</p>" +
                        "<p><a style=\"color:#fff\" href=\"javascript:history.back()\">Go Back</a> &nbsp; " +
                        "<a style=\"color:#fff\" href=\"" + HttpUtility.HtmlAttributeEncode(home) + "\">Home</a></p>" +
                        "</body></html>");
                }
            }
            catch
            {
                // Headers were already sent - nothing more can be written. The
                // error itself has been logged above.
            }
        }
    }
}
