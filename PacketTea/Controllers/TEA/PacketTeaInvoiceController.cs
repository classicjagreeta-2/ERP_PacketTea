using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using DocumentFormat.OpenXml.Vml.Office;
using DocumentFormat.OpenXml.Wordprocessing;
using PacketTea.Models;
using PacketTea.Models.Master;
using PacketTea.Models.PT;
using PacketTea.Models.PT.PacketTea.Models.PT;


//using Finance.Models;
//using Finance.Models.DTO;
//using Finance.Models.Master;
//using Finance.Models.Master.Finance.Models.Master;
using PagedList;
using SixLabors.Fonts;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using static PacketTea.Helpers;


namespace PacketTea.Controllers.PacketTea
{
    public class PacketTeaInvoiceController : Controller
    {
        // GET: PacketTeaInvoice
        public async Task<ActionResult> Index(string sortOrder, string currentFilter, string searchString, int? page = 1, int pageSize = 25)
        {
            var permissions = (List<AEDV>)Session["User_AEDV"];
            ViewBag.Permission = permissions?.FirstOrDefault(x => x.Controller == "PacketTeaInvoice");

            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";
            ViewBag.CurrentFilter = searchString;
            ViewBag.PageSize = pageSize;
            
            int pageNumber = page ?? 1;
            ViewBag.Page = pageNumber;
            ViewBag.LOCA = Utility.SessionHelper.GetUser().Loca;
            var response = await Services.FinanceGetAsync<PageModel<UNBL_HED>>($"/api/Invoice/GetByPage?page={pageNumber}&pageSize={pageSize}&search={searchString}");
            var list = response.Data?.value?.results?.ToList() ?? new List<UNBL_HED>();
            var totalRows = response.Data?.value?.rowCount ?? 0;

            var pagedList = new StaticPagedList<UNBL_HED>(list, pageNumber, pageSize, totalRows);

            if (Request.IsAjaxRequest())
                return PartialView("_PT_List", pagedList);

            return View(pagedList);
        }
        public async Task<ActionResult> InsertOrUpdate(string id = "", string unit = "", string loca ="")
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                var sessionLoca = Utility.SessionHelper.GetUser().Loca;

                var vm_new = new UNBLDATA
                {
                    unbL_HED = new UNBL_HED
                    {
                        unit = unit,
                        loca = string.IsNullOrWhiteSpace(loca) ? sessionLoca : loca,
                        flag = "2"
                    },
                    unbl = new List<UNBL>
                    {
                       new UNBL()
                    },
                    unblplt = new List<UNBLPLT>
                    {
                        new UNBLPLT()
                    }
                };

                return View(vm_new);
            }
            var decryptedId = Utility.Cryptography.Decrypt(id);
            var deResponse = await Services.FinanceGetAsync<UNBLDATA>($"/api/Invoice/GetByID?id={decryptedId}");
            if (!deResponse.IsSuccessStatusCode)
            {
                TempData["toastrError"] = deResponse.Message ?? "Unable to load data.";
                return RedirectToAction("Login", "Auth");
            }
            var header = deResponse.Data.unbL_HED;
            var details = deResponse.Data.unbl;
            var freeResp = deResponse.Data.unblplt;
            var vm = new UNBLDATA
            {
                unbL_HED = header,
                unbl = details,
                unblplt = freeResp
            };
            vm.unbL_HED.flag = vm.unbl.FirstOrDefault()?.flag ?? "";
            return View(vm);
        }
        //public async Task<> getFreeTeaDetailsByItc (string itc)
        //{
        //    var deResponse = await Services.GetAsync<UNBLDATA>($"/api/Invoice/GetByID?id={decryptedId}");
        //}
        public async Task<JsonResult> GetUnit(string q = "", int limit = 10, string fieldValue = "", string fieldText = "", string value = "", int p = 1)
        {
            var resp = await Services.FinanceGetAsync<List<UNIT>>("/api/MasterUnit/GetAll");
            var all = resp?.Data ?? new List<UNIT>();

            // 🔍 Search filter
            if (!string.IsNullOrWhiteSpace(q))
            {
                all = all.Where(x => (x.CODE != null && x.CODE.Contains(q)) || (x.NAME != null && x.NAME.Contains(q)) || (x.LOCA != null && x.LOCA.Contains(q))).ToList();
            }
            var count = all.Count;

            // 📄 Paging
            var data = all.Skip((p - 1) * limit).Take(limit).Select(x => new { CODE = x.CODE, NAME = x.NAME, LOCA = x.LOCA }).ToList();
            return Json(new { data = data, count = count }, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> GetBankCode(int aCodePrefix = 0, string searchString = "", string unit = "", string q = "", int limit = 0, string fieldValue = "", string fieldText = "", string value = "", int p = 0)
        {
            if (string.IsNullOrEmpty(q)) q = value;
            var resp = await Services.FinanceGetAsync<List<M_BH21>>($"/api/BH21/GetByACodePrefix?aCodePrefix={aCodePrefix:D2}&Unit={unit}");
            var data = resp?.Data;
            var results = resp?.Data ?? new List<M_BH21>();
            foreach (var item in results) { item._acodeName = item.ACODE + " " + item.ACNAME; }

            return Json(new { data = data, count = data?.Count ?? 0 }, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> GetPartyCode(string q = "", int limit = 0, string fieldValue = "", string fieldText = "", string value = "", int p = 1)
        {
            //if (string.IsNullOrEmpty(q)) q = value;
            var resp = await Services.FinanceGetAsync<PageModel<SLMAS>>($"/api/CustomerMaster/GetByPage?search={q}&page={p}&pageSize={limit}");
            var data = resp?.Data?.value;

            return Json(new { data = data?.results ?? new List<SLMAS>(), count = data?.rowCount ?? 0 }, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> GetVehicleType(string q = "", int limit = 0, string fieldValue = "", string fieldText = "", string value = "", int p = 1)
        {
            if (string.IsNullOrEmpty(q)) q = value;
            var resp = await Services.FinanceGetAsync<PageModel<VTYPE>>($"/api/MasterVehicleType/GetByPage?page={p}&pageSize={limit}");
            var data = resp?.Data?.value;

            return Json(new { data = data?.results ?? new List<VTYPE>(), count = data?.rowCount ?? 0 }, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> GetVehicleNo(string q = "", int limit = 0, string fieldValue = "", string fieldText = "", string value = "", int p = 1)
        {
            if (string.IsNullOrEmpty(q)) q = value;
            var resp = await Services.FinanceGetAsync<PageModel<VEHMAS>>($"/api/MasterVehicle/GetByPage?page={p}&pageSize={limit}");
            var data = resp?.Data?.value;

            return Json(new { data = data?.results ?? new List<VEHMAS>(), count = data?.rowCount ?? 0 }, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> GetTransCode(string q = "", int limit = 0, string fieldValue = "", string fieldText = "", string value = "", int p = 1)
        {
            if (string.IsNullOrEmpty(q)) q = value;
            var resp = await Services.BoughtleafGetAsync<PageModel<M_Transp>>($"/api/Bl_M_Transp/GetByPage?page={p}&pageSize={limit}");
            var data = resp?.Data?.value;

            return Json(new { data = data?.results ?? new List<M_Transp>(), count = data?.rowCount ?? 0 }, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> GetBroker(string q = "", int limit = 0, string fieldValue = "", string fieldText = "", string value = "", int p = 1)
        {
            if (string.IsNullOrEmpty(q)) q = value;
            var resp = await Services.BoughtleafGetAsync<PageModel<M_Broker>>($"/api/Bl_M_Broker/GetByPage?search={q}&page={p}&pageSize={limit}");
            var data = resp?.Data?.value;

            return Json(new { data = data?.results ?? new List<M_Broker>(), count = data?.rowCount ?? 0 }, JsonRequestBehavior.AllowGet);
        }

        // APIs call for the Inseart&Update List 
        public async Task<JsonResult> GetPacketTeaInvoiceDetails(string unit, string bldt, string blno)
        {
            try
            {
                string formattedDate = bldt.Replace("/", "-");
                var docyear = Utility.SessionHelper.GetUser().DocYear;
                if (!string.IsNullOrWhiteSpace(bldt))
                {
                    DateTime date;
                    if (DateTime.TryParse(bldt, out date))
                    {
                        formattedDate = date.ToString("yyyy-MM-dd");
                    }
                }
                var detResponse = await Services.FinanceGetAsync<List<UNBL>>($"/api/Invoice/GetByDocno?unit={unit}&bldt={formattedDate}&blno={blno}&docyear={docyear}");
                var list = detResponse?.Data ?? new List<UNBL>();

                return Json(list, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetDetailJson Error: {ex.Message}");
                return Json(new List<UNBL>(), JsonRequestBehavior.AllowGet);
            }
        }
        public async Task<ActionResult> LoadPacketTeaInvoiceDetails(string unit, string bldt, string blno, string sortOrder, string currentFilter, string searchString, int? page = 1, int pageSize = 25)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";
            ViewBag.CurrentFilter = searchString;
            ViewBag.PageSize = pageSize;

            int pageNumber = page ?? 1;
            string formattedDate = bldt;

            if (DateTime.TryParse(bldt, out DateTime dt))
            {
                formattedDate = dt.ToString("yyyy-MM-dd");
            }

            var docyear = Utility.SessionHelper.GetUser().DocYear;
            var response = await Services.FinanceGetAsync<List<UNBL>>($"/api/Invoice/GetByDocno?unit={unit}&bldt={formattedDate}&blno={blno}&docyear={docyear}");
            var list = response.Data ?? new List<UNBL>();
            int totalCount = list.Count;
            int actualPageSize = pageSize > 0 ? pageSize : Math.Max(totalCount, 1);

            var pagedList = new StaticPagedList<UNBL>(list, pageNumber, actualPageSize, totalCount);
            return PartialView("_PT_Detail_Table", pagedList);
        }
        public async Task<JsonResult> GetItems(string q = "", int limit = 0, string value = "", int p = 1)
        {
            if (string.IsNullOrEmpty(q)) q = value;
            var resp = await Services.FinanceGetAsync<PageModel<ITMAS>>($"/api/ProductMaster/GetByPage?search={Uri.EscapeDataString(q)}&page={p}&pageSize={limit}");

            var data = resp?.Data?.value;

            return Json(new { data = data?.results ?? new List<ITMAS>(), count = data?.rowCount ?? 0 }, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetGst(string q = "", int limit = 0, string value = "", int p = 1)
        {
            //if (string.IsNullOrEmpty(q)) q = value;
            var resp = await Services.FinanceGetAsync<PageModel<M_GSTMAS>>($"/api/MasterGST/GetByPage?page={p}&pageSize={limit}");
            var data = resp?.Data?.value;

            return Json(new { data = data?.results ?? new List<M_GSTMAS>(), count = data?.rowCount ?? 0 }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddRow(int rowIndex)
        {
            ViewBag.RowIndex = rowIndex;
            var model = new List<UNBL> { new UNBL() };
            return PartialView("_PT_Detail_Table", model);
        }

        [HttpPost]
        public async Task<ActionResult> SavePacket(UNBLDATA model)
        {
            try
            {
                string fy = Session["SelectedfinancialYear"].ToString();
                var dates = fy.Split('-');

                string startYear = DateTime.ParseExact(dates[0].Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
                string finalYear = DateTime.ParseExact(dates[1].Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
                foreach(var i in model.unblplt)
                {
                    i.PLCRCD = "01";
                    i.BLNO = string.Empty;
                }
                var json = JsonSerializer.Serialize(model);
                var response = await Services.FinancePostAsync<UNBLDATA>($"/api/Invoice/SaveOrUpdateAll?Stdt1={startYear}&Stdt2={finalYear}", model);
                
                if (response.IsSuccessStatusCode)
                {
                    return Json(new
                    {
                        success = true,
                        message = $"{response.Data.unbL_HED.blno} Packet Tea Invoice Saved Successfully."
                    });
                }

                string msg = response.Message;

                if (!string.IsNullOrEmpty(msg) && msg.Contains("ORA-20107"))
                {
                    msg = "Insufficient Stock.";
                }

                return Json(new
                {
                    success = false,
                    message = msg
                });
            }
            catch (Exception ex)
            {
                string msg = ex.Message;

                if (msg.Contains("ORA-20107"))
                {
                    msg = "Insufficient Stock.";
                }

                return Json(new
                {
                    success = false,
                    message = msg
                });
            }
        }
        [HttpGet]
        public async Task<ActionResult> GetFreeDetails(string q="", int p = 1, int limit = 25)
        {
            var resp = await Services.FinanceGetAsync<PageModel<FreeDetail>> ( $"/api/Invoice/GetPlastByPage?page={p}&pageSize={limit}");
            var results = resp?.Data?.value?.results ?? new List<FreeDetail>();

            return Json(new{ data = results,  count = results.Count }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public async Task<ActionResult> GetStockDetail(string itc, string unit, string mfgdate)
        {
            var resp = await Services.FinanceGetAsync<List<ItemStock>>($"/api/Invoice/GetStockDetail?unit={unit}&itc={itc}&mfgdate={mfgdate:yyyy-MM-dd}");
            return Json(resp.Data, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public async Task<ActionResult> GetStockMfgDate(string itc, string unit)
        {
            var resp = await Services.FinanceGetAsync<List<DateTime>>($"/api/Invoice/GetStockMfgDate?unit={unit}&itc={itc}");
            var data = resp.Data.Select(x => new {mfgdt = x.ToString("dd-MM-yyyy")});
            return Json(data, JsonRequestBehavior.AllowGet);
        }
       
    }
}






