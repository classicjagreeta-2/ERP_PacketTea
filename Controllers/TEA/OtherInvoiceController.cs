using Newtonsoft.Json;
using PacketTea.Models;
using PacketTea.Models.PT;
using PacketTea.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using static PacketTea.Helpers;

namespace Finance.Controllers.TEA
{
    // "TB Sales - Other Invoice" -- ported from VB6 trn_scrap_inv.frm. Modeled on
    // MasterBlendEntryController's shape (real Type dropdown flowing through
    // List -> New row -> locked entry screen, same as Blend Type), but every
    // data call goes through Services.SalesGetAsync/SalesPostAsync instead of
    // the plain Get/PostAsync -- this module's data lives in the Sales schema
    // (CLASSIC_CONTROL.SCHEMA_SALES), not the unit's normal operating schema.
    // Route params carry `doctype` everywhere MasterBlendEntryController carries
    // `blendType`.
    public class OtherInvoiceController : Controller
    {
        private string CurrentLoca => SessionHelper.GetUser()?.Loca ?? "";

        // Static fallback -- kept in sync with ClassicERPCoreAPI's OtherInvoiceController.DocTypes.
        // GetAllowedTypesAsync always prefers the API's own list; this only covers the API
        // being briefly unreachable.
        private static readonly Dictionary<string, string> FallbackDocTypes = new Dictionary<string, string>
        {
            { "1", "Invoice" },
            { "2", "Debit Note" },
            { "3", "Credit Note" },
        };

        private async Task<Dictionary<string, string>> GetAllowedTypesAsync()
        {
            var response = await Services.SalesGetAsync<Dictionary<string, string>>("/api/OtherInvoice/GetAllowedTypes");
            return (response.IsSuccessStatusCode ? response.Data : null) ?? FallbackDocTypes;
        }

        // Backs the list page's "New" inline row Unit picker.
        private async Task<List<UnitOption>> GetUnitsForUserAsync()
        {
            var response = await Services.SalesGetAsync<List<UnitOption>>("/api/OtherInvoice/GetUnitsForUser");
            return (response.IsSuccessStatusCode ? response.Data : null) ?? new List<UnitOption>();
        }

        // GET: OtherInvoice
        public async Task<ActionResult> Index(string doctype, string unit, string searchString, int? page = 1, int pageSize = 15, string sortBy = "", string sortDir = "")
        {
            var sdsd = (List<AEDV>)Session["User_AEDV"];
            ViewBag.Permission = sdsd?.FirstOrDefault(l => l.Controller == "PacketTeaPurchaseEntry");
            ViewBag.CurrentFilter = searchString;
            ViewBag.SortBy = sortBy;
            ViewBag.SortDir = sortDir;
            ViewBag.PageSize = pageSize;
            ViewBag.Page = page ?? 1;
            ViewBag.DocType = doctype;
            ViewBag.Unit = unit;
            ViewBag.DocTypes = await GetAllowedTypesAsync();
            ViewBag.UnitList = await GetUnitsForUserAsync();

            var response = await Services.SalesGetAsync<PageModel<dynamic>>(
                $"/api/OtherInvoice/GetByPage?doctype={doctype}&unit={unit}&search={searchString}&page={page}&pageSize={pageSize}&sortBy={sortBy}&sortDir={sortDir}");

            var json = response?.Data != null ? JsonConvert.SerializeObject(response.Data) : null;
            var wrapper = json != null ? JsonConvert.DeserializeObject<GetByPageResult>(json) : null;
            var list = wrapper?.value?.results ?? new List<InvDocRow>();
            ViewBag.RowCount = wrapper?.value?.rowCount ?? 0;

            if (!response?.IsSuccessStatusCode ?? false)
            {
                TempData["toastrError"] = !string.IsNullOrEmpty(response?.Message)
                    ? response.Message
                    : $"Unable to load Other Invoice list (API returned {response?.StatusCode}).";
            }

            return View(list);
        }

        private class GetByPageResult { public GetByPageValue value { get; set; } }
        private class GetByPageValue { public List<InvDocRow> results { get; set; } public int rowCount { get; set; } }

        // GET: OtherInvoice/InsertOrUpdate
        // `unit`/`doctype` are only populated when opened from the list page's "New" inline
        // row -- both repeat here read-only/locked instead of Doc Type staying editable and
        // Unit staying invisible, same as Blend Type locks in Master Blend Entry.
        public async Task<ActionResult> InsertOrUpdate(string docno = "", string docdt = "", string doctype = "", string unit = "", bool view = false)
        {
            var allowedTypes = await GetAllowedTypesAsync();
            ViewBag.DocTypes = allowedTypes;
            ViewBag.LockedFromList = string.IsNullOrEmpty(docno) && !string.IsNullOrEmpty(unit) && !string.IsNullOrEmpty(doctype);
            ViewBag.IsView = view;

            if (string.IsNullOrEmpty(docno))
            {
                var effectiveType = !string.IsNullOrEmpty(doctype) ? doctype : allowedTypes.Keys.FirstOrDefault() ?? "1";
                var model = new T_INV_DATA
                {
                    TRN_INV_HEAD = new T_INV_HEAD
                    {
                        LOCA = CurrentLoca,
                        UNIT = unit,
                        DOCTYPE = effectiveType,
                        DOCDT = DateTime.Today,
                        RATE_TYPE = "E"
                    },
                    TRN_INV_DETAIL = new List<T_INV_DETAIL>()
                };
                ViewBag.IsEdit = false;
                return View(model);
            }

            var response = await Services.SalesGetAsync<dynamic>(
                $"/api/OtherInvoice/GetByDocNo?doctype={doctype}&docno={docno}&docdt={docdt}&unit={unit}");
            if (!response.IsSuccessStatusCode || response.Data == null)
            {
                TempData["toastrError"] = response.Message ?? "Record not found.";
                return RedirectToAction("Index", new { doctype, unit });
            }

            var json = JsonConvert.SerializeObject(response.Data);
            var wrapper = JsonConvert.DeserializeObject<GetByDocNoResult>(json);

            var editModel = new T_INV_DATA
            {
                TRN_INV_HEAD = wrapper.head ?? new T_INV_HEAD(),
                TRN_INV_DETAIL = wrapper.details ?? new List<T_INV_DETAIL>()
            };
            ViewBag.IsEdit = true;
            return View(editModel);
        }

        private class GetByDocNoResult
        {
            public T_INV_HEAD head { get; set; }
            public List<T_INV_DETAIL> details { get; set; }
        }

        // POST: OtherInvoice/Save (AJAX, body = T_INV_DATA as JSON)
        [HttpPost]
        public async Task<JsonResult> Save(T_INV_DATA model)
        {
            if (model?.TRN_INV_HEAD != null)
                model.TRN_INV_HEAD.LOCA = string.IsNullOrEmpty(model.TRN_INV_HEAD.LOCA) ? CurrentLoca : model.TRN_INV_HEAD.LOCA;

            var response = await Services.SalesPostAsync<dynamic>("/api/OtherInvoice/SaveOrUpdate", model);
            return Json(new
            {
                success = response.IsSuccessStatusCode,
                message = response.IsSuccessStatusCode ? "Saved successfully." : (response.Message ?? "Save failed."),
                data = response.Data
            });
        }

        // POST: OtherInvoice/Delete
        [HttpPost]
        public async Task<ActionResult> Delete(string docno, string doctype, string unit)
        {
            var response = await Services.SalesPostAsync<dynamic>(
                $"/api/OtherInvoice/Delete?doctype={doctype}&docno={docno}&unit={unit}", new { });

            TempData[response.IsSuccessStatusCode ? "toastrSuccess" : "toastrError"] =
                response.IsSuccessStatusCode ? "Deleted successfully." : (response.Message ?? "Delete failed.");

            return RedirectToAction("Index", new { doctype });
        }

        // GET: OtherInvoice/GetRowDetail (AJAX, fired by the Index list's "+" toggle).
        // Unlike Master Blend Entry, the API's own GetRowDetail already includes the
        // item lines, so this is a single passthrough call, not two merged ones.
        [HttpGet]
        public async Task<ActionResult> GetRowDetail(string docno = "", string doctype = "", string unit = "")
        {
            var r = await Services.SalesGetAsync<dynamic>(
                $"/api/OtherInvoice/GetRowDetail?doctype={doctype}&docno={docno}&unit={unit}");
            if (!r.IsSuccessStatusCode || r.Data == null)
                return JsonExact(new { success = false, message = r.Message ?? "Record not found." });

            var obj = (Newtonsoft.Json.Linq.JObject)r.Data;
            obj["success"] = true;
            return JsonExact(obj);
        }

        // =====================================================================
        // AJAX lookup passthroughs
        // =====================================================================
        private ActionResult JsonExact(object data) =>
            Content(JsonConvert.SerializeObject(data), "application/json");

        // Refined 401-vs-other-failure version (see ProductionEntryController) --
        // distinguishes an expired session (440, prompts re-login) from any other
        // API failure (500, generic error) instead of collapsing both to 440.
        private ActionResult JsonExactOrSessionExpired<T>(ResponseApiModel<T> r)
        {
            if (!r.IsSuccessStatusCode)
            {
                bool isAuthFailure = string.Equals(r.StatusCode, HttpStatusCode.Unauthorized.ToString(), StringComparison.OrdinalIgnoreCase);
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
            var r = await Services.SalesGetAsync<dynamic>($"/api/OtherInvoice/GetParty?search={search}&pageSize=50");
            return JsonExactOrSessionExpired(r);
        }

        [HttpGet]
        public async Task<ActionResult> GetItemMaster(string search = "")
        {
            var r = await Services.SalesGetAsync<dynamic>($"/api/OtherInvoice/GetItemMaster?search={search}&pageSize=50");
            return JsonExactOrSessionExpired(r);
        }

        [HttpGet]
        public async Task<ActionResult> GetBank(string search = "")
        {
            var r = await Services.SalesGetAsync<dynamic>($"/api/OtherInvoice/GetBank?search={search}&pageSize=50");
            return JsonExactOrSessionExpired(r);
        }
    }

    public class InvDocRow
    {
        public string doctype { get; set; }
        public string docno { get; set; }
        public DateTime? docdt { get; set; }
        public string pcd { get; set; }
        public string partyName { get; set; }
        public string unit { get; set; }
        public string bankCode { get; set; }
        public decimal? grossAmt { get; set; }
        public decimal? roundOff { get; set; }
        public decimal? totalAmt { get; set; }
        public string irn { get; set; }
    }
}
