using PacketTea.Utility;
using System.Web;
using System.Web.Mvc;

namespace HRMS
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            // Logs every unhandled action exception and returns JSON to AJAX
            // callers / the Error view to page requests. See GlobalExceptionFilter.
            filters.Add(new GlobalExceptionFilter());
        }
    }
}
