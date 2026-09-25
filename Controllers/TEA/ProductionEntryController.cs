using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PacketTea;
using PacketTea.Models;
using PacketTea.Models.PT;
using PacketTea.Utility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using static PacketTea.Helpers;

namespace Finance.Controllers.TEA
{
    // "TB Sales - Production Entry" -- ported from VB6 frmProduction.frm.
    // Modeled 1:1 on TeaSampleDrawEntryController/MasterBlendEntryController,
    // except every data call goes through Services.SalesGetAsync/
    // SalesPostAsync instead of the plain Get/PostAsync, because this module's
    // data lives in the Sales schema (CLASSIC_CONTROL.SCHEMA_SALES), not the
    // unit's normal operating schema.
    public class ProductionEntryController : Controller
    {
        private string CurrentLoca => SessionHelper.GetUser()?.Loca ?? "";

        private async Task<Dictionary<string, string>> GetAllowedTypesAsync()
        {
            var response = await Services.SalesGetAsync<Dictionary<string, string>>("/api/ProductionEntry/GetAllowedTypes");
            return (response.IsSuccessStatusCode ? response.Data : null) ?? new Dictionary<string, string> { { "PT", "Packet Tea" } };
        }

        // Backs the list page's "New" inline row Unit picker.
        private async Task<List<UnitOption>> GetUnitsForUserAsync()
        {
            var response = await Services.SalesGetAsync<List<UnitOption>>("/api/ProductionEntry/GetUnitsForUser");
            return (response.IsSuccessStatusCode ? response.Data : null) ?? new List<UnitOption>();
        }

        // GET: ProductionEntry
        public async Task<ActionResult> Index(string type, string unit, string searchString, int? page = 1, int pageSize = 15, string sortBy = "", string sortDir = "")
        {
            var sdsd = (List<AEDV>)Session["User_AEDV"];
            ViewBag.Permission = AEDV.ForScreen(sdsd, "ProductionEntry");
            ViewBag.CurrentFilter = searchString;
            ViewBag.SortBy = sortBy;
            ViewBag.SortDir = sortDir;
            ViewBag.PageSize = pageSize;
            ViewBag.Type = type;
            ViewBag.Unit = unit;
            // The Types the user has rights to and the Units they are linked to -- independent
            // lookups, so run them side by side.
            var typesTask = GetAllowedTypesAsync();
            var unitsTask = GetUnitsForUserAsync();
            await Task.WhenAll(typesTask, unitsTask);
            ViewBag.Types = typesTask.Result;
            var userUnits = unitsTask.Result;
            ViewBag.UnitList = userUnits;
            // Only the Units the user is linked to: the one asked for (if permitted) else every one of
            // them, comma-separated (API GetByPage parses it) -- never an unfiltered blank `unit`.
            var unitFilter = Uri.EscapeDataString(UnitScope.ListFilter(unit, userUnits));

            // Chunked list: a full page view always starts at the first chunk; further chunks
            // arrive as AJAX calls (with the same sort) and return just the table rows.
            var isChunkRequest = Request.IsAjaxRequest();
            var pageNo = isChunkRequest ? Math.Max(page ?? 1, 1) : 1;
            ViewBag.Page = pageNo;
            ViewBag.RowOffset = (pageNo - 1) * pageSize;

            var response = await Services.SalesGetAsync<PageModel<dynamic>>(
                $"/api/ProductionEntry/GetByPage?type={Uri.EscapeDataString(type ?? "")}&unit={unitFilter}&search={Uri.EscapeDataString(searchString ?? "")}&page={pageNo}&pageSize={pageSize}&sortBy={Uri.EscapeDataString(sortBy ?? "")}&sortDir={Uri.EscapeDataString(sortDir ?? "")}");

            var json = response?.Data != null ? JsonConvert.SerializeObject(response.Data) : null;
            var wrapper = json != null ? JsonConvert.DeserializeObject<GetByPageResult>(json) : null;
            var list = wrapper?.value?.results ?? new List<ProdDocRow>();
            ViewBag.RowCount = wrapper?.value?.rowCount ?? 0;

            if (isChunkRequest)
            {
                // A failed chunk must not leave a toast queued for the next full page load; an
                // empty response tells the list's scroll loader to stop.
                return PartialView("_ListRows", (response?.IsSuccessStatusCode ?? false) ? list : new List<ProdDocRow>());
            }

            if (!response?.IsSuccessStatusCode ?? false)
            {
                TempData["toastrError"] = !string.IsNullOrEmpty(response?.Message)
                    ? response.Message
                    : $"Unable to load Production Entry list (API returned {response?.StatusCode}).";
            }

            return View(list);
        }

        private class GetByPageResult { public GetByPageValue value { get; set; } }
        private class GetByPageValue { public List<ProdDocRow> results { get; set; } public int rowCount { get; set; } }

        // GET: ProductionEntry/InsertOrUpdate
        // `unit` is the Unit picked in the list's "New" row (new entry) or the row's own Unit for
        // Edit / View (a Doc No repeats across units).
        public async Task<ActionResult> InsertOrUpdate(string docno = "", string type = "", string unit = "", bool view = false)
        {
            var sdsd = (List<AEDV>)Session["User_AEDV"];
            var permission = AEDV.ForScreen(sdsd, "ProductionEntry");
            ViewBag.Permission = permission;

            // `view` is set by the list's View button -- same fetch as Edit, but the form renders
            // read-only. It only applies to an existing record and needs the View right.
            view = view && !string.IsNullOrEmpty(docno);
            if (view && !(permission?.View ?? false))
            {
                TempData["toastrError"] = "You do not have permission to view this entry.";
                return RedirectToAction("Index", new { type });
            }
            ViewBag.IsView = view;

            bool isNewEntry = string.IsNullOrEmpty(docno);

            // The list page's New button already looks disabled without the Add right, but this
            // URL is reachable directly -- block it here too (enforce rights server-side).
            if (isNewEntry && !(permission?.Add ?? false))
            {
                TempData["toastrError"] = "You do not have permission to add a new entry.";
                return RedirectToAction("Index", new { type });
            }

            var allowedTypes = await GetAllowedTypesAsync();
            ViewBag.Types = allowedTypes;
            ViewBag.LockedFromList = string.IsNullOrEmpty(docno) && !string.IsNullOrEmpty(unit);

            // Doc Date picker's lower bound -- the later of the financial-year start and the AEDV
            // back-date allowance (Aday for a new entry, Eday while editing). Save() is the actual
            // enforcement point; this just steers the date picker.
            DateTime? fyStartDate = null;
            if (TryGetFinancialYear(out var fyFrom, out var fyTo))
            {
                fyStartDate = fyFrom;
                ViewBag.FyStart = fyFrom.ToString("yyyy-MM-dd");
                ViewBag.FyEnd = fyTo.ToString("yyyy-MM-dd");
            }
            var backDateFloor = (permission ?? new AEDV()).MinDocDate(isNewEntry);
            var effectiveMin = fyStartDate.HasValue && fyStartDate.Value > backDateFloor ? fyStartDate.Value : backDateFloor;
            ViewBag.MinDocDate = effectiveMin.ToString("yyyy-MM-dd");

            if (string.IsNullOrEmpty(docno))
            {
                // A Unit passed in must be one the user is linked to (Save re-checks it too).
                if (!string.IsNullOrEmpty(unit) && !UnitScope.IsAllowed(unit, await GetUnitsForUserAsync()))
                {
                    TempData["toastrError"] = $"You do not have permission for Unit {unit}.";
                    return RedirectToAction("Index", new { type });
                }

                var effectiveType = !string.IsNullOrEmpty(type) ? type : allowedTypes.Keys.FirstOrDefault() ?? "PT";
                var model = new T_PROD_DATA
                {
                    T_PROD = new T_PROD
                    {
                        UNIT = unit,
                        DATE_ORA = DateTime.Today,
                        MFGDT = DateTime.Today,
                        PD = 0,
                        GROSS_WEIGHT = 0,
                        NET_WEIGHT = 0,
                        GAIN_LOSS = 0
                    }
                };
                ViewBag.IsEdit = false;
                ViewBag.NewUnit = unit;
                ViewBag.NewType = effectiveType;
                return View(model);
            }

            // The record must belong to a Unit the user is linked to -- checked alongside the load,
            // against the row's own Unit (or the one the list passed).
            var unitsTask = GetUnitsForUserAsync();
            var response = await Services.SalesGetAsync<dynamic>($"/api/ProductionEntry/GetByDocNo?docno={Uri.EscapeDataString(docno)}&unit={Uri.EscapeDataString(unit ?? "")}");
            var userUnits = await unitsTask;
            if (!response.IsSuccessStatusCode || response.Data == null)
            {
                TempData["toastrError"] = response.Message ?? "Record not found.";
                return RedirectToAction("Index", new { type, unit });
            }

            var json = JsonConvert.SerializeObject(response.Data);
            var wrapper = JsonConvert.DeserializeObject<GetByDocNoResult>(json);

            var editModel = new T_PROD_DATA { T_PROD = wrapper.row ?? new T_PROD() };
            var recordUnit = !string.IsNullOrEmpty(editModel.T_PROD.UNIT) ? editModel.T_PROD.UNIT : unit;
            if (!UnitScope.IsAllowed(recordUnit, userUnits))
            {
                TempData["toastrError"] = $"You do not have permission for Unit {recordUnit}.";
                return RedirectToAction("Index", new { type });
            }

            // Block opening Edit outright when the user has no Edit right, or the record's own Doc
            // Date has fallen outside the Eday back-date window -- View bypasses this (read-only
            // regardless of the Edit/back-date policy).
            if (!view)
            {
                var editErr = AEDV.CheckAddEdit(permission, false, editModel.T_PROD.DATE_ORA ?? DateTime.Today);
                if (editErr != null)
                {
                    TempData["toastrError"] = editErr;
                    return RedirectToAction("Index", new { type });
                }
            }

            ViewBag.IsEdit = true;
            ViewBag.NewUnit = editModel.T_PROD.UNIT;
            ViewBag.NewType = type;
            return View(editModel);
        }

        private class GetByDocNoResult { public T_PROD row { get; set; } }

        // GET: ProductionEntry/GetRowDetail (AJAX, fired by the Index list's "+" toggle)
        // Uses the API's lean GetRowLines ({ rows: [...] } only); the row's own Unit is passed
        // (a Doc No repeats across units).
        [HttpGet]
        public async Task<ActionResult> GetRowDetail(string docno = "", string unit = "")
        {
            var r = await Services.SalesGetAsync<dynamic>(
                $"/api/ProductionEntry/GetRowLines?docno={Uri.EscapeDataString(docno ?? "")}&unit={Uri.EscapeDataString(unit ?? "")}");
            if (!r.IsSuccessStatusCode || r.Data == null)
                return JsonExact(new { success = false, message = r.Message ?? "Record not found." });

            var obj = JObject.FromObject(r.Data);
            obj["success"] = true;
            return JsonExact(obj);
        }

        // POST: ProductionEntry/Save
        // Validation mirrors VB frmProduction Command1_Click + its field Validate events.
        // Lock date ("PLAN"), the edtype back-date window, the edit-mode negative-stock
        // check and docno/id_ent generation stay with the API's SaveOrUpdate.
        [HttpPost]
        public async Task<JsonResult> Save(T_PROD_DATA model)
        {
            var p = model?.T_PROD;
            if (p == null)
                return Json(new { success = false, message = "Nothing to save." });

            // Server-side AEDV Add/Edit + back-date enforcement -- the date picker's `min`
            // (see InsertOrUpdate) only steers well-behaved clients; this is the actual gate.
            // `IsNew` comes from the client (IsNew: !IS_EDIT); a blank Doc No means new otherwise.
            var sdsd = (List<AEDV>)Session["User_AEDV"];
            var permission = AEDV.ForScreen(sdsd, "ProductionEntry");
            bool isNew = model.IsNew ?? string.IsNullOrEmpty(p.DOCNO);
            var permErr = AEDV.CheckAddEdit(permission, isNew, p.DATE_ORA ?? DateTime.Today);
            if (permErr != null)
                return Json(new { success = false, message = permErr });

            // The Unit posted from the entry screen (the one picked on the list for a NEW record,
            // the record's own for an edit) must be one the user is linked to.
            var postedUnit = (p.UNIT ?? "").Trim();
            if (postedUnit.Length == 0)
                return Json(new { success = false, message = "Select a specific unit before saving a Production entry." });
            if (!UnitScope.IsAllowed(postedUnit, await GetUnitsForUserAsync()))
                return Json(new { success = false, message = $"You do not have permission for Unit {postedUnit}." });

            p.LOCA = CurrentLoca;
            p.ITCD = (p.ITCD ?? "").Trim();
            p.MRP = (p.MRP ?? "").Trim();
            p.BATCHNO = (p.BATCHNO ?? "").Trim().ToUpperInvariant();
            p.SHIFT = (p.SHIFT ?? "").Trim();
            p.TM = "T";

            var error = await ValidateAsync(p);
            if (error != null)
                return Json(new { success = false, message = error.Message, field = error.Field });

            var response = await Services.SalesPostAsync<dynamic>("/api/ProductionEntry/SaveOrUpdate", p);
            return Json(new
            {
                success = response.IsSuccessStatusCode,
                message = response.IsSuccessStatusCode ? "Saved successfully." : (response.Message ?? "Save failed."),
                data = response.Data
            });
        }

        // POST: ProductionEntry/Delete
        [HttpPost]
        public async Task<ActionResult> Delete(string docno, string type, string unit)
        {
            var sdsd = (List<AEDV>)Session["User_AEDV"];
            var permission = AEDV.ForScreen(sdsd, "ProductionEntry");
            if (!(permission?.Delete ?? false))
            {
                TempData["toastrError"] = "You do not have permission to delete this entry.";
                return RedirectToAction("Index", new { type });
            }

            // The row's Unit (a Doc No repeats across units) must be one the user is linked to.
            if (!UnitScope.IsAllowed(unit, await GetUnitsForUserAsync()))
            {
                TempData["toastrError"] = string.IsNullOrWhiteSpace(unit)
                    ? "Select a specific unit to delete a Production entry."
                    : $"You do not have permission for Unit {unit}.";
                return RedirectToAction("Index", new { type });
            }

            var response = await Services.SalesPostAsync<dynamic>($"/api/ProductionEntry/Delete?docno={Uri.EscapeDataString(docno ?? "")}&unit={Uri.EscapeDataString(unit ?? "")}", new { });

            TempData[response.IsSuccessStatusCode ? "toastrSuccess" : "toastrError"] =
                response.IsSuccessStatusCode ? "Deleted successfully." : (response.Message ?? "Delete failed.");

            return RedirectToAction("Index", new { type, unit });
        }

        // =====================================================================
        // Save validation (VB frmProduction rules)
        // =====================================================================
        private class ValidationError
        {
            public string Message { get; set; }
            public string Field { get; set; }
            public ValidationError(string message, string field) { Message = message; Field = field; }
        }

        private class ItemInfo
        {
            public string Code { get; set; }
            public string Name { get; set; }
            public int Nob { get; set; }
            public int Bbd { get; set; }
        }

        private async Task<ValidationError> ValidateAsync(T_PROD p)
        {
            bool isEdit = !string.IsNullOrEmpty(p.DOCNO);
            var today = DateTime.Today;

            if (!p.DATE_ORA.HasValue) return new ValidationError("Date required", "DATE_ORA");
            var docDate = p.DATE_ORA.Value.Date;

            if (TryGetFinancialYear(out var fyFrom, out var fyTo) && (docDate < fyFrom || docDate > fyTo))
                return new ValidationError("Date not in this financial Year", "DATE_ORA");

            var seqError = await ValidateDateSequenceAsync(p.UNIT, p.DOCNO, docDate, isEdit);
            if (seqError != null) return seqError;

            if (string.IsNullOrEmpty(p.ITCD)) return new ValidationError("Item code required", "ITCD");
            var item = await GetItemAsync(p.ITCD);
            if (item == null) return new ValidationError("Invalid Item", "ITCD");

            if (string.IsNullOrEmpty(p.MRP)) return new ValidationError("MRP required", "MRP");
            if (p.MRP.Length > 4) return new ValidationError("MRP can be at most 4 characters", "MRP");
            if (string.IsNullOrEmpty(p.BATCHNO)) return new ValidationError("Batchno Number required", "BATCHNO");

            if (!p.MFGDT.HasValue) return new ValidationError("Enter Manufacturing Date", "MFGDT");
            var mfgDate = p.MFGDT.Value.Date;
            if (mfgDate > today) return new ValidationError("Can not enter future dated Manufacturing Date", "MFGDT");
            // VB treats a missing BBD as 0, which would reject every entry -- only enforce a real BBD.
            if (item.Bbd > 0 && mfgDate <= today.AddDays(-item.Bbd))
                return new ValidationError("Material age is more than BBD Period (" + item.Bbd + " days)", "MFGDT");

            if (TimeSpan.TryParse(p.TIME_FROM ?? "", out var tFrom) && TimeSpan.TryParse(p.TIME_TO ?? "", out var tTo) && tFrom >= tTo)
                return new ValidationError("Time To should be greater than Time From", "TIME_TO");

            var pd = p.PD ?? 0;
            if (pd < 0) return new ValidationError("Net Production can not be negative", "PD");
            // Quantities are "cases.pieces": the decimal part is a piece count, so it must be below NOB.
            int pieces = (int)Math.Round((pd - Math.Truncate(pd)) * 100);
            if (item.Nob > 1 && pieces > item.Nob - 1)
                return new ValidationError("Decimal part cannot be more than " + (item.Nob - 1), "PD");

            p.PD = pd;
            p.PD_BTLS = Math.Truncate(pd) * item.Nob + pieces;
            p.GROSS_WEIGHT = p.GROSS_WEIGHT ?? 0;
            p.NET_WEIGHT = p.NET_WEIGHT ?? 0;
            p.GAIN_LOSS = p.GAIN_LOSS ?? 0;
            return null;
        }

        // Session financial year looks like "01/04/2026 - 31/03/2027".
        private static bool TryGetFinancialYear(out DateTime from, out DateTime to)
        {
            from = to = DateTime.MinValue;
            var matches = Regex.Matches(SessionHelper.GetUser()?.Financialyear ?? "", @"\d{2}/\d{2}/\d{4}");
            return matches.Count >= 2
                && DateTime.TryParseExact(matches[0].Value, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out from)
                && DateTime.TryParseExact(matches[1].Value, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out to);
        }

        // Add: date cannot be earlier than the unit's latest entry.
        // Edit: date must sit between the previous and next doc of the same unit.
        private async Task<ValidationError> ValidateDateSequenceAsync(string unit, string docno, DateTime docDate, bool isEdit)
        {
            if (!isEdit)
            {
                var latest = (await GetUnitPageAsync(unit, "DOCDT", "desc", 1, 1)).FirstOrDefault();
                if (latest?.docdt != null && latest.docdt.Value.Date > docDate)
                    return new ValidationError("Date cannot be less than " + latest.docdt.Value.ToString("dd/MM/yyyy"), "DATE_ORA");
                return null;
            }

            const int pageSize = 500;
            ProdDocRow prev = null, next = null;
            bool found = false;
            for (int page = 1; page <= 40; page++)
            {
                var rows = await GetUnitPageAsync(unit, "DOCNO", "asc", page, pageSize);
                foreach (var row in rows)
                {
                    if (found) { next = row; break; }
                    if (row.docno == docno) found = true; else prev = row;
                }
                if (next != null || rows.Count < pageSize) break;
            }
            if (!found) return null;

            if (prev?.docdt != null && prev.docdt.Value.Date > docDate)
                return new ValidationError("Date should be more than " + prev.docdt.Value.ToString("dd/MM/yyyy"), "DATE_ORA");
            if (next?.docdt != null && next.docdt.Value.Date < docDate)
                return new ValidationError("Date should be less than " + next.docdt.Value.ToString("dd/MM/yyyy"), "DATE_ORA");
            return null;
        }

        private async Task<List<ProdDocRow>> GetUnitPageAsync(string unit, string sortBy, string sortDir, int page, int pageSize)
        {
            var response = await Services.SalesGetAsync<PageModel<dynamic>>(
                $"/api/ProductionEntry/GetByPage?type=&unit={Uri.EscapeDataString(unit ?? "")}&search=&page={page}&pageSize={pageSize}&sortBy={sortBy}&sortDir={sortDir}");
            if (response?.Data == null) return new List<ProdDocRow>();
            var wrapper = JsonConvert.DeserializeObject<GetByPageResult>(JsonConvert.SerializeObject(response.Data));
            return wrapper?.value?.results ?? new List<ProdDocRow>();
        }

        private async Task<ItemInfo> GetItemAsync(string code)
        {
            var r = await Services.SalesGetAsync<dynamic>($"/api/ProductionEntry/GetItemMaster?search={Uri.EscapeDataString(code)}&pageSize=50");
            if (!r.IsSuccessStatusCode || r.Data == null) return null;
            if (!(JToken.FromObject(r.Data) is JArray rows)) return null;

            var match = rows.FirstOrDefault(row => string.Equals(Field(row, "CODE")?.Trim(), code, StringComparison.OrdinalIgnoreCase));
            if (match == null) return null;

            decimal.TryParse(Field(match, "NOB"), NumberStyles.Any, CultureInfo.InvariantCulture, out var nob);
            decimal.TryParse(Field(match, "BBD"), NumberStyles.Any, CultureInfo.InvariantCulture, out var bbd);
            return new ItemInfo { Code = code, Name = Field(match, "NAME"), Nob = nob >= 1 ? (int)nob : 1, Bbd = (int)bbd };
        }

        private static string Field(JToken row, string name) =>
            (row as JObject)?.Properties()
                .FirstOrDefault(prop => string.Equals(prop.Name, name, StringComparison.OrdinalIgnoreCase))?
                .Value?.ToString();

        // =====================================================================
        // AJAX lookup passthroughs
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

        // Item picker endpoint for jquery.inputpicker (Root UI convention -- see CLAUDE.md's
        // "Lookup pickers" rule): the plugin's own (q, limit, ..., p) contract and { data, count }
        // response. The API matches `q` against both columns the dropdown shows (Code, Name). It
        // has no paging of its own, so `count` is just what came back -- one page. A failed call
        // keeps the session-expired / error surfacing (the entry screen's Item validation also
        // reads this endpoint, and must not mistake a failure for "Invalid Item").
        [HttpGet]
        public async Task<ActionResult> GetItemMaster(string q = "", int limit = 0, string fieldValue = "", string fieldText = "", string value = "", int p = 1)
        {
            if (string.IsNullOrEmpty(q)) q = value;
            var r = await Services.SalesGetAsync<dynamic>($"/api/ProductionEntry/GetItemMaster?search={Uri.EscapeDataString((q ?? "").Trim())}&pageSize={(limit > 0 ? limit : 50)}");
            if (!r.IsSuccessStatusCode || r.Data == null) return JsonExactOrSessionExpired(r);
            var arr = JToken.FromObject(r.Data) as JArray ?? new JArray();
            return JsonExact(new { data = arr, count = arr.Count });
        }
    }

    public class ProdDocRow
    {
        public string docno { get; set; }
        public DateTime? docdt { get; set; }
        public string itcd { get; set; }
        public string itemName { get; set; }
        public string mrp { get; set; }
        public string batchno { get; set; }
        public DateTime? mfgdt { get; set; }
        public string unit { get; set; }
        public decimal? pd { get; set; }
        public decimal? grossWeight { get; set; }
        public decimal? netWeight { get; set; }
    }
}
