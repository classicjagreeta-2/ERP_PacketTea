using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PacketTea;
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

        // AEDV Add/Edit/Delete/View + back-date rights (CLAUDE.md "AEDV" section): the screen's own
        // "OtherInvoice" row of Session["User_AEDV"], falling back to / combined with the shared
        // "PacketTeaPurchaseEntry" bucket like the other TEA screens (see AEDV.ForScreen).
        // One controller serves all three menus, so they share one permission.
        private const string Screen = "OtherInvoice";
        private AEDV Permission => AEDV.ForScreen((List<AEDV>)Session["User_AEDV"], Screen);

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
            ViewBag.Permission = Permission;
            ViewBag.Tran = tran;
            ViewBag.TranName = TranNames[tran];
            ViewBag.ListAction = TranActions[tran];
            ViewBag.CurrentFilter = searchString;
            ViewBag.SortBy = sortBy;
            ViewBag.SortDir = sortDir;
            ViewBag.PageSize = pageSize;
            ViewBag.Unit = unit;
            // Allowed types and the user's Units are independent lookups -- run them side by side.
            var typesTask = GetAllowedTypesAsync(tran);
            var unitsTask = GetUnitsForUserAsync();
            await Task.WhenAll(typesTask, unitsTask);
            ViewBag.DocTypes = typesTask.Result;
            var userUnits = unitsTask.Result;
            ViewBag.UnitList = userUnits;
            // Only the Units the user is linked to: the one asked for (if permitted) else every one of
            // them, comma-separated (API GetByPage parses it) -- never an unfiltered blank `unit`.
            var unitFilter = UnitScope.ListFilter(unit, userUnits);

            // Chunked list: a full page view always starts at the first chunk; further chunks
            // arrive as AJAX calls (Scripts/list-infinite-scroll.js) and return just the rows.
            var isChunkRequest = Request.IsAjaxRequest();
            var pageNo = isChunkRequest ? Math.Max(page ?? 1, 1) : 1;
            ViewBag.Page = pageNo;
            ViewBag.RowOffset = (pageNo - 1) * pageSize;

            var response = await Services.SalesGetAsync<JObject>(
                $"/api/OtherInvoice/GetByPage?tran={E(tran)}&unit={E(unitFilter)}&search={E(searchString)}&page={pageNo}&pageSize={pageSize}&sortBy={E(sortBy)}&sortDir={E(sortDir)}");

            var value = response?.Data?["value"];
            var list = value?["results"]?.ToObject<List<InvDocRow>>() ?? new List<InvDocRow>();
            ViewBag.RowCount = value?["rowCount"]?.Value<int>() ?? 0;

            if (isChunkRequest)
            {
                // A failed chunk must not leave a toast queued for the next full page load; an
                // empty response tells the list's scroll loader to stop.
                return PartialView("_ListRows", (response?.IsSuccessStatusCode ?? false) ? list : new List<InvDocRow>());
            }

            if (!(response?.IsSuccessStatusCode ?? false))
            {
                TempData["toastrError"] = !string.IsNullOrEmpty(response?.Message)
                    ? response.Message
                    : $"Unable to load the {TranNames[tran]} list (API returned {response?.StatusCode}).";
            }

            return View("Index", list);
        }

        // Doc Date picker's lower bound -- the later of the financial-year start and the AEDV
        // back-date allowance (Aday for a new document, Eday while editing). Save() is the real
        // gate; this only steers the date picker (same as TeaPurchaseNoteController).
        private void SetMinDocDate(AEDV permission, bool isNew)
        {
            DateTime? fyStart = null;
            string fy = Session["SelectedfinancialYear"]?.ToString();
            if (!string.IsNullOrEmpty(fy) && fy.Contains("-"))
            {
                var parts = fy.Split('-');
                string startDigits = new string(parts[0].Where(char.IsDigit).ToArray());
                if (startDigits.Length >= 4)
                    fyStart = new DateTime(int.Parse(startDigits.Substring(startDigits.Length - 4)), 4, 1);
            }
            var backDateFloor = (permission ?? new AEDV()).MinDocDate(isNew);
            var effectiveMin = fyStart.HasValue && fyStart.Value > backDateFloor ? fyStart.Value : backDateFloor;
            ViewBag.MinDocDate = effectiveMin.ToString("yyyy-MM-dd");
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

            var permission = Permission;
            ViewBag.Permission = permission;

            bool isNew = string.IsNullOrEmpty(docno);

            // `view` is set by the list's View button -- same fetch as Edit, but the form renders
            // read-only. It only applies to an existing document and needs the View right.
            view = view && !isNew;
            if (view && !(permission?.View ?? false))
            {
                TempData["toastrError"] = "You do not have permission to view this entry.";
                return RedirectToAction(TranActions[tran]);
            }
            ViewBag.IsView = view;

            // The list hides New without the Add right, but this URL is reachable directly.
            if (isNew && !(permission?.Add ?? false))
            {
                TempData["toastrError"] = "You do not have permission to add a new entry.";
                return RedirectToAction(TranActions[tran]);
            }

            SetMinDocDate(permission, isNew);

            var docTypes = await GetAllowedTypesAsync(tran);
            ViewBag.DocTypes = docTypes;

            if (isNew)
            {
                if (string.IsNullOrEmpty(unit) || string.IsNullOrEmpty(doctype))
                {
                    TempData["toastrError"] = "Select Unit and Type from the New row first.";
                    return RedirectToAction(TranActions[tran]);
                }
                var units = await GetUnitsForUserAsync();
                // The Unit picked in the list's New row must be one the user is linked to.
                if (!UnitScope.IsAllowed(unit, units))
                {
                    TempData["toastrError"] = $"You do not have permission for Unit {unit}.";
                    return RedirectToAction(TranActions[tran]);
                }
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

            // The Unit permission lookup and the document load run side by side.
            var unitsTask = GetUnitsForUserAsync();
            var docTask = Services.SalesGetAsync<JObject>(
                $"/api/OtherInvoice/GetByDocNo?docYear={E(docYear)}&unit={E(unit)}&doctype={E(doctype)}&docno={E(docno)}");
            await Task.WhenAll(unitsTask, docTask);
            var response = docTask.Result;
            if (!response.IsSuccessStatusCode || response.Data == null)
            {
                TempData["toastrError"] = response.Message ?? "Record not found.";
                return RedirectToAction(TranActions[tran]);
            }

            var head = response.Data["head"]?.ToObject<T_INV_HEAD>() ?? new T_INV_HEAD();
            var details = response.Data["details"]?.ToObject<List<T_INV_DETAIL>>() ?? new List<T_INV_DETAIL>();

            // The document's own Unit must be one the user is linked to (a Doc No repeats across units).
            var docUnit = !string.IsNullOrEmpty(head.UNIT) ? head.UNIT : unit;
            if (!UnitScope.IsAllowed(docUnit, unitsTask.Result))
            {
                TempData["toastrError"] = $"You do not have permission for Unit {docUnit}.";
                return RedirectToAction(TranActions[tran]);
            }

            // Block opening Edit outright when the user has no Edit right, or the document's own
            // Doc Date has fallen outside the Eday back-date window, or its e-Invoice (IRN) is
            // generated -- View bypasses this (read-only regardless of the Edit / back-date policy).
            if (!view)
            {
                var editErr = AEDV.CheckAddEdit(permission, false, head.DOCDT ?? DateTime.Today);
                if (editErr == null && !string.IsNullOrWhiteSpace(head.IRN))
                    editErr = "E-Invoice has been Generated for this, can't edit.";
                if (editErr != null)
                {
                    TempData["toastrError"] = editErr;
                    return RedirectToAction(TranActions[tran]);
                }
            }

            ViewBag.IsEdit = true;
            return View(new T_INV_DATA { TRAN = tran, TRN_INV_HEAD = head, TRN_INV_DETAIL = details });
        }

        // POST: OtherInvoice/Preview -- VB6 Save step 1: validate + build the accounting voucher, no writes.
        [HttpPost]
        public async Task<ActionResult> Preview(T_INV_DATA model)
        {
            var deny = await CheckSaveAllowedAsync(model);
            if (deny != null) return JsonExact(new { success = false, message = deny });
            var r = await Services.SalesPostAsync<JObject>("/api/OtherInvoice/Preview", model);
            return JsonExact(new { success = r.IsSuccessStatusCode, message = r.IsSuccessStatusCode ? null : (r.Message ?? "Validation failed."), data = r.Data });
        }

        // Server-side enforcement shared by Preview and Save (the Doc Date picker's `min` only steers
        // well-behaved clients): AEDV Add/Edit + back-date window, then the Unit (a NEW document is
        // stamped with the one picked on the list, posted from the entry screen; an edit keeps the
        // document's own) must be one the user is linked to. Null = allowed, else the message.
        private async Task<string> CheckSaveAllowedAsync(T_INV_DATA model)
        {
            var head = model?.TRN_INV_HEAD;
            if (head == null) return "Nothing to save.";
            bool isNew = model.IsNew ?? string.IsNullOrWhiteSpace(head.DOCNO);
            var permErr = AEDV.CheckAddEdit(Permission, isNew, head.DOCDT ?? DateTime.Today);
            if (permErr != null) return permErr;
            if (!UnitScope.IsAllowed(head.UNIT, await GetUnitsForUserAsync()))
                return $"You do not have permission for Unit {head.UNIT}.";
            return null;
        }

        // POST: OtherInvoice/Save -- VB6 Save step 2: the actual commit.
        [HttpPost]
        public async Task<ActionResult> Save(T_INV_DATA model)
        {
            var deny = await CheckSaveAllowedAsync(model);
            if (deny != null) return JsonExact(new { success = false, message = deny });
            var r = await Services.SalesPostAsync<JObject>("/api/OtherInvoice/SaveOrUpdate", model);
            return JsonExact(new { success = r.IsSuccessStatusCode, message = r.IsSuccessStatusCode ? "Saved successfully." : (r.Message ?? "Save failed."), data = r.Data });
        }

        // POST: OtherInvoice/Delete
        [HttpPost]
        public async Task<ActionResult> Delete(string tran, string docYear, string unit, string doctype, string docno)
        {
            tran = TranOf(tran);
            if (!(Permission?.Delete ?? false))
            {
                TempData["toastrError"] = "You do not have permission to delete this entry.";
                return RedirectToAction(TranActions[tran]);
            }

            // The row's Unit (a Doc No repeats across units) must be one the user is linked to.
            if (!UnitScope.IsAllowed(unit, await GetUnitsForUserAsync()))
            {
                TempData["toastrError"] = $"You do not have permission for Unit {unit}.";
                return RedirectToAction(TranActions[tran]);
            }

            var r = await Services.SalesPostAsync<JObject>(
                $"/api/OtherInvoice/Delete?docYear={E(docYear)}&unit={E(unit)}&doctype={E(doctype)}&docno={E(docno)}", new { });

            TempData[r.IsSuccessStatusCode ? "toastrSuccess" : "toastrError"] =
                r.IsSuccessStatusCode ? $"{docno} deleted." : (r.Message ?? "Delete failed.");
            return RedirectToAction(TranActions[tran]);
        }

        // GET: OtherInvoice/GetRowDetail (AJAX, the list's "+" toggle / Lines button). Uses the API's
        // lean GetRowLines ({ details: [...] } only, one query) -- the row's own key incl. Unit is passed.
        [HttpGet]
        public async Task<ActionResult> GetRowDetail(string docYear = "", string unit = "", string doctype = "", string docno = "")
        {
            var r = await Services.SalesGetAsync<JObject>(
                $"/api/OtherInvoice/GetRowLines?docYear={E(docYear)}&unit={E(unit)}&doctype={E(doctype)}&docno={E(docno)}");
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

        // Reshapes a plain-array lookup response into { data, count } for jquery.inputpicker
        // (Root UI convention -- see CLAUDE.md's "Lookup pickers" rule). The API returns up to its
        // top-100 matches (no rowCount of its own), so this pages that array in memory: `p`/`limit`
        // pick the slice and `count` is the number of matches, which is what the plugin's footer
        // shows and pages by. Same shape as PackingController.PickerJson.
        private ActionResult PickerJson(ResponseApiModel<JToken> r, int limit, int p)
        {
            var arr = (r?.IsSuccessStatusCode == true ? r.Data as JArray : null) ?? new JArray();
            var take = limit > 0 ? limit : 10;
            var page = arr.Skip((Math.Max(p, 1) - 1) * take).Take(take);
            return JsonExact(new { data = page, count = arr.Count });
        }

        // inputpicker endpoints (Party / Transporter / Bank / Item / GST Code). The plugin sends the
        // typed text as `q` (the API matches it, case-insensitively, against EVERY column the dropdown
        // shows) and -- when the field already holds a value -- that value as `value`, which is
        // looked up as an exact code so a loaded document's picker shows its own row.
        private async Task<ActionResult> Picker(string path, string q, string value, int limit, int p, string unit)
        {
            var arg = string.IsNullOrWhiteSpace(q) && !string.IsNullOrWhiteSpace(value)
                ? "code=" + E(value.Trim())
                : "search=" + E((q ?? "").Trim());
            return PickerJson(await Services.SalesGetAsync<JToken>($"/api/OtherInvoice/{path}?{arg}&unit={E(unit)}"), limit, p);
        }

        [HttpGet] public Task<ActionResult> GetParty(string q = "", int limit = 0, string fieldValue = "", string fieldText = "", string value = "", int p = 1, string unit = "") =>
            Picker("GetParty", q, value, limit, p, unit);
        [HttpGet] public Task<ActionResult> GetTransporter(string q = "", int limit = 0, string fieldValue = "", string fieldText = "", string value = "", int p = 1, string unit = "") =>
            Picker("GetTransporter", q, value, limit, p, unit);
        [HttpGet] public Task<ActionResult> GetBank(string q = "", int limit = 0, string fieldValue = "", string fieldText = "", string value = "", int p = 1, string unit = "") =>
            Picker("GetBank", q, value, limit, p, unit);
        [HttpGet] public Task<ActionResult> GetItemMaster(string q = "", int limit = 0, string fieldValue = "", string fieldText = "", string value = "", int p = 1, string unit = "") =>
            Picker("GetItemMaster", q, value, limit, p, unit);
        [HttpGet] public Task<ActionResult> GetGstCode(string q = "", int limit = 0, string fieldValue = "", string fieldText = "", string value = "", int p = 1, string unit = "") =>
            Picker("GetGstCode", q, value, limit, p, unit);
        // Plain (non-picker) GST master rows -- the entry screen loads the rate rows of the codes on
        // a saved / referenced document's lines with this (raw array, no paging).
        [HttpGet] public Task<ActionResult> GetGstRates(string unit = "", string code = "") =>
            Lookup($"GetGstCode?unit={E(unit)}&code={E(code)}");
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
