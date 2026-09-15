
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Security.Claims;
using NLog;
using System.Web.Configuration;

using System.Configuration;
//using static Org.BouncyCastle.Math.EC.ECCurve;
using System.Threading;
using PacketTea.Utility;
//using Microsoft.VisualBasic.ApplicationServices;
using DocumentFormat.OpenXml.Spreadsheet;
using PacketTea;
using PacketTea.Models;

namespace PacketTea.Controllers
{
    public class AuthController : Controller
    {

        Logger logger = LogManager.GetCurrentClassLogger();
        // GET: Login
        public ActionResult Login(Model_User model_User)
      {

            Session["expiresMin"] = null;
            Session["expiresTime"] = null;

            if (Request.Cookies["_cc_timeout"] != null)
            {
                Response.Cookies.Remove("_cc_timeout");
                Response.Cookies.Add(new HttpCookie("_cc_timeout", "false"));
            }
            else
            {
                Response.Cookies.Add(new HttpCookie("_cc_timeout", "false"));
            }
            ViewBag.ErrorMsg = TempData["toastrError"];
            return View(model_User);
        }
        public async Task<ActionResult> Auth(Model_User model_User)
        {
            ViewBag.ErrorMsg = string.Empty;
            var Return = await Services.PostAsync<UserModels>("/Users/authenticate", model_User);
            if (Return != null)
            {
                if (Return.Data != null)
                {
                    var user = new  Helpers.UserInfo
                    {
                        getToken = Return.Data.accessToken,
                        getUserName = Return.Data.username,
                        UserPassword = model_User.Password,
                        getUserfullName = Return.Data.firstName,
                    };
                    SessionHelper.SetUser(user);

                    // Utility.SessionHelper.GetUser().getToken = Return.Data.accessToken;
                    // Utility.SessionHelper.GetUser().getUserName = Return.Data.username;
                    ///Configuration config = WebConfigurationManager.OpenWebConfiguration("~/Web.Config");
                    ///SessionStateSection section = (SessionStateSection)config.GetSection("system.web/sessionState");
                    int timeoutMin = (int)Return.Data.expiresMin * 1000 * 60;
                    var timeout = Return.Data.expiresTime;
                    Session["expiresMin"] = timeoutMin;
                    Session["expiresTime"] = timeout;

                    //Response.Cookies.Remove("cc_timeout");
                    //Response.Cookies.Add(new HttpCookie("cc_timeout", "true"));

                    return Json(new { result = "Redirect", url = Url.Action("SelectModule", "Module") }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    ModelState.AddModelError("CustomError", Return.Message);
                    ViewBag.ErrorMsg = Return.Message;
                    return Json(Return); //View("Login");
                }
            }
            else
            {
                logger.Warn("Error occured in Home controller Index Action");
                return Json(new { result = "Redirect", url = Url.Action("Login") });
            }
        }
        public async Task<ActionResult> Refreshtoken()
        {
            var tokenModel = new
            {
                accessToken =  Utility.SessionHelper.GetUser().getToken, // Utility.SessionHelper.GetUser().getToken,
                refreshToken = Guid.NewGuid()
            };
            var Return = await Services.PostAsync<UserModels>("/Users/refresh", tokenModel);
            var user = new Helpers.UserInfo
            {
                getToken = Return.Data.accessToken,
                getUserName = Return.Data.username
            };
            SessionHelper.SetUser(user);

            // Utility.SessionHelper.GetUser().getToken = Return.Data.accessToken;
            // Utility.SessionHelper.GetUser().getUserName = Return.Data.username;
            int timeoutMin = (int)Return.Data.expiresMin * 60 * 1000;
            var timeout = Return.Data.expiresTime;
            Session["expiresMin"] = timeoutMin;
            Session["expiresTime"] = timeout;
            return Json(new { expirestime = timeoutMin });
        }
        public async Task<ActionResult> keepAlive()
        {
            return Json(new { result = true });
        }
    }

    public class UserModels
    {
        public int id { get; set; }
        public string firstName { get; set; }
        public object lastName { get; set; }
        public string username { get; set; }
        public string accessToken { get; set; }
        public string refreshToken { get; set; }
        public double expiresMin { get; set; }
        public TimeSpan expiresTime { get; set; }
    }
}