using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc.Filters;
using System.Web.Mvc;
using System.Security.Claims;

namespace PacketTea.Utility
{
    public class AuthAttribute : ActionFilterAttribute, IAuthenticationFilter
    {
        private bool _auth;

        public void OnAuthentication(AuthenticationContext filterContext)
        {
            // Logic for making a user authenticate
            _auth = (filterContext.ActionDescriptor.GetCustomAttributes(
                typeof(OverrideAuthenticationAttribute), true).Length == 0);
        }

        public void OnAuthenticationChallenge(AuthenticationChallengeContext filterContext)
        {
            var user = filterContext.HttpContext.User;
            if (user == null || !user.Identity.IsAuthenticated)
            {
                filterContext.Result = new HttpUnauthorizedResult();
            }
        }
    }
}