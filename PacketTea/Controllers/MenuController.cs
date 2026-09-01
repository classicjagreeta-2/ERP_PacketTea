
using PacketTea.Models;
using PacketTea;
using PacketTea.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static PacketTea.Helpers;

namespace PacketTea.Controllers
{
    //[UnAuthorize]
    public class MenuController : Controller
    {
        // GET: Menu
        public async Task<ActionResult> Index()
        {
            try
            {
                //var ddd = new
                //{
                //    DefaultCompany = "",
                //    DefaultFinancialYear = "",
                //    DefaultLocation = ""
                //};
                //var response1 = await Services.PostAsync<JObject>("/api/Menu/GetAll?moduleCode={0}", ddd); // Utility.SessionHelper.GetUser().ModuleCode
                var response = await Services.GetAsync<JObject>(string.Format("/api/Menu/GetAll?moduleCode={0}", Utility.SessionHelper.GetUser().ModuleCode)); // Utility.SessionHelper.GetUser().ModuleCode
                if (response.IsSuccessStatusCode)
                {
                    var dbResp = await Services.GetAsync<CLASSIC_CONTROL>("/api/Invoice/boughtleafbcheck");

                    if (dbResp?.Data == null)
                    {
                        throw new Exception("PacketTea database mapping not found.");
                    }
                    var userinfo = Utility.SessionHelper.GetUser();
                    //userinfo.Factorydb = dbResp.Data.SCHEMA_FACTORY;

                    //var expiresMin = Session["expiresMin"];
                    //var expiresTime = (TimeSpan)Session["expiresTime"];
                    var currentTime = DateTime.Now.TimeOfDay;
                    //var dd = (currentTime - expiresTime).Minutes;
                    Session["MenuList"] = response.Data["manuDetails"];
                    var User_AEDV = response.Data["usracS_AEDV"];
                    var AEDVJson = (JsonConvert.SerializeObject(User_AEDV));
                    var _AEDV = JsonConvert.DeserializeObject<List<AEDV>>(AEDVJson);
                    Session["User_AEDV"] = _AEDV;
                   
                    return View();
                }
                else
                {
                    TempData["toastrError"]= response.Message;
                }
            }
            catch (TaskCanceledException)
            {
                
                //return RedirectToAction("Index");
            }
            
            return RedirectToAction("Login", "Auth");
        }

        public async Task<ActionResult> GetMenu()
        {
            var responce = await Services.GetAsync<List<MODULE>>(string.Format("/api/Module/GetAll", Utility.SessionHelper.GetUser().getUserName));//  Utility.SessionHelper.GetUser().getUserName
            if (responce.IsSuccessStatusCode)
                Session["MenuList"] = responce;
            return View();
        }
    }
}