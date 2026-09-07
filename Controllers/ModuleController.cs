using PacketTea.Controllers;
using PacketTea.Models;
using PacketTea.Utility;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace PacketTea.Controllers
{
    public class ModuleController : Controller
    {
        public static string _ModuleCode;
        Logger logger = LogManager.GetCurrentClassLogger();

        public async Task<ActionResult> SelectModule()
        {
            var responce = await Services.GetAsync<List<MODULE>>(string.Format("/api/Module/GetAllByUser?Username={0}", Utility.SessionHelper.GetUser().getUserName));// Utility.SessionHelper.GetUser().getUserName
            if ((!responce.IsSuccessStatusCode))
            {
                logger.Warn("Error occured in Api" + responce.StatusCode);
                return RedirectToAction("Login", "Auth");
            }
            return View(responce.Data.ToList());
        }
        public async Task<ActionResult> CompanySelection(string code, string controlschema, string menuschema, string sortby, string mname, string UserName = null, string LastLoginDt = null, string Password = null)
        {
            var userinfo = Utility.SessionHelper.GetUser();

            if (!string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password))
            {
                var model_User = new Model_User()
                {
                    UserName = UserName,
                    Password = Password
                };
                var Return = await Services.PostAsync<UserModels>("/Users/authenticate", model_User);
                if (Return != null)
                {
                    if (Return.Data != null)
                    {
                        userinfo.getToken = Return.Data.accessToken;
                        userinfo.getUserName = Return.Data.username;
                        //Helpers.UserInfo.getToken = Return.Data.accessToken;
                        //Helpers.UserInfo.getUserName = Return.Data.username;
                    }
                }
            }
            //Helpers.UserInfo.ModuleCode = code;
            //((Helpers.UserInfo)Session["UserInfo"]).ModuleCode = code;
            userinfo.ModuleCode = code;
            userinfo.ControlSchema = controlschema;
            userinfo.MenuSchema = menuschema;
            userinfo.ModuleName = mname;
            _ModuleCode = code;
            Utility.SessionHelper.SetUser(userinfo);
            var responce = await Services.GetAsync<List<CompanySelection>>(string.Format("/api/Company/GetByModule?module={0}", code));
            SelectListModel selectListModel = new SelectListModel();

            if ((!responce.IsSuccessStatusCode))
            {
                logger.Warn("Error occured in Api" + responce.StatusCode);
                return RedirectToAction("Login", "Auth");
            }
            else
            {
                var selectedCompany = "";
                var selectedLocation = "";
                var selectedFYear = "";


                var data = responce.Data.ToList();
                if (Request.Cookies["SelectedCompany"] != null)
                {
                    selectedCompany = Request.Cookies["SelectedCompany"].Value.ToString();
                    selectedLocation = Request.Cookies["SelectedLocation"].Value.ToString();
                    selectedFYear = Request.Cookies["SelectedFYear"].Value.ToString();
                }

                if (!string.IsNullOrEmpty(selectedCompany) && !string.IsNullOrEmpty(selectedLocation) && !string.IsNullOrEmpty(selectedFYear))
                {
                    var model = await getCompanies(selectedCompany, selectedLocation, selectedFYear);
                    var CompanyList = model.CompanyList.ToList();
                    foreach (var item in CompanyList.Where(k => k.Text == selectedCompany))
                    {
                        item.Selected = true;
                    }
                    if (model.LocationList != null)
                    {
                        var LocationList = model.LocationList.ToList();
                        foreach (var item in LocationList.Where(k => k.Text == selectedLocation))
                        {
                            item.Selected = true;
                        }
                        selectListModel.LocationList = LocationList;
                    }
                    else
                    {
                        selectListModel.CompanyList = data.Select(x => new SelectListItem { Text = x.Company.ToString(), Value = x.Company.ToString() });
                        selectListModel.LocationList = data.Select(x => new SelectListItem { Text = "", Value = "" });
                    }


                    if (model.FinancialYearList != null)
                    {
                        var FinancialYearList = model.FinancialYearList.Distinct().ToList();
                        foreach (var item in FinancialYearList.Where(k => k.Text == selectedFYear))
                        {
                            item.Selected = true;
                        }
                        selectListModel.FinancialYearList = FinancialYearList;
                    }
                    else
                    {
                        selectListModel.FinancialYearList = data.Select(x => new SelectListItem { Text = "", Value = "" });
                    }

                    selectListModel.CompanyList = CompanyList;
                }
                else
                {
                    selectListModel.CompanyList = data.Select(x => new SelectListItem { Text = x.Company.ToString(), Value = x.Company.ToString() });
                    selectListModel.LocationList = data.Select(x => new SelectListItem { Text = "", Value = "" });
                    selectListModel.FinancialYearList = data.Select(x => new SelectListItem { Text = "", Value = "" });
                }
            }
            return View(selectListModel);
        }



        public async Task<ActionResult> CompanySelectionByPass(string token, string module, string loca, string sortby = null)
        {
            var userinfo = Utility.SessionHelper.GetUser();
            userinfo.getToken = token;
            userinfo.ModuleCode = module;
            Utility.SessionHelper.SetUser(userinfo);
            module = EncryptionHelper.Decrypt(module);
            var responce = await Services.GetAsync<List<CompanySelection>>(string.Format("/api/Company/GetByModule?module={0}", module));
            SelectListModel selectListModel = new SelectListModel();
            if ((!responce.IsSuccessStatusCode))
            {
                logger.Warn("Error occured in Api" + responce.StatusCode);
                return RedirectToAction("Login", "Auth");
            }
            else
            {
                var data = responce.Data.ToList();
                selectListModel.CompanyList = data.Select(x => new SelectListItem { Text = x.Company.ToString(), Value = x.Company.ToString() });
                selectListModel.LocationList = data.Select(x => new SelectListItem { Text = "", Value = "" });
                selectListModel.FinancialYearList = data.Select(x => new SelectListItem { Text = "", Value = "" });
            }
            return View("CompanySelection", selectListModel);
        }

        [HttpPost]
        public async Task<ActionResult> CompanySelection(string CompanyName, string LocationName, string FinancialYear)
        {

            var model = await getCompanies(CompanyName, LocationName, FinancialYear);
            Session["companyList"] = model.CompanyList;
            Session["locationList"] = model.LocationList;
            Session["financialYearList"] = model.FinancialYearList;
            Session["currentCompany"] = string.Format("{0}, {1} [{2}]", CompanyName, LocationName, FinancialYear);
            Session["SelectedCompany"] = CompanyName;
            Session["SelectedLocation"] = LocationName;
            Session["SelectedfinancialYear"] = FinancialYear;
            Session["loca"] = Utility.SessionHelper.GetUser().Loca; // Utility.SessionHelper.GetUser().Loca;
            Session["address"] = Utility.SessionHelper.GetUser().Address; // Utility.SessionHelper.GetUser().Loca;
            Session["docyear"] = Utility.SessionHelper.GetUser().DocYear; // Utility.SessionHelper.GetUser().Loca;
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> CompanySelectionNav(string CompanyName, string LocationName, string FinancialYear, string viewname)
        {
            var model = await getCompanies(CompanyName, LocationName, FinancialYear);
            Session["companyList"] = model.CompanyList;
            Session["locationList"] = model.LocationList;
            Session["financialYearList"] = model.FinancialYearList;
            Session["currentCompany"] = string.Format("{0}, {1} [{2}]", CompanyName, LocationName, FinancialYear);
            Session["SelectedCompany"] = CompanyName;
            Session["SelectedLocation"] = LocationName;
            Session["SelectedfinancialYear"] = FinancialYear;
            Session["loca"] = Utility.SessionHelper.GetUser().Loca; // Utility.SessionHelper.GetUser().Loca;
            Session["address"] = Utility.SessionHelper.GetUser().Address; // Utility.SessionHelper.GetUser().Loca;
            Session["docyear"] = Utility.SessionHelper.GetUser().DocYear; // Utility.SessionHelper.GetUser().Loca;
            return View(viewname);
        }

        private async Task<SelectListModel> getCompanies(string CompanyName, string LocationName, string FinancialYear)
        {
            var modulecode = Utility.SessionHelper.GetUser().ModuleCode;

            if (String.IsNullOrEmpty(modulecode))
            {
                modulecode = _ModuleCode;
            }
            // Utility.SessionHelper.GetUser().ModuleCode;

            SelectListModel model = new SelectListModel();
            try
            {
                if (!string.IsNullOrEmpty(CompanyName))
                {
                    var responce = await Services.GetAsync<List<CompanySelection>>(string.Format("/api/Company/GetByModule?module={0}", modulecode));
                    var data = responce.Data.ToList();
                    model.CompanyList = data.Select(x => new SelectListItem { Text = x.Company.ToString(), Value = x.Company.ToString() });
                    if (!string.IsNullOrEmpty(CompanyName))
                    {
                        var locationsList = data.Where(l => l.Company == CompanyName).SelectMany(k => k.locationSelection);
                        if (locationsList.Count() == 0)
                        {
                            //Session.Clear();
                            //Session.Abandon();
                            Response.Cookies.Add(new HttpCookie("SelectedCompany", ""));
                            Response.Cookies.Add(new HttpCookie("SelectedLocation", ""));
                            Response.Cookies.Add(new HttpCookie("SelectedFYear", ""));

                            var userinfo = Utility.SessionHelper.GetUser();
                            userinfo.ModuleCode = _ModuleCode;
                            Utility.SessionHelper.SetUser(userinfo);
                            return model;
                        }
                        var distinctLocation = locationsList.Select(l => l.Location).Distinct();
                        model.LocationList = distinctLocation.Select(l => new SelectListItem { Text = l, Value = l });
                        model.FinancialYearList = data.Select(x => new SelectListItem { Text = "Please select", Value = "" });
                        if (!string.IsNullOrEmpty(LocationName))
                        {
                            var fyear = locationsList.Select(k => k.FinancialYear).Distinct().ToList();
                            model.FinancialYearList = fyear.Select(l => new SelectListItem { Text = l, Value = l });
                        }

                        if (!string.IsNullOrEmpty(FinancialYear))
                        {
                            var datab = locationsList.Where(l => l.Location == LocationName && l.FinancialYear == FinancialYear).Select(k => new { k.Database, k.Loca, k.Address, k.DocYear }).FirstOrDefault();
                            if (datab == null)
                            {
                                ////Session.Clear();
                                //Session.Abandon();
                                Response.Cookies.Add(new HttpCookie("SelectedCompany", ""));
                                Response.Cookies.Add(new HttpCookie("SelectedLocation", ""));
                                Response.Cookies.Add(new HttpCookie("SelectedFYear", ""));
                                var userinfo = Utility.SessionHelper.GetUser();
                                userinfo.ModuleCode = _ModuleCode;
                                Utility.SessionHelper.SetUser(userinfo);
                                return model;
                            }
                            var user = SessionHelper.GetUser();
                            user.getDbName = datab.Database;
                            user.Loca = datab.Loca;
                            user.Address = datab.Address;
                            user.DocYear = datab.DocYear;
                            user.ModuleCode = _ModuleCode;

                            user.CurentCompany = CompanyName;
                            user.CurentLocation = LocationName;
                            user.Financialyear = FinancialYear;

                            SessionHelper.SetUser(user);
                            Response.Cookies.Add(new HttpCookie("SelectedCompany", CompanyName));
                            Response.Cookies.Add(new HttpCookie("SelectedLocation", LocationName));
                            Response.Cookies.Add(new HttpCookie("SelectedFYear", FinancialYear));
                        }
                    }
                }
                else
                {
                    var responce = await Services.GetAsync<List<CompanySelection>>(string.Format("/api/Company/GetByModule?module={0}", Utility.SessionHelper.GetUser().ModuleCode));// Utility.SessionHelper.GetUser().ModuleCode
                    var data = responce.Data.ToList();
                    model.CompanyList = data.Select(x => new SelectListItem { Text = x.Company.ToString(), Value = x.Company.ToString() });
                    model.LocationList = data.Select(x => new SelectListItem { Text = "", Value = "" });
                    model.FinancialYearList = data.Select(x => new SelectListItem { Text = "", Value = "" });
                }
                Session["companyList"] = model.CompanyList;
                Session["locationList"] = model.LocationList;
                Session["financialYearList"] = model.FinancialYearList;
                Session["currentCompany"] = string.Format("{0}, {1} [{2}]", CompanyName, LocationName, FinancialYear);
                Session["SelectedCompany"] = CompanyName;
                Session["SelectedLocation"] = LocationName;
                Session["SelectedfinancialYear"] = FinancialYear;
                Session["loca"] = Utility.SessionHelper.GetUser().Loca; // Utility.SessionHelper.GetUser().Loca;
                Session["address"] = Utility.SessionHelper.GetUser().Address; // Utility.SessionHelper.GetUser().Loca;
                Session["docyear"] = Utility.SessionHelper.GetUser().DocYear; // Utility.SessionHelper.GetUser().Loca;
            }
            catch (Exception ex)
            {
                //Session.Clear();
                //Session.Abandon();
                Response.Cookies.Add(new HttpCookie("SelectedCompany", ""));
                Response.Cookies.Add(new HttpCookie("SelectedLocation", ""));
                Response.Cookies.Add(new HttpCookie("SelectedFYear", ""));
                var userinfo = Utility.SessionHelper.GetUser();
                userinfo.ModuleCode = _ModuleCode;
                Utility.SessionHelper.SetUser(userinfo);
                throw;
            }
            return model;
        }
    }

    public class SelectListModel
    {
        public IEnumerable<SelectListItem> CompanyList { get; set; }
        public IEnumerable<SelectListItem> LocationList { get; set; }
        public IEnumerable<SelectListItem> FinancialYearList { get; set; }

        public string CompanyName { get; set; }
        public string LocationName { get; set; }
        public string FinancialYear { get; set; }
    }
}