using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Web;
using System.Web.DynamicData;
using System.Web.Mvc;

namespace PacketTea.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetMenu()
        {
            return View("_NavMenu");
            //return PartialView("_Menu");
        }

        public ActionResult Test()
        {
            return View();
        }

        [HttpPost]
        public ActionResult MultipleSubmit(string Name, string Address, string Phone, string Submittype, FormCollection formCollection)
        {
            switch (Submittype)
            {
                case "Save":
                    {
                        break;
                    }
                case "Submit":
                    {
                        break;
                    }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult MultipleButtonNewSubmit(FormCollection formCollection)
        {

            return RedirectToAction("Index");
        }



        [Authorize]
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            var IsAuthenticated = User.Identity.IsAuthenticated;
            //List<Claim> claims = new List<Claim>
            //{
            //    new Claim("Name", "PhoneNumber")
            //};
            //var claim = new Claim("PhoneNumber", "Password");
            //var identity = new ClaimsIdentity(new[] { claim }, "BasicAuthentication"); // this uses basic auth
            //var principal = new ClaimsPrincipal(identity);
            //HttpContext.User = principal;
            //bool result = HttpContext.User.Identity.IsAuthenticated;
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}