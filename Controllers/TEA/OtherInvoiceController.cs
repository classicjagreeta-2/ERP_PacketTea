using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    // "TB Sales - Other Invoice" -- ported from VB6 trn_scrap_inv.frm. One screen,
    // three menus (VB6 pbill_tag): Other Invoice (Index, tran 1), Other Invoice
    // Debit Note (DebitNote, tran 2), Other Invoice Credit Note (CreditNote, tran 3).
    // List and entry screen follow Master Blend Entry: the list's "New" row picks
    // Unit + Type (the document type), which are then locked on the entry screen.
    // Every data call goes through Services.SalesGetAsync/SalesPostAsync -- the
    // data lives in the Sales schema (CLASSIC_CONTROL.SCHEMA_SALES, e.g. FIN_JSTIL2027).
    public class OtherInvoiceController : Controller
    {
        private static readonly Dictionary<string, string> TranNames = new Dictionary<string, string>
        {
            { "1", "Other Invoice" },
            { "2", "Other Invoice Debit Note" },
            { "3", "Other Invoice Credit Note" },
        };
        private static readonly Dictionary<string, string> TranActions = new Dictionary<string, string>
        {
            { "1", "Index" },
            { "2", "DebitNote" },
            { "3", "CreditNote" },
        };

        private static string TranOf(string tran) => tran == "2" || tran == "3" ? tran : "1";
        private static string E(string s) => Uri.EscapeDataString(s ?? "");

        private async Task<Dictionary<string, string>> GetAllowedTypesAsync(string tran)
        {
            var response = await Services.SalesGetAsync<Dictionary<string, string>>($"/api/OtherInvoice/GetAllowedTypes?tran={E(tran)}");
            return (response.IsSuccessStatusCode ? response.Data : null) ?? new Dictionary<string, string>();
        }

        private async Task<List<UnitOption>> GetUnitsForUserAsync()
        {
            var response = await Services.SalesGetAsync<List<UnitOption>>("/api/OtherInvoice/GetUnitsForUser");
            return (response.IsSuccessStatusCode ? response.Data : null) ?? new List<UnitOption>();
        }

        // GET: OtherInvoice (menu "Other Invoice")
        public Task<ActionResult> Index(string unit, string searchString, int? page = 1, int pageSize = 15, string sortBy = "", string sortDir = "")
            => List("1", unit, searchString, page, pageSize, sortBy, sortDir);

        // GET: OtherInvoice/DebitNote (menu "Other Invoice Debit Note")
        public Task<ActionResult> DebitNote(string unit, string searchString, int? page = 1, int pageSize = 15, string sortBy = "", string sortDir = "")
            => List("2", unit, searchString, page, pageSize, sortBy, sortDir);

        // GET: OtherInvoice/CreditNote (menu "Other Invoice Credit Note")
        public Task<ActionResult> CreditNote(string unit, string searchString, int? page = 1, int pageSize = 15, string sortBy = "", string sortDir = "")
            => List("3", unit, searchString, page, pageSize, sortBy, sortDir);

        private async Task<ActionResult> List(string tran, string unit, string searchString, int? page, int pageSize, string sortBy, string sortDir)
        {
            ViewBag.Tran = tran;
            ViewBag.TranName = TranNames[tran];
            ViewBag.ListAction = TranActions[tran];
            ViewBag.CurrentFilter = searchString;
            ViewBag.SortBy = sortBy;
            ViewBag.SortDir = sortDir;
            ViewBag.PageSize = pageSize;
            ViewBag.Page = page ?? 1;
            ViewBag.Unit = unit;
            ViewBag.DocTypes = await GetAllowedTypesAsync(tran);
            ViewBag.UnitList = await GetUnitsForUserAsync();

            var response = await Services.SalesGetAsync<JObject>(
                $"/api/OtherInvoice/GetByPage?tran={E(tran)}&unit={E(unit)}&search={E(searchString)}&page={page ?? 1}&pageSize={pageSize}&sortBy={E(sortBy)}&sortDir={E(sortDir)}");

            var value = response?.Data?["value"];
            var list = value?["results"]?.ToObject<List<InvDocRow>>() ?? new List<InvDocRow>();
            ViewBag.RowCount = value?["rowCount"]?.Value<int>() ?? 0;

            if (!(response?.IsSuccessStatusCode ?? false))
            {
                TempData["toastrError"] = !string.IsNullOrEmpty(response?.Message)
                    ? response.Message
                    : $"Unable to load the {TranNames[tran]} list (API returned {response?.StatusCode}).";
            }

            return View("Index", list);
        }

        // GET: OtherInvoice/InsertOrUpdate
        // New: `unit` + `doctype` come from the list's "New" row and are locked here.
        // Edit/View: the document's full key (docYear, unit, doctype, docno).
        public async Task<ActionResult> InsertOrUpdate(string tran = "1", string docYear = "", string unit = "", string doctype = "", string docno = "", bool view = false)
        {
            tran = TranOf(tran);
            ViewBag.Tran = tran;
            ViewBag.TranName = TranNames[tran];
            ViewBag.ListAction = TranActions[tran];
            ViewBag.IsView = view;
            var docTypes = await GetAllowedTypesAsync(tran);
            ViewBag.DocTypes = docTypes;

            if (string.IsNullOrEmpty(docno))
            {
                if (string.IsNullOrEmpty(unit) || string.IsNullOrEmpty(doctype))
                {
                    TempData["toastrError"] = "Select Unit and Type from the New row first.";
                    return RedirectToAction(TranActions[tran]);
                }
                var units = await GetUnitsForUserAsync();
                var u = units.FirstOrDefault(x => string.Equals(x.CODE, unit, StringComparison.OrdinalIgnoreCase));
                ViewBag.IsEdit = false;
                return View(new T_INV_DATA
                {
                    TRAN = tran,
                    TRN_INV_HEAD = new T_INV_HEAD
                    {
                        TRAN = tran,
                        UNIT = unit,
                        DOCTYPE = doctype,
                        DOCDT = DateTime.Today,
                        RATE_TYPE = "N",
                        UnitStateCode = u?.STATE
                    }
                });
            }

            var response = await Services.SalesGetAsync<JObject>(
                $"/api/OtherInvoice/GetByDocNo?docYear={E(docYear)}&unit={E(unit)}&doctype={E(doctype)}&docno={E(docno)}");
            if (!response.IsSuccessStatusCode || response.Data == null)
            {
                TempData["toastrError"] = response.Message ?? "Record not found.";
                return RedirectToAction(TranActions[tran]);
            }

            var head = response.Data["head"]?.ToObject<T_INV_HEAD>() ?? new T_INV_HEAD();
            var details = response.Data["details"]?.ToObject<List<T_INV_DETAIL>>() ?? new List<T_INV_DETAIL>();
            ViewBag.IsEdit = true;
            return View(new T_INV_DATA { TRAN = tran, TRN_INV_HEAD = head, TRN_INV_DETAIL = details });
        }

        // POST: OtherInvoice/Preview -- VB6 Save step 1: validate + build the accounting voucher, no writes.
        [HttpPost]
        public async Task<ActionResult> Preview(T_INV_DATA model)
        {
            var r = await Services.SalesPostAsync<JObject>("/api/OtherInvoice/Preview", model);
            return JsonExact(new { success = r.IsSuccessStatusCode, message = r.IsSuccessStatusCode ? null : (r.Message ?? "Validation failed."), data = r.Data });
        }

        // POST: OtherInvoice/Save -- VB6 Save step 2: the actual commit.
        [HttpPost]
        public async Task<ActionResult> Save(T_INV_DATA model)
        {
            var r = await Services.SalesPostAsync<JObject>("/api/OtherInvoice/SaveOrUpdate", model);
            return JsonExact(new { success = r.IsSuccessStatusCode, message = r.IsSuccessStatusCode ? "Saved successfully." : (r.Message ?? "Save failed."), data = r.Data });
        }

        // POST: OtherInvoice/Delete
        [HttpPost]
        public async Task<ActionResult> Delete(string tran, string docYear, string unit, string doctype, string docno)
        {
            tran = TranOf(tran);
            var r = await Services.SalesPostAsync<JObject>(
                $"/api/OtherInvoice/Delete?docYear={E(docYear)}&unit={E(unit)}&doctype={E(doctype)}&docno={E(docno)}", new { });

            TempData[r.IsSuccessStatusCode ? "toastrSuccess" : "toastrError"] =
                r.IsSuccessStatusCode ? $"{docno} deleted." : (r.Message ?? "Delete failed.");
            return RedirectToAction(TranActions[tran]);
        }

        // GET: OtherInvoice/GetRowDetail (AJAX, the list's "+" toggle)
        [HttpGet]
        public async Task<ActionResult> GetRowDetail(string docYear = "", string unit = "", string doctype = "", string docno = "")
        {
            var r = await Services.SalesGetAsync<JObject>(
                $"/api/OtherInvoice/GetRowDetail?docYear={E(docYear)}&unit={E(unit)}&doctype={E(doctype)}&docno={E(docno)}");
            if (!r.IsSuccessStatusCode || r.Data == null)
                return JsonExact(new { success = false, message = r.Message ?? "Record not found." });
            r.Data["success"] = true;
            return JsonExact(r.Data);
        }

        // =====================================================================
        // AJAX lookup passthroughs (all searchable on every displayed column)
        // =====================================================================
        private ActionResult JsonExact(object data) =>
            Content(JsonConvert.SerializeObject(data), "application/json");

        // 440 = session expired (prompts re-login), 500 = any other API failure.
        private ActionResult JsonExactOrSessionExpired<T>(ResponseApiModel<T> r)
        {
            if (!r.IsSuccessStatusCode)
            {
                bool isAuthFailure = string.Equals(r.StatusCode, HttpStatusCode.Unauthorized.ToString(), StringComparison.OrdinalIgnoreCase);
                Response.StatusCode = isAuthFailure ? 440 : 500;
                return JsonExact(new
                {
                    sessionExpired = isAuthFailure,
                    message = isAuthFailure ? "Your session has expired. Please log in again."
                            : !string.IsNullOrEmpty(r.Message) ? r.Message : $"Unable to load data (API returned {r.StatusCode})."
                });
            }
            return JsonExact(r.Data);
        }

        private async Task<ActionResult> Lookup(string path) =>
            JsonExactOrSessionExpired(await Services.SalesGetAsync<JToken>("/api/OtherInvoice/" + path));

        [HttpGet] public Task<ActionResult> GetParty(string search = "", string unit = "", string code = "") =>
            Lookup($"GetParty?search={E(search)}&unit={E(unit)}&code={E(code)}");
        [HttpGet] public Task<ActionResult> GetTransporter(string search = "", string unit = "", string code = "") =>
            Lookup($"GetTransporter?search={E(search)}&unit={E(unit)}&code={E(code)}");
        [HttpGet] public Task<ActionResult> GetBank(string search = "", string unit = "", string code = "") =>
            Lookup($"GetBank?search={E(search)}&unit={E(unit)}&code={E(code)}");
        [HttpGet] public Task<ActionResult> GetItemMaster(string search = "", string unit = "", string code = "") =>
            Lookup($"GetItemMaster?search={E(search)}&unit={E(unit)}&code={E(code)}");
        [HttpGet] public Task<ActionResult> GetGstCode(string search = "", string unit = "", string code = "") =>
            Lookup($"GetGstCode?search={E(search)}&unit={E(unit)}&code={E(code)}");
        [HttpGet] public Task<ActionResult> GetRefCode(string search = "", string acode = "", string code = "") =>
            Lookup($"GetRefCode?search={E(search)}&acode={E(acode)}&code={E(code)}");
        [HttpGet] public Task<ActionResult> GetCostCentre(string search = "", string code = "") =>
            Lookup($"GetCostCentre?search={E(search)}&code={E(code)}");
        [HttpGet] public Task<ActionResult> GetRefInvoices(string pcd = "", string unit = "", bool lastYear = false, string search = "") =>
            Lookup($"GetRefInvoices?pcd={E(pcd)}&unit={E(unit)}&lastYear={lastYear}&search={E(search)}");
        [HttpGet] public Task<ActionResult> GetRefInvoiceLines(string refNo = "", string unit = "", bool lastYear = false) =>
            Lookup($"GetRefInvoiceLines?refNo={E(refNo)}&unit={E(unit)}&lastYear={lastYear}");
    }

    public class InvDocRow
    {
        public string docYear { get; set; }
        public string unit { get; set; }
        public string doctype { get; set; }
        public string docno { get; set; }
        public DateTime? docdt { get; set; }
        public string pcd { get; set; }
        public string partyName { get; set; }
        public string contractNo { get; set; }
        public DateTime? contractDate { get; set; }
        public string vehNo { get; set; }
        public string bankCode { get; set; }
        public decimal? taxableAmt { get; set; }
        public decimal? gstAmt { get; set; }
        public decimal? tcsAmt { get; set; }
        public decimal? roundOff { get; set; }
        public decimal? totalAmt { get; set; }
        public string irn { get; set; }
    }
}
