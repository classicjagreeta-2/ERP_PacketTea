using PacketTea.Utility;
using NLog;
using System.Net;
using System.Web.Mvc;

namespace PacketTea.Controllers
{
    /// <summary>
    /// Receives browser-side errors (window.onerror / unhandled promise
    /// rejections) reported by Scripts/global-error-handler.js and writes them
    /// to the same NLog file as server errors. Deliberately not a
    /// BaseController: errors must still be reported from the login page or
    /// after the session has expired.
    /// </summary>
    public class ErrorController : Controller
    {
        private static readonly Logger logger = LogManager.GetLogger("ClientErrorHandler");
        private const int MaxFieldLength = 4000;

        [HttpPost]
        [ValidateInput(false)] // stack traces / messages routinely contain '<' and '>'
        public ActionResult LogClientError(string kind, string message, string source, string line, string column, string stack, string pageUrl)
        {
            try
            {
                string errorId = ErrorReporting.NewErrorId();
                logger.Error(
                    $"[ClientErrorId {errorId}] {Clip(kind)} on {Clip(pageUrl)} | User={ErrorReporting.CurrentUserName(HttpContext)}"
                    + $" | {Clip(message)} @ {Clip(source)}:{Clip(line)}:{Clip(column)}"
                    + (string.IsNullOrWhiteSpace(stack) ? "" : "\n" + Clip(stack)));
            }
            catch
            {
                // Never let error reporting itself fail the request.
            }

            return new HttpStatusCodeResult(HttpStatusCode.NoContent);
        }

        private static string Clip(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            return value.Length > MaxFieldLength ? value.Substring(0, MaxFieldLength) + "..." : value;
        }
    }
}
