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
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
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

        protected void Application_AcquireRequestState(object sender, EventArgs e)
        {
            var context = HttpContext.Current;
            string user = HttpContext.Current.Request.QueryString["MId"];
            string currentPath = context.Request.Url.AbsolutePath.ToLower();

            if (user == "REPRT")
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
