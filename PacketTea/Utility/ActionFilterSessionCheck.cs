using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace PacketTea.Utility
{
    public class ActionFilterSessionCheck : ActionFilterAttribute, IActionFilter
    {
        void IActionFilter.OnActionExecuting(ActionExecutingContext filterContext)
        {
            string userName = null;
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                userName = filterContext.HttpContext.User.Identity.Name;

                //ERROR***
                //string userID = filterContext.HttpContext.User.Identity.GetUserID();

            }

            string sessionLogin = "Login" + userName;
            if (userName == null || (System.Web.HttpContext.Current.Session[sessionLogin] == null))
            {

                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    //AJAX request
                    throw new Exception("Session time out");
                }
                else
                {
                    // Standard request
                    filterContext.Result = new RedirectToRouteResult(
                                       new RouteValueDictionary
                                       {
                                       { "action", "Login" },
                                       { "controller", "Account" }
                                       });
                }
            }


            //Call standard behavior
            this.OnActionExecuting(filterContext);
        }
    }
}