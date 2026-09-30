using NLog;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Routing;

namespace PacketTea.Utility
{
    /// <summary>
    /// Shared helpers for the global exception handling pipeline
    /// (GlobalExceptionFilter for MVC actions, Application_Error for everything
    /// else, and ErrorController.LogClientError for browser-side errors).
    ///
    /// Every unhandled error is logged once with a short reference id, and that
    /// same id is shown to the user - so a screenshot of the error popup is
    /// enough to find the full stack trace in the NLog file (Log\yyyy-MM-dd.log).
    /// </summary>
    public static class ErrorReporting
    {
        private static readonly Logger logger = LogManager.GetLogger("GlobalErrorHandler");

        public const string GenericMessage = "An unexpected error occurred while processing your request.";

        public static string NewErrorId()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 8).ToUpperInvariant();
        }

        public static int GetStatusCode(Exception ex)
        {
            if (ex is HttpException httpEx)
            {
                int code = httpEx.GetHttpCode();
                return code >= 400 ? code : 500;
            }
            return 500;
        }

        /// <summary>
        /// Logs the exception with request context and returns the reference id.
        /// 404s are logged as warnings (usually a broken link / missing script),
        /// everything else as errors.
        /// </summary>
        public static string Log(Exception ex, HttpContextBase http, RouteData routeData)
        {
            string errorId = NewErrorId();

            try
            {
                string method = SafeGet(() => http?.Request?.HttpMethod) ?? "";
                string url = SafeGet(() => http?.Request?.RawUrl) ?? "";
                string user = CurrentUserName(http);
                string controller = SafeGet(() => routeData?.Values["controller"]?.ToString()) ?? "";
                string action = SafeGet(() => routeData?.Values["action"]?.ToString()) ?? "";
                string kind = SafeGet(() => WantsJson(http?.Request)) ? "AJAX" : "PAGE";

                string message = $"[ErrorId {errorId}] {method} {url} | {kind} | User={user}"
                    + (controller.Length > 0 ? $" | {controller}/{action}" : "");

                if (GetStatusCode(ex) == 404)
                {
                    logger.Warn(ex, message);
                }
                else
                {
                    logger.Error(ex, message);
                }
            }
            catch
            {
                // Logging must never become a second failure on top of the first.
            }

            return errorId;
        }

        public static string CurrentUserName(HttpContextBase http)
        {
            string name = SafeGet(() => (http?.Session?["UserInfo"] as PacketTea.Helpers.UserInfo)?.getUserName);
            return string.IsNullOrWhiteSpace(name) ? "(anonymous)" : name;
        }

        /// <summary>
        /// Message safe to show the user. Local requests (developer on the
        /// server itself) get the full inner-exception chain; remote users get
        /// a generic message so SQL/stack details are not exposed.
        /// </summary>
        public static string UserMessage(Exception ex, HttpContextBase http, string errorId)
        {
            int status = GetStatusCode(ex);
            string text;

            if (status == 404)
            {
                text = "The requested page or resource was not found.";
            }
            else if (ex is HttpRequestValidationException)
            {
                text = "The request contains characters that are not allowed (for example '<' or '>').";
            }
            else if (SafeGet(() => http?.Request?.IsLocal ?? false))
            {
                var messages = new List<string>();
                for (var current = ex; current != null; current = current.InnerException)
                {
                    messages.Add(current.Message);
                }
                text = string.Join(" ---> ", messages);
            }
            else
            {
                text = GenericMessage;
            }

            return text + " (Reference: " + errorId + ")";
        }

        /// <summary>
        /// True when the caller is script (jQuery AJAX, or fetch asking for JSON)
        /// and expects a JSON body rather than an HTML error page.
        /// </summary>
        public static bool WantsJson(HttpRequestBase request)
        {
            if (request == null) return false;

            if (string.Equals(request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            string accept = request.Headers["Accept"] ?? "";
            if (accept.IndexOf("application/json", StringComparison.OrdinalIgnoreCase) >= 0
                && accept.IndexOf("text/html", StringComparison.OrdinalIgnoreCase) < 0)
            {
                return true;
            }

            return (request.ContentType ?? "").StartsWith("application/json", StringComparison.OrdinalIgnoreCase);
        }

        private static T SafeGet<T>(Func<T> getter)
        {
            try { return getter(); }
            catch { return default(T); }
        }
    }
}
