using Newtonsoft.Json;
using PacketTea;
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
    // "AWR Entry" -- ported from VB6 TRN_AWR.frm, Packet Tea (PT) transaction
    // sub-type only. Modeled 1:1 on MasterBlendEntryController -- see that
    // class's comments for the rationale behind the rights-filtered type list,
    // the AWRTypeUnit stopgap, and the session-expiry-surfacing lookups.
    //
    // Brought in line with Packing Entry (see PackingController): Unit-wise permission
    // (USER_SCHEMA_LINK via GetUnitsForUser), AEDV Add/Edit/Delete/View + back-date policy
    // (AEDV.ForScreen "AWREntry"), chunked infinite-scroll list, Root UI (inputpicker) lookups.
    // The list starts with a Unit column; each row also carries its Unit + AWR Type as data
    // attributes / link params.
    //
    // Scope note (agreed plan, Phase 1): Damage/Shortage and Package Detail
    // sub-grids, and every JST-only field, are intentionally not exposed here.
    public class AWREntryController : Controller
    {
        // Same value domain as MasterBlendEntryController.BlendTypes -- the user
        // confirmed AWR Type reuses it exactly, just under a different label.
        public static readonly Dictionary<string, string> AWRTypes = new Dictionary<string, string>
        {
            { "PT", "Packet Tea" },
            { "TT", "Tea Trading" },
            { "WT", "Web Tea" },
            { "BT", "Bagicha Tea" },
            { "ST", "Sale Tea" },
            { "TB", "Birla Tea" }
        };

        private static readonly Dictionary<string, string> AWRTypeUnit = new Dictionary<string, string>
        {
            { "PT", "GORA" },
            { "TT", "JSTI" },
            { "WT", "JSTI" },
            { "BT", "BGCH" },
            { "ST", "TTSI" },
            { "TB", "JSTI" },
        };

        private string CurrentUnit => SessionHelper.GetUser()?.CurentUnit ?? "";
        private string CurrentLoca => SessionHelper.GetUser()?.Loca ?? "";
        private string UnitForAWRType(string awrType) =>
            !string.IsNullOrEmpty(CurrentUnit) ? CurrentUnit :
            (awrType != null && AWRTypeUnit.TryGetValue(awrType, out var u) ? u : "");

        private async Task<Dictionary<string, string>> GetAllowedAWRTypesAsync()
        {
            var response = await Services.GetAsync<List<string>>("/api/TeaAWR/GetAllowedAWRTypes");
            if (!response.IsSuccessStatusCode || response.Data == null)
                return AWRTypes;
            return AWRTypes.Where(t => response.Data.Contains(t.Key))
                            .ToDictionary(t => t.Key, t => t.Value);
        }

        private async Task<List<UnitOption>> GetUnitsForUserAsync()
        {
            var response = await Services.GetAsync<List<UnitOption>>("/api/TeaAWR/GetUnitsForUser");
            return (response.IsSuccessStatusCode ? response.Data : null) ?? new List<UnitOption>();
        }

        // GET: AWREntry
        // Scoped to the Units the user is linked to (and to `unit` when given) and to every AWR
        // Type the user has rights to -- see MasterBlendEntryController.Index / PackingController.Index.
        // The list shows a Unit column, and each row carries its own Unit + AWR Type as data
        // attributes / link params (a Doc No repeats across units).
        public async Task<ActionResult> Index(string awrType, string searchString, int? page = 1, int pageSize = 15, string sortBy = "", string sortDir = "", string unit = "")
        {
            var sdsd = (List<AEDV>)Session["User_AEDV"];
            ViewBag.Permission = AEDV.ForScreen(sdsd, "AWREntry");
            ViewBag.CurrentFilter = searchString;
            ViewBag.SortBy = sortBy;
            ViewBag.SortDir = sortDir;
            ViewBag.PageSize = pageSize;
            ViewBag.AWRType = awrType;
            // AWR Types the user has rights to and the Units they are linked to -- independent
            // lookups, so run them side by side.
            var typesTask = GetAllowedAWRTypesAsync();
            var unitsTask = GetUnitsForUserAsync();
            await Task.WhenAll(typesTask, unitsTask);
            var allowedTypes = typesTask.Result;
            ViewBag.AWRTypes = allowedTypes;
            var userUnits = unitsTask.Result;
            ViewBag.UnitList = userUnits;
            var unitFilter = Uri.EscapeDataString(UnitScope.ListFilter(unit, userUnits));
            // The list covers every permitted type as ONE comma-separated list, not the single
            // `awrType` in the URL (API TeaBlendController.ParseUnits style). No permitted type at
            // all sends a code that matches nothing rather than an (unfiltered) blank.
            var typeFilter = Uri.EscapeDataString(allowedTypes.Count == 0 ? UnitScope.NoUnits : string.Join(",", allowedTypes.Keys));

            // Chunked list: a full page view always starts at the first chunk; further chunks
            // arrive as AJAX calls (with the same sort) and return just the table rows.
            var isChunkRequest = Request.IsAjaxRequest();
            var pageNo = isChunkRequest ? Math.Max(page ?? 1, 1) : 1;
            ViewBag.Page = pageNo;
            ViewBag.RowOffset = (pageNo - 1) * pageSize;

            var response = await Services.GetAsync<PageModel<dynamic>>(
                $"/api/TeaAWR/GetByPage?awrType={typeFilter}&unit={unitFilter}&search={Uri.EscapeDataString(searchString ?? "")}&page={pageNo}&pageSize={pageSize}&sortBy={Uri.EscapeDataString(sortBy ?? "")}&sortDir={Uri.EscapeDataString(sortDir ?? "")}");

            var json = response?.Data != null ? JsonConvert.SerializeObject(response.Data) : null;
            var wrapper = json != null ? JsonConvert.DeserializeObject<GetByPageResult>(json) : null;
            var list = wrapper?.value?.results ?? new List<AWRDocRow>();
            ViewBag.RowCount = wrapper?.value?.rowCount ?? 0;

            if (isChunkRequest)
            {
                // A failed chunk must not leave a toast queued for the next full page load; an
                // empty response tells the list's scroll loader to stop.
                return PartialView("_ListRows", (response?.IsSuccessStatusCode ?? false) ? list : new List<AWRDocRow>());
            }

            if (!response?.IsSuccessStatusCode ?? false)
            {
                TempData["toastrError"] = !string.IsNullOrEmpty(response?.Message)
                    ? response.Message
                    : $"Unable to load AWR Entry list (API returned {response?.StatusCode}).";
            }

            return View(list);
        }

        private class GetByPageResult { public GetByPageValue value { get; set; } }
        private class GetByPageValue { public List<AWRDocRow> results { get; set; } public int rowCount { get; set; } }

        // GET: AWREntry/InsertOrUpdate
        // `unit` is populated when this was opened from the list -- the Unit picked in the New
        // row (new entry; honored over the AWRTypeUnit stopgap guess and locks the AWR Type), or
        // the row's own Unit for Edit / View (a Doc No repeats across units).
        public async Task<ActionResult> InsertOrUpdate(string docno = "", string awrDate = "", string awrType = "", string unit = "", bool view = false)
        {
            var sdsd = (List<AEDV>)Session["User_AEDV"];
            var permission = AEDV.ForScreen(sdsd, "AWREntry");
            ViewBag.Permission = permission;

            // `view` is set by the list's View button -- same fetch as Edit, but the form renders
            // read-only. It only applies to an existing record and needs the View right.
            view = view && !string.IsNullOrEmpty(docno);
            if (view && !(permission?.View ?? false))
            {
                TempData["toastrError"] = "You do not have permission to view this entry.";
                return RedirectToAction("Index", new { awrType });
            }
            ViewBag.IsView = view;

            var allowedAWRTypes = await GetAllowedAWRTypesAsync();
            ViewBag.AWRTypes = allowedAWRTypes;
            ViewBag.LockedFromList = string.IsNullOrEmpty(docno) && !string.IsNullOrEmpty(unit) && !string.IsNullOrEmpty(awrType);

            bool isNewEntry = string.IsNullOrEmpty(docno);

            // The list page's New button already looks disabled without the Add right, but this
            // URL is reachable directly -- block it here too (enforce rights server-side).
            if (isNewEntry && !(permission?.Add ?? false))
            {
                TempData["toastrError"] = "You do not have permission to add a new entry.";
                return RedirectToAction("Index", new { awrType });
            }

            string fy = Session["SelectedfinancialYear"]?.ToString();
            DateTime? fyStartDate = null;
            if (!string.IsNullOrEmpty(fy) && fy.Contains("-"))
            {
                var parts = fy.Split('-');
                string startDigits = new string(parts[0].Where(char.IsDigit).ToArray());
                string endDigits = new string(parts[1].Where(char.IsDigit).ToArray());
                if (startDigits.Length >= 4 && endDigits.Length >= 4)
                {
                    fyStartDate = new DateTime(int.Parse(startDigits.Substring(startDigits.Length - 4)), 4, 1);
                    ViewBag.FyStart = fyStartDate.Value.ToString("yyyy-MM-dd");
                    ViewBag.FyEnd = new DateTime(int.Parse(endDigits.Substring(endDigits.Length - 4)), 3, 31).ToString("yyyy-MM-dd");
                }
            }

            // AWR Date picker's lower bound -- the later of the financial-year start and the AEDV
            // back-date allowance (Aday for a new entry, Eday while editing). Save() is the actual
            // enforcement point; this just steers the date picker.
            var backDateFloor = (permission ?? new AEDV()).MinDocDate(isNewEntry);
            var effectiveMin = fyStartDate.HasValue && fyStartDate.Value > backDateFloor ? fyStartDate.Value : backDateFloor;
            ViewBag.MinDocDate = effectiveMin.ToString("yyyy-MM-dd");

            if (string.IsNullOrEmpty(docno))
            {
                var effectiveAWRType = !string.IsNullOrEmpty(awrType) ? awrType :
                    allowedAWRTypes.ContainsKey("PT") ? "PT" :
                    allowedAWRTypes.Keys.FirstOrDefault() ?? "PT";
                var model = new T_AWR_PT_DATA
                {
                    T_AWR = new List<T_AWR>()
                };
                ViewBag.IsEdit = false;
                // Honor the Unit explicitly chosen on the list page over the AWRTypeUnit stopgap
                // guess -- see UnitForAWRType. Save() checks it against the user's units.
                ViewBag.NewUnit = !string.IsNullOrEmpty(unit) ? unit : UnitForAWRType(effectiveAWRType);
                ViewBag.NewAWRType = effectiveAWRType;
                return View(model);
            }

            // The record must belong to a Unit the user is linked to -- checked alongside the load,
            // against the row's own Unit (or the one the list passed).
            var unitsTask = GetUnitsForUserAsync();
            var response = await Services.GetAsync<dynamic>(
                $"/api/TeaAWR/GetByDocNo?docno={Uri.EscapeDataString(docno)}&awrDate={Uri.EscapeDataString(awrDate ?? "")}&awrType={Uri.EscapeDataString(awrType ?? "")}&unit={Uri.EscapeDataString(!string.IsNullOrEmpty(unit) ? unit : CurrentUnit)}");
            var userUnits = await unitsTask;

            if (!response.IsSuccessStatusCode || response.Data == null)
            {
                TempData["toastrError"] = response.Message ?? "Record not found.";
                return RedirectToAction("Index", new { awrType });
            }

            var json = JsonConvert.SerializeObject(response.Data);
            var wrapper = JsonConvert.DeserializeObject<GetByDocNoResult>(json);

            var editModel = new T_AWR_PT_DATA { T_AWR = wrapper.rows ?? new List<T_AWR>() };
            var recordUnit = editModel.T_AWR.FirstOrDefault()?.UNIT;
            if (!UnitScope.IsAllowed(!string.IsNullOrEmpty(recordUnit) ? recordUnit : unit, userUnits))
            {
                TempData["toastrError"] = $"You do not have permission for Unit {(!string.IsNullOrEmpty(recordUnit) ? recordUnit : unit)}.";
                return RedirectToAction("Index", new { awrType });
            }

            // Block opening Edit outright when the user has no Edit right, or the record's own AWR
            // Date has fallen outside the Eday back-date window -- View bypasses this (read-only
            // regardless of the Edit/back-date policy).
            if (!view)
            {
                var editErr = AEDV.CheckAddEdit(permission, false, editModel.T_AWR.FirstOrDefault()?.AWR_DATE ?? DateTime.Today);
                if (editErr != null)
                {
                    TempData["toastrError"] = editErr;
                    return RedirectToAction("Index", new { awrType });
                }
            }

            ViewBag.IsEdit = true;
            ViewBag.NewUnit = editModel.T_AWR.FirstOrDefault()?.UNIT;
            ViewBag.NewAWRType = editModel.T_AWR.FirstOrDefault()?.TRAN_TYPE ?? awrType;
            return View(editModel);
        }

        private class GetByDocNoResult { public List<T_AWR> rows { get; set; } }

        // GET: AWREntry/GetRowDetail (AJAX, fired by the Index list's "+" toggle)
        [HttpGet]
        // Uses the API's lean GetRowLines ({ rows: [...] } only -- no whole-master-table name
        // reads); the row's own Unit is passed (a Doc No repeats across units).
        public async Task<ActionResult> GetRowDetail(string docno = "", string awrDate = "", string awrType = "", string unit = "")
        {
            var r = await Services.GetAsync<dynamic>(
                $"/api/TeaAWR/GetRowLines?docno={Uri.EscapeDataString(docno ?? "")}&awrDate={Uri.EscapeDataString(awrDate ?? "")}&awrType={Uri.EscapeDataString(awrType ?? "")}&unit={Uri.EscapeDataString(unit ?? "")}");
            if (!r.IsSuccessStatusCode || r.Data == null)
                return JsonExact(new { success = false, message = r.Message ?? "Record not found." });

            var obj = (Newtonsoft.Json.Linq.JObject)r.Data;
            obj["success"] = true;
            return JsonExact(obj);
        }

        // POST: AWREntry/Save
        [HttpPost]
        public async Task<JsonResult> Save(T_AWR_PT_DATA model)
        {
            // Server-side AEDV Add/Edit + back-date enforcement -- the date picker's `min`
            // (see InsertOrUpdate) only steers well-behaved clients; this is the actual gate.
            // T_AWR is denormalized (no header object), so the AWR Date is read off the first line.
            var sdsd = (List<AEDV>)Session["User_AEDV"];
            var permission = AEDV.ForScreen(sdsd, "AWREntry");
            var firstRow = model?.T_AWR?.FirstOrDefault();
            bool isNew = model?.IsNew ?? string.IsNullOrEmpty(firstRow?.DOCNO);
            var permErr = AEDV.CheckAddEdit(permission, isNew, firstRow?.AWR_DATE ?? DateTime.Today);
            if (permErr != null)
                return Json(new { success = false, message = permErr });

            // The Unit posted from the entry screen (the one picked on the list for a NEW record,
            // the record's own for an edit) must be one the user is linked to -- see
            // MasterBlendEntryController.Save. A blank Unit falls back to the AWRTypeUnit guess below.
            var postedUnits = (model?.T_AWR ?? new List<T_AWR>())
                .Select(r => (r.UNIT ?? "").Trim()).Where(u => u.Length > 0).Distinct().ToList();
            if (postedUnits.Count > 0)
            {
                var userUnits = await GetUnitsForUserAsync();
                var denied = postedUnits.FirstOrDefault(u => !UnitScope.IsAllowed(u, userUnits));
                if (denied != null)
                    return Json(new { success = false, message = $"You do not have permission for Unit {denied}." });
            }

            // LOCA is never posted by the client (InsertOrUpdate.cshtml's payload builder
            // doesn't collect it -- there's no form field for it), so every row arrived with
            // LOCA null. The legacy VB6 form always supplied it on every insert, and every
            // existing T_AWR row has it set (all "JST" in this company's data), so stamp it
            // here from the session -- same source (SessionHelper.GetUser().Loca) several
            // other PT models already default to -- rather than trusting/requiring the client.
            foreach (var r in model?.T_AWR ?? new List<T_AWR>())
            {
                r.LOCA = CurrentLoca;
                // Store under the Unit that was picked on the list; UnitForAWRType (the fixed
                // AWR Type -> Unit table) is only the fallback when none was posted.
                r.UNIT = string.IsNullOrWhiteSpace(r.UNIT) ? UnitForAWRType(r.TRAN_TYPE) : r.UNIT;
            }

            var response = await Services.PostAsync<dynamic>("/api/TeaAWR/SaveOrUpdate", model);
            return Json(new
            {
                success = response.IsSuccessStatusCode,
                message = response.IsSuccessStatusCode ? "Saved successfully." : (response.Message ?? "Save failed."),
                data = response.Data
            });
        }

        // POST: AWREntry/Delete
        [HttpPost]
        public async Task<ActionResult> Delete(string docno, string awrDate, string awrType, string unit = "")
        {
            var sdsd = (List<AEDV>)Session["User_AEDV"];
            var permission = AEDV.ForScreen(sdsd, "AWREntry");
            if (!(permission?.Delete ?? false))
            {
                TempData["toastrError"] = "You do not have permission to delete this entry.";
                return RedirectToAction("Index", new { awrType });
            }

            // The row's Unit (Doc No repeats across units) must be one the user is linked to.
            if (!string.IsNullOrEmpty(unit) && !UnitScope.IsAllowed(unit, await GetUnitsForUserAsync()))
            {
                TempData["toastrError"] = $"You do not have permission for Unit {unit}.";
                return RedirectToAction("Index", new { awrType });
            }

            var response = await Services.PostAsync<dynamic>(
                $"/api/TeaAWR/Delete?docno={Uri.EscapeDataString(docno ?? "")}&awrDate={Uri.EscapeDataString(awrDate ?? "")}&awrType={Uri.EscapeDataString(awrType ?? "")}&unit={Uri.EscapeDataString(!string.IsNullOrEmpty(unit) ? unit : CurrentUnit)}", new { });

            TempData[response.IsSuccessStatusCode ? "toastrSuccess" : "toastrError"] =
                response.IsSuccessStatusCode ? "Deleted successfully." : (response.Message ?? "Delete failed.");

            return RedirectToAction("Index", new { awrType });
        }

        // =====================================================================
        // AJAX lookup passthroughs -- same JsonExact/JsonExactOrSessionExpired
        // rationale as MasterBlendEntryController (see its comments).
        // =====================================================================
        private ActionResult JsonExact(object data) =>
            Content(JsonConvert.SerializeObject(data), "application/json");

        // Only a real 401 from the API means the session actually expired -- ResponseApiModel.StatusCode
        // is the raw HttpStatusCode name (e.g. "Unauthorized", "InternalServerError"). Every other failure
        // (a genuine bug/exception in the lookup endpoint, a bad schema, etc.) used to be reported as
        // "session expired" too, which sent users off to re-login for problems that had nothing to do with
        // their session and hid the real error. Now only 401 gets that message; anything else surfaces the
        // API's actual message so it can be diagnosed.
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

        // Reshapes a plain-array lookup response into { data, count } for jquery.inputpicker
        // (Root UI convention -- see CLAUDE.md's "lookup pickers" rule). These lookups don't
        // support true server-side paging (the underlying API returns up to `limit` matches,
        // no rowCount of its own), so `count` is just what came back -- inputpicker always
        // shows a single page here. Same helper as PackingController.PickerJson.
        private ActionResult PickerJson(ResponseApiModel<dynamic> r)
        {
            var arr = (r?.IsSuccessStatusCode == true && r.Data != null)
                ? (Newtonsoft.Json.Linq.JArray)r.Data
                : new Newtonsoft.Json.Linq.JArray();
            return JsonExact(new { data = arr, count = arr.Count });
        }

        // inputpicker endpoints (Warehouse / Sales Centre) -- the plugin's own (q, limit, ..., p)
        // contract and { data, count } response. The API matches `q` against every column the
        // dropdown shows (Warehouse: Code, Name, Destination; Sales Centre: Code, Name).
        [HttpGet]
        public async Task<ActionResult> GetWarehouse(string q = "", int limit = 0, string fieldValue = "", string fieldText = "", string value = "", int p = 1)
        {
            if (string.IsNullOrEmpty(q)) q = value;
            var r = await Services.GetAsync<dynamic>($"/api/TeaAWR/GetWarehouse?search={Uri.EscapeDataString((q ?? "").Trim())}&pageSize={(limit > 0 ? limit : 50)}");
            return PickerJson(r);
        }

        [HttpGet]
        public async Task<ActionResult> GetSalesCentre(string q = "", int limit = 0, string fieldValue = "", string fieldText = "", string value = "", int p = 1)
        {
            if (string.IsNullOrEmpty(q)) q = value;
            var r = await Services.GetAsync<dynamic>($"/api/TeaAWR/GetSalesCentre?search={Uri.EscapeDataString((q ?? "").Trim())}&pageSize={(limit > 0 ? limit : 50)}");
            return PickerJson(r);
        }

        [HttpGet]
        public async Task<ActionResult> GetDespatchData(string awrType, string mode = "Purchase")
        {
            var r = await Services.GetAsync<dynamic>($"/api/TeaAWR/GetDespatchData?awrType={awrType}&mode={mode}");
            return JsonExactOrSessionExpired(r);
        }
    }

    // Small view-model for the list page (mirrors TeaAWRController.GetByPage's
    // anonymous document-summary shape) -- kept here rather than under
    // Models\PT since it's purely a list-page projection, not a saved entity.
    public class AWRDocRow
    {
        public string docno { get; set; }
        public string awrNo { get; set; }
        public DateTime awrDate { get; set; }
        public DateTime? arrivalDate { get; set; }
        public string warehouse { get; set; }
        public string warehouseName { get; set; }
        public string saleCentre { get; set; }
        public string saleCentreName { get; set; }
        public string unit { get; set; }
        public string tranType { get; set; }
        public decimal totalBagChest { get; set; }
        public decimal totalNetWt { get; set; }
        public decimal totalBagRcvd { get; set; }
    }
}
