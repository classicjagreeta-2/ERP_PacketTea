using System.Web.Mvc;

namespace PacketTea.Utility
{
    /// <summary>
    /// Global safety net for every MVC controller (registered in FilterConfig),
    /// replacing the default HandleErrorAttribute, which did not log anything,
    /// only worked when customErrors was on, and answered AJAX calls with an
    /// HTML page the calling script could not read.
    ///
    /// Any exception an action doesn't handle itself is logged with a reference
    /// id, then returned as:
    ///   - AJAX / JSON callers : HTTP 500 + { success:false, message, errorId }
    ///   - normal page request : Views/Shared/Error.cshtml with the same info
    /// Actions that already catch their own exceptions are unaffected.
    /// </summary>
    public class GlobalExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext filterContext)
        {
            if (filterContext == null || filterContext.ExceptionHandled)
            {
                return;
            }

            // Let a failing child action (Html.Action) bubble up so it is
            // reported once, by the parent request.
            if (filterContext.IsChildAction)
            {
                return;
            }

            var ex = filterContext.Exception;
            var http = filterContext.HttpContext;

            string errorId = ErrorReporting.Log(ex, http, filterContext.RouteData);
            string message = ErrorReporting.UserMessage(ex, http, errorId);
            int statusCode = ErrorReporting.GetStatusCode(ex);

            if (ErrorReporting.WantsJson(http.Request))
            {
                filterContext.Result = new JsonResult
                {
                    Data = new { success = false, message = message, errorId = errorId },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            else
            {
                string controller = filterContext.RouteData.Values["controller"]?.ToString() ?? "";
                string action = filterContext.RouteData.Values["action"]?.ToString() ?? "";

                var viewData = new ViewDataDictionary<HandleErrorInfo>(new HandleErrorInfo(ex, controller, action));
                viewData["ErrorId"] = errorId;
                viewData["ErrorMessage"] = message;
                viewData["StatusCode"] = statusCode;

                filterContext.Result = new ViewResult
                {
                    ViewName = "~/Views/Shared/Error.cshtml",
                    ViewData = viewData,
                    TempData = filterContext.Controller.TempData
                };
            }

            filterContext.ExceptionHandled = true;
            http.Response.Clear();
            http.Response.StatusCode = statusCode;
            // Stop IIS replacing our body with its own generic error page.
            http.Response.TrySkipIisCustomErrors = true;
        }
    }
}
