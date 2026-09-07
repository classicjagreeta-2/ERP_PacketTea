
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PacketTea.Models;
namespace PacketTea.Utility
{
    public static class SessionHelper
    {
        public static void SetUser(Helpers.UserInfo user)
        {
            HttpContext.Current.Session["UserInfo"] = user;
        }

        public static Helpers.UserInfo GetUser()
        {
            var context = HttpContext.Current;

            if (context?.Session == null)
                return null;

            if (context.Session["UserInfo"] == null)
            {
                context.Session["UserInfo"] = new Helpers.UserInfo();
            }

            return (Helpers.UserInfo)context.Session["UserInfo"];
        }

        public static void RemoveUser()
        {
            HttpContext.Current.Session.Remove("UserInfo");
        }
    }
}