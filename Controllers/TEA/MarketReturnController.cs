using Newtonsoft.Json;
using PacketTea.Models;
using PacketTea.Models.PT;
using PacketTea.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using static PacketTea.Helpers;

namespace Finance.Controllers.TEA
{
    // "TB Sales - Market Return" -- ported from VB6 mreturn.frm. Modeled on
    // MasterBlendEntryController's head+detail shape (List/New/Save/Delete +
    // "+"-expand row detail), but every data call goes through
    // Services.SalesGetAsync/SalesPostAsync instead of the plain Get/PostAsync
    // -- this module's data lives in the Sales schema
    // (CLASSIC_CONTROL.SCHEMA_SALES), same as ProductionEntryController.
    public class MarketReturnController : Controller
    {
        private string CurrentLoca => SessionHelper.GetUser()?.Loca ?? "";

        private async Task<Dictionary<string, string>> GetAllowedTypesAsync()
        {
            var response = await Services.SalesGetAsync<Dictionary<string, string>>("/api/MarketReturn/GetAllowedTypes");
            return (response.IsSuccessStatusCode ? response.Data : null) ?? new Dictionary<string, string> { { "MR", "Market Return" } };
        }

        // Backs the list page's "New" inline row Unit picker.
        private async Task<List<UnitOption>> GetUnitsForUserAsync()
        {
            var response = await Services.SalesGetAsync<List<UnitOption>>("/api/MarketReturn/GetUnitsForUser");
            return (response.IsSuccessStatusCode ? response.Data : null) ?? new List<UnitOption>();
        }

        // GET: MarketReturn
        public async Task<ActionResult> Index(string type, string unit, string searchString, int? page = 1, int pageSize = 15, string sortBy = "", string sortDir = "")
        {
            var sdsd = (List<AEDV>)Session["User_AEDV"];
            ViewBag.Permission = sdsd?.FirstOrDefault(l => l.Controller == "PacketTeaPurchaseEntry");
            ViewBag.CurrentFilter = searchString;
            ViewBag.SortBy = sortBy;
            ViewBag.SortDir = sortDir;
            ViewBag.PageSize = pageSize;
            ViewBag.Page = page ?? 1;
            ViewBag.Type = type;
            ViewBag.Unit = unit;
            ViewBag.Types = await GetAllowedTypesAsync();
            ViewBag.UnitList = await GetUnitsForUserAsync();

            var response = await Services.SalesGetAsync<PageModel<dynamic>>(
                $"/api/MarketReturn/GetByPage?type={type}&unit={unit}&search={searchString}&page={page}&pageSize={pageSize}&sortBy={sortBy}&sortDir={sortDir}");

            var json = response?.Data != null ? JsonConvert.SerializeObject(response.Data) : null;
            var wrapper = json != null ? JsonConvert.DeserializeObject<GetByPageResult>(json) : null;
            var list = wrapper?.value?.results ?? new List<MretuDocRow>();
            ViewBag.RowCount = wrapper?.value?.rowCount ?? 0;

            if (!response?.IsSuccessStatusCode ?? false)
            {
                TempData["toastrError"] = !string.IsNullOrEmpty(response?.Message)
                    ? response.Message
                    : $"Unable to load Market Return list (API returned {response?.StatusCode}).";
            }

            return View(list);
        }

        private class GetByPageResult { public GetByPageValue value { get; set; } }
        private class GetByPageValue { public List<MretuDocRow> results { get; set; } public int rowCount { get; set; } }

        // GET: MarketReturn/InsertOrUpdate
        public async Task<ActionResult> InsertOrUpdate(string docno = "", string type = "", string unit = "", bool view = false)
        {
            var allowedTypes = await GetAllowedTypesAsync();
            ViewBag.Types = allowedTypes;
            ViewBag.LockedFromList = string.IsNullOrEmpty(docno) && !string.IsNullOrEmpty(unit);
            ViewBag.IsView = view;

            if (string.IsNullOrEmpty(docno))
            {
                var effectiveType = !string.IsNullOrEmpty(type) ? type : allowedTypes.Keys.FirstOrDefault() ?? "MR";
                var model = new T_MRETU_DATA
                {
                    T_MRETU_HED = new T_MRETU_HED { UNIT = unit, LOCA = CurrentLoca, DATE_ORA = DateTime.Today },
                    T_MRETU_DET = new List<T_MRETU>()
                };
                ViewBag.IsEdit = false;
                ViewBag.NewUnit = unit;
                ViewBag.NewType = effectiveType;
                return View(model);
            }

            var response = await Services.SalesGetAsync<dynamic>($"/api/MarketReturn/GetByDocNo?docno={docno}&unit={unit}");
            if (!response.IsSuccessStatusCode || response.Data == null)
            {
                TempData["toastrError"] = response.Message ?? "Record not found.";
                return RedirectToAction("Index", new { type, unit });
            }

            var json = JsonConvert.SerializeObject(response.Data);
            var wrapper = JsonConvert.DeserializeObject<GetByDocNoResult>(json);

            var editModel = new T_MRETU_DATA
            {
                T_MRETU_HED = wrapper.head ?? new T_MRETU_HED(),
                T_MRETU_DET = wrapper.details ?? new List<T_MRETU>()
            };
            ViewBag.IsEdit = true;
            ViewBag.NewUnit = editModel.T_MRETU_HED.UNIT;
            ViewBag.NewType = type;
            return View(editModel);
        }

        private class GetByDocNoResult { public T_MRETU_HED head { get; set; } public List<T_MRETU> details { get; set; } }

        // POST: MarketReturn/Save
        [HttpPost]
        public async Task<JsonResult> Save(T_MRETU_DATA model)
        {
            if (model?.T_MRETU_HED != null)
                model.T_MRETU_HED.LOCA = string.IsNullOrEmpty(model.T_MRETU_HED.LOCA) ? CurrentLoca : model.T_MRETU_HED.LOCA;

            // The API's MRETU_DATA names its parts MRETU_HED/MRETU_DET (no "T_"
            // prefix) -- posting T_MRETU_DATA as-is left the header null there.
            var payload = new
            {
                MRETU_HED = model?.T_MRETU_HED,
                MRETU_DET = model?.T_MRETU_DET,
                OptFlag = model?.OptFlag,
                BillYearBack = model?.BillYearBack ?? 0
            };
            var response = await Services.SalesPostAsync<dynamic>("/api/MarketReturn/SaveOrUpdate", payload);
            return Json(new
            {
                success = response.IsSuccessStatusCode,
                message = response.IsSuccessStatusCode ? "Saved successfully." : (response.Message ?? "Save failed."),
                data = response.Data
            });
        }

        // POST: MarketReturn/Delete
        [HttpPost]
        public async Task<ActionResult> Delete(string docno, string type, string unit)
        {
            var response = await Services.SalesPostAsync<dynamic>($"/api/MarketReturn/Delete?docno={docno}&unit={unit}", new { });

            TempData[response.IsSuccessStatusCode ? "toastrSuccess" : "toastrError"] =
                response.IsSuccessStatusCode ? "Deleted successfully." : (response.Message ?? "Delete failed.");

            return RedirectToAction("Index", new { type, unit });
        }

        // GET: MarketReturn/GetRowDetail (AJAX, fired by the Index list's "+" toggle)
        [HttpGet]
        public async Task<ActionResult> GetRowDetail(string docno = "", string unit = "")
        {
            var r = await Services.SalesGetAsync<dynamic>($"/api/MarketReturn/GetRowDetail?docno={docno}&unit={unit}");
            if (!r.IsSuccessStatusCode || r.Data == null)
                return JsonExact(new { success = false, message = r.Message ?? "Record not found." });

            var obj = (Newtonsoft.Json.Linq.JObject)r.Data;
            return JsonExact(obj);
        }

        // =====================================================================
        // AJAX lookup passthroughs (same JsonExact/JsonExactOrSessionExpired
        // pattern as ProductionEntryController -- see its own notes).
        // =====================================================================
        private ActionResult JsonExact(object data) =>
            Content(JsonConvert.SerializeObject(data), "application/json");

        private ActionResult JsonExactOrSessionExpired<T>(ResponseApiModel<T> r)
        {
            if (!r.IsSuccessStatusCode)
            {
                bool isAuthFailure = string.Equals(r.StatusCode, System.Net.HttpStatusCode.Unauthorized.ToString(), StringComparison.OrdinalIgnoreCase);
                if (isAuthFailure)
                {
                    Response.StatusCode = 440;
                    return JsonExact(new { sessionExpired = true, message = "Your session has expired. Please log in again." });
                }
                Response.StatusCode = 500;
                return JsonExact(new
                {
                    sessionExpired = false,
                    message = !string.IsNullOrEmpty(r.Message) ? r.Message : $"Unable to load data (API returned {r.StatusCode})."
                });
            }
            return JsonExact(r.Data);
        }

        [HttpGet]
        public async Task<ActionResult> GetParty(string search = "")
        {
            var r = await Services.SalesGetAsync<dynamic>($"/api/MarketReturn/GetParty?search={search}&pageSize=50");
            return JsonExactOrSessionExpired(r);
        }

        [HttpGet]
        public async Task<ActionResult> GetTransporter(string search = "")
        {
            var r = await Services.SalesGetAsync<dynamic>($"/api/MarketReturn/GetTransporter?search={search}&pageSize=50");
            return JsonExactOrSessionExpired(r);
        }

        [HttpGet]
        public async Task<ActionResult> GetItemMaster(string search = "")
        {
            var r = await Services.SalesGetAsync<dynamic>($"/api/MarketReturn/GetItemMaster?search={search}&pageSize=50");
            return JsonExactOrSessionExpired(r);
        }

        // Bill No help (VB6 TEXT3_buttonclick) -- the chosen party's bills for this
        // unit + the signed-in LOCA, in the current / last / before-last year.
        [HttpGet]
        public async Task<ActionResult> GetBillList(string pcd = "", string unit = "", int yearBack = 0, string search = "")
        {
            var r = await Services.SalesGetAsync<dynamic>(
                $"/api/MarketReturn/GetBillList?pcd={Uri.EscapeDataString(pcd ?? "")}&unit={Uri.EscapeDataString(unit ?? "")}" +
                $"&loca={Uri.EscapeDataString(CurrentLoca)}&yearBack={yearBack}&search={Uri.EscapeDataString(search ?? "")}");
            return JsonExactOrSessionExpired(r);
        }

        [HttpGet]
        public async Task<ActionResult> GetOriginalBill(string blno = "", string unit = "", string rt = "", int yearBack = 0,
                                                        string excludeDocno = "", string excludeUnit = "")
        {
            var r = await Services.SalesGetAsync<dynamic>(
                $"/api/MarketReturn/GetOriginalBill?blno={Uri.EscapeDataString(blno ?? "")}&unit={Uri.EscapeDataString(unit ?? "")}" +
                $"&rt={Uri.EscapeDataString(rt ?? "")}&yearBack={yearBack}" +
                $"&excludeDocno={Uri.EscapeDataString(excludeDocno ?? "")}&excludeUnit={Uri.EscapeDataString(excludeUnit ?? "")}");
            return JsonExactOrSessionExpired(r);
        }
    }

    public class MretuDocRow
    {
        public string docno { get; set; }
        public DateTime? docdt { get; set; }
        public string blno { get; set; }
        public DateTime? bldt { get; set; }
        public string pcd { get; set; }
        public string partyName { get; set; }
        public string veh { get; set; }
        public string unit { get; set; }
        public string irn { get; set; }
        public decimal? totQty { get; set; }
        public decimal? totAmt { get; set; }
    }
}
