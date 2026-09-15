
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace PacketTea
{
    public class UnAuthorize : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            if (HttpContext.Current.Request.Cookies["cc_timeout"] != null)
            {
                var isTimeout = Convert.ToBoolean(HttpContext.Current.Request.Cookies["cc_timeout"].Value);
                if (!isTimeout)
                {
                    //context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                    //{
                    //    controller = "Home",
                    //    action = "Index"
                    //}));
                }
            }
        }
    }
}