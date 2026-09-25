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
    // "Tea Sample Draw Entry" -- ported from VB6 trn_sample_draw.frm, Packet
    // Tea (PT) transaction sub-type only. Modeled 1:1 on AWREntryController /
    // MasterBlendEntryController; list / AEDV / Unit scoping follows PackingController.
    public class TeaSampleDrawEntryController : Controller
    {
        public static readonly Dictionary<string, string> AWRTypes = AWREntryController.AWRTypes;

        private static readonly Dictionary<string, string> AWRTypeUnit = new Dictionary<string, string>
        {
            { "PT", "GORA" }, { "TT", "JSTI" }, { "WT", "JSTI" }, { "BT", "BGCH" }, { "ST", "TTSI" }, { "TB", "JSTI" },
        };

        private string CurrentUnit => SessionHelper.GetUser()?.CurentUnit ?? "";
        private string CurrentLoca => SessionHelper.GetUser()?.Loca ?? "";
        // Only the fallback when no Unit was posted / picked on the list (CurrentUnit is always
        // blank, so this is really the fixed Type -> Unit table).
        private string UnitForAWRType(string awrType) =>
            !string.IsNullOrEmpty(CurrentUnit) ? CurrentUnit :
            (awrType != null && AWRTypeUnit.TryGetValue(awrType, out var u) ? u : "");

        private async Task<Dictionary<string, string>> GetAllowedAWRTypesAsync()
        {
            var response = await Services.GetAsync<List<string>>("/api/TeaSampleDraw/GetAllowedAWRTypes");
            if (!response.IsSuccessStatusCode || response.Data == null)
                return AWRTypes;
            return AWRTypes.Where(t => response.Data.Contains(t.Key))
                            .ToDictionary(t => t.Key, t => t.Value);
        }

        // User.UnitList for the PacketTea module (USER_SCHEMA_LINK) -- same as
        // PackingController.GetUnitsForUserAsync. Backs the list's Unit scoping and the "New"
        // inline row's Unit picker.
        private async Task<List<UnitOption>> GetUnitsForUserAsync()
        {
            var response = await Services.GetAsync<List<UnitOption>>("/api/TeaSampleDraw/GetUnitsForUser");
            return (response.IsSuccessStatusCode ? response.Data : null) ?? new List<UnitOption>();
        }

        // GET: TeaSampleDrawEntry
        // Scoped to the Units the user is linked to (and to `unit` when given) and to every AWR
        // type the user has rights to -- see PackingController.Index. Sort (Doc No / Doc Date) is
        // server-side and travels with every chunk.
        public async Task<ActionResult> Index(string awrType, string searchString, int? page = 1, int pageSize = 15, string sortBy = "", string sortDir = "", string unit = "")
        {
            var sdsd = (List<AEDV>)Session["User_AEDV"];
            ViewBag.Permission = AEDV.ForScreen(sdsd, "TeaSampleDrawEntry");
            ViewBag.CurrentFilter = searchString;
            ViewBag.SortBy = sortBy;
            ViewBag.SortDir = sortDir;
            ViewBag.PageSize = pageSize;
            ViewBag.AWRType = awrType;
            // The two lookups are independent, so run them side by side.
            var allowedTask = GetAllowedAWRTypesAsync();
            var unitsTask = GetUnitsForUserAsync();
            await Task.WhenAll(allowedTask, unitsTask);
            var allowedTypes = allowedTask.Result;
            ViewBag.AWRTypes = allowedTypes;
            var userUnits = unitsTask.Result;
            ViewBag.UnitList = userUnits;
            // CurrentUnit is always blank, so it is NOT a filter: the list covers the one unit asked
            // for (if permitted) else every unit the user is linked to.
            var unitFilter = Uri.EscapeDataString(UnitScope.ListFilter(unit, userUnits));
            // Every permitted type goes to the API as ONE comma-separated list. A user with no type
            // rights gets the "matches nothing" marker rather than a blank (= unfiltered) list.
            var typeFilter = Uri.EscapeDataString(allowedTypes.Count > 0 ? string.Join(",", allowedTypes.Keys) : UnitScope.NoUnits);

            // Chunked list: a full page view always starts at the first chunk; further chunks
            // arrive as AJAX calls and return just the table rows (see Scripts/list-infinite-scroll.js).
            var isChunkRequest = Request.IsAjaxRequest();
            var pageNo = isChunkRequest ? Math.Max(page ?? 1, 1) : 1;
            ViewBag.Page = pageNo;
            ViewBag.RowOffset = (pageNo - 1) * pageSize;

            var response = await Services.GetAsync<PageModel<dynamic>>(
                $"/api/TeaSampleDraw/GetByPage?awrType={typeFilter}&unit={unitFilter}&search={Uri.EscapeDataString(searchString ?? "")}&page={pageNo}&pageSize={pageSize}&sortBy={Uri.EscapeDataString(sortBy ?? "")}&sortDir={Uri.EscapeDataString(sortDir ?? "")}");

            var json = response?.Data != null ? JsonConvert.SerializeObject(response.Data) : null;
            var wrapper = json != null ? JsonConvert.DeserializeObject<GetByPageResult>(json) : null;
            var list = wrapper?.value?.results ?? new List<SampleDrawDocRow>();
            ViewBag.RowCount = wrapper?.value?.rowCount ?? 0;

            if (isChunkRequest)
            {
                // A failed chunk must not leave a toast queued for the next full page load; an
                // empty response tells the list's scroll loader to stop.
                return PartialView("_ListRows", (response?.IsSuccessStatusCode ?? false) ? list : new List<SampleDrawDocRow>());
            }

            if (!response?.IsSuccessStatusCode ?? false)
            {
                TempData["toastrError"] = !string.IsNullOrEmpty(response?.Message)
                    ? response.Message
                    : $"Unable to load Tea Sample Draw list (API returned {response?.StatusCode}).";
            }

            return View(list);
        }

        private class GetByPageResult { public GetByPageValue value { get; set; } }
        private class GetByPageValue { public List<SampleDrawDocRow> results { get; set; } public int rowCount { get; set; } }

        // GET: TeaSampleDrawEntry/InsertOrUpdate
        // `unit` is the Unit picked on the list's "New" row (new entry) or the row's own Unit
        // (Edit / View -- a Doc No repeats across units). It must be one the user is linked to.
        public async Task<ActionResult> InsertOrUpdate(string docno = "", string docdt = "", string awrType = "", string unit = "", bool view = false)
        {
            var sdsd = (List<AEDV>)Session["User_AEDV"];
            var permission = AEDV.ForScreen(sdsd, "TeaSampleDrawEntry");
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

            bool isNewEntry = string.IsNullOrEmpty(docno);

            // The list's New button is dimmed without the Add right, but this URL is reachable
            // directly -- enforce it here too (rights are enforced server-side, not just by hiding
            // buttons).
            if (isNewEntry && !(permission?.Add ?? false))
            {
                TempData["toastrError"] = "You do not have permission to add a new entry.";
                return RedirectToAction("Index", new { awrType });
            }

            var allowedAWRTypes = await GetAllowedAWRTypesAsync();
            ViewBag.AWRTypes = allowedAWRTypes;
            ViewBag.LockedFromList = isNewEntry && !string.IsNullOrEmpty(unit) && !string.IsNullOrEmpty(awrType);

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

            // Doc Date picker's lower bound -- the later of the financial-year start and the AEDV
            // back-date allowance (Aday for a new entry, Eday while editing). Save() is the actual
            // enforcement point; this just steers the date picker.
            var backDateFloor = (permission ?? new AEDV()).MinDocDate(isNewEntry);
            var effectiveMin = fyStartDate.HasValue && fyStartDate.Value > backDateFloor ? fyStartDate.Value : backDateFloor;
            ViewBag.MinDocDate = effectiveMin.ToString("yyyy-MM-dd");

            if (isNewEntry)
            {
                var effectiveAWRType = !string.IsNullOrEmpty(awrType) ? awrType :
                    allowedAWRTypes.ContainsKey("PT") ? "PT" :
                    allowedAWRTypes.Keys.FirstOrDefault() ?? "PT";
                var model = new T_SAMPLE_DRAW_PT_DATA { T_SAMPLE_DRAW = new List<T_SAMPLE_DRAW>() };
                ViewBag.IsEdit = false;
                // Honor the Unit explicitly chosen on the list page over the AWRTypeUnit guess.
                ViewBag.NewUnit = !string.IsNullOrEmpty(unit) ? unit : UnitForAWRType(effectiveAWRType);
                ViewBag.NewAWRType = effectiveAWRType;
                return View(model);
            }

            // Load the document and the user's units side by side; the unit check runs on both the
            // Unit the link carried and the loaded record's own Unit.
            var unitsTask = GetUnitsForUserAsync();
            var response = await Services.GetAsync<dynamic>(
                $"/api/TeaSampleDraw/GetByDocNo?docno={Uri.EscapeDataString(docno)}&docdt={Uri.EscapeDataString(docdt ?? "")}&awrType={Uri.EscapeDataString(awrType ?? "")}&unit={Uri.EscapeDataString(!string.IsNullOrEmpty(unit) ? unit : CurrentUnit)}");
            var userUnits = await unitsTask;

            if (!string.IsNullOrEmpty(unit) && !UnitScope.IsAllowed(unit, userUnits))
            {
                TempData["toastrError"] = $"You do not have permission for Unit {unit}.";
                return RedirectToAction("Index", new { awrType });
            }

            if (!response.IsSuccessStatusCode || response.Data == null)
            {
                TempData["toastrError"] = response.Message ?? "Record not found.";
                return RedirectToAction("Index", new { awrType });
            }

            var json = JsonConvert.SerializeObject(response.Data);
            var wrapper = JsonConvert.DeserializeObject<GetByDocNoResult>(json);

            var editModel = new T_SAMPLE_DRAW_PT_DATA { T_SAMPLE_DRAW = wrapper.rows ?? new List<T_SAMPLE_DRAW>() };
            var head = editModel.T_SAMPLE_DRAW.FirstOrDefault();

            var recordUnit = head?.UNIT;
            if (!UnitScope.IsAllowed(recordUnit, userUnits))
            {
                TempData["toastrError"] = $"You do not have permission for Unit {recordUnit}.";
                return RedirectToAction("Index", new { awrType });
            }

            // Block opening Edit outright when the user has no Edit right, or the record's own Doc
            // Date has fallen outside the Eday back-date window -- View bypasses this (read-only
            // regardless of the Edit/back-date policy).
            if (!view)
            {
                var editErr = AEDV.CheckAddEdit(permission, false, head?.DOCDT ?? DateTime.Today);
                if (editErr != null)
                {
                    TempData["toastrError"] = editErr;
                    return RedirectToAction("Index", new { awrType });
                }
            }

            ViewBag.IsEdit = true;
            ViewBag.NewUnit = head?.UNIT;
            ViewBag.NewAWRType = head?.TRAN_CODE ?? awrType;
            return View(editModel);
        }

        private class GetByDocNoResult { public List<T_SAMPLE_DRAW> rows { get; set; } }

        // GET: TeaSampleDrawEntry/GetRowDetail (AJAX, fired by the Index list's "+" toggle / Lines)
        // -- the API's lean GetRowLines (lines + Mark/Grade names only). The row's own Unit is
        // passed along, since a Doc No repeats across units.
        [HttpGet]
        public async Task<ActionResult> GetRowDetail(string docno = "", string docdt = "", string awrType = "", string unit = "")
        {
            var r = await Services.GetAsync<dynamic>(
                $"/api/TeaSampleDraw/GetRowLines?docno={Uri.EscapeDataString(docno ?? "")}&docdt={Uri.EscapeDataString(docdt ?? "")}&awrType={Uri.EscapeDataString(awrType ?? "")}&unit={Uri.EscapeDataString(unit ?? "")}");
            if (!r.IsSuccessStatusCode || r.Data == null)
                return JsonExact(new { success = false, message = r.Message ?? "Record not found." });

            var obj = (Newtonsoft.Json.Linq.JObject)r.Data;
            obj["success"] = true;
            return JsonExact(obj);
        }

        // POST: TeaSampleDrawEntry/Save
        [HttpPost]
        public async Task<JsonResult> Save(T_SAMPLE_DRAW_PT_DATA model)
        {
            var rows = model?.T_SAMPLE_DRAW ?? new List<T_SAMPLE_DRAW>();
            var head = rows.FirstOrDefault();

            // Server-side AEDV Add/Edit + back-date enforcement -- the date picker's `min` (see
            // InsertOrUpdate) only steers well-behaved clients; this is the actual gate.
            var sdsd = (List<AEDV>)Session["User_AEDV"];
            var permission = AEDV.ForScreen(sdsd, "TeaSampleDrawEntry");
            bool isNew = model?.IsNew ?? string.IsNullOrEmpty(head?.DOCNO);
            var permErr = AEDV.CheckAddEdit(permission, isNew, head?.DOCDT ?? DateTime.Today);
            if (permErr != null)
                return Json(new { success = false, message = permErr });

            // The document is stored under the Unit picked on the list (posted from the entry
            // screen as UNIT); it must be one the user is linked to -- for a NEW record, and for an
            // edit too (InsertOrUpdate already refuses to open a record of a foreign Unit; this
            // stops a hand-built post from re-stamping one). The AWRTypeUnit guess is only the
            // fallback when no Unit was posted.
            var unit = !string.IsNullOrWhiteSpace(head?.UNIT) ? head.UNIT.Trim() : UnitForAWRType(head?.TRAN_CODE);
            if (!string.IsNullOrWhiteSpace(unit) && !UnitScope.IsAllowed(unit, await GetUnitsForUserAsync()))
                return Json(new { success = false, message = $"You do not have permission for Unit {unit}." });

            // Same fix as AWREntryController.Save -- LOCA is never posted by the client, so
            // stamp it from the session here rather than trusting/requiring the client.
            foreach (var r in rows)
            {
                r.LOCA = CurrentLoca;
                r.UNIT = unit;
            }

            var response = await Services.PostAsync<dynamic>("/api/TeaSampleDraw/SaveOrUpdate", model);
            return Json(new
            {
                success = response.IsSuccessStatusCode,
                message = response.IsSuccessStatusCode ? "Saved successfully." : (response.Message ?? "Save failed."),
                data = response.Data
            });
        }

        // POST: TeaSampleDrawEntry/Delete
        [HttpPost]
        public async Task<ActionResult> Delete(string docno, string docdt, string awrType, string unit = "")
        {
            var sdsd = (List<AEDV>)Session["User_AEDV"];
            var permission = AEDV.ForScreen(sdsd, "TeaSampleDrawEntry");
            if (!(permission?.Delete ?? false))
            {
                TempData["toastrError"] = "You do not have permission to delete this entry.";
                return RedirectToAction("Index", new { awrType });
            }

            // The row's Unit (a Doc No repeats across units) must be one the user is linked to.
            if (!UnitScope.IsAllowed(unit, await GetUnitsForUserAsync()))
            {
                TempData["toastrError"] = string.IsNullOrWhiteSpace(unit)
                    ? "Unit is required to delete an entry."
                    : $"You do not have permission for Unit {unit}.";
                return RedirectToAction("Index", new { awrType });
            }

            var response = await Services.PostAsync<dynamic>(
                $"/api/TeaSampleDraw/Delete?docno={Uri.EscapeDataString(docno ?? "")}&docdt={Uri.EscapeDataString(docdt ?? "")}&awrType={Uri.EscapeDataString(awrType ?? "")}&unit={Uri.EscapeDataString(unit.Trim())}", new { });

            TempData[response.IsSuccessStatusCode ? "toastrSuccess" : "toastrError"] =
                response.IsSuccessStatusCode ? "Deleted successfully." : (response.Message ?? "Delete failed.");

            return RedirectToAction("Index", new { awrType });
        }

        // =====================================================================
        // AJAX lookup passthroughs
        // =====================================================================
        private ActionResult JsonExact(object data) =>
            Content(JsonConvert.SerializeObject(data), "application/json");

        // Same fix as AWREntryController.JsonExactOrSessionExpired -- only a real 401 from the
        // API means the session actually expired. Every other failure used to be mislabeled
        // "session expired" too, hiding the real error.
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
        // (Root UI convention -- see CLAUDE.md's "lookup pickers" rule); same helper as
        // PackingController.PickerJson. These lookups don't support true server-side paging (the
        // API returns up to `limit` matches, no rowCount of its own), so `count` is just what came
        // back and inputpicker always shows a single page.
        private ActionResult PickerJson(ResponseApiModel<dynamic> r)
        {
            var arr = (r?.IsSuccessStatusCode == true && r.Data != null)
                ? (Newtonsoft.Json.Linq.JArray)r.Data
                : new Newtonsoft.Json.Linq.JArray();
            return JsonExact(new { data = arr, count = arr.Count });
        }

        // The four header pickers below are inputpicker endpoints: (q, limit, ..., p) in,
        // { data, count } out. The API matches `q` case-insensitively on every column shown
        // (Code + Name; Warehouse also Destination).
        [HttpGet]
        public async Task<ActionResult> GetSalesCentre(string q = "", int limit = 0, string fieldValue = "", string fieldText = "", string value = "", int p = 1)
        {
            if (string.IsNullOrEmpty(q)) q = value;
            var r = await Services.GetAsync<dynamic>($"/api/TeaSampleDraw/GetSalesCentre?search={Uri.EscapeDataString((q ?? "").Trim())}&pageSize={(limit > 0 ? limit : 50)}");
            return PickerJson(r);
        }

        [HttpGet]
        public async Task<ActionResult> GetBroker(string q = "", int limit = 0, string fieldValue = "", string fieldText = "", string value = "", int p = 1)
        {
            if (string.IsNullOrEmpty(q)) q = value;
            var r = await Services.GetAsync<dynamic>($"/api/TeaSampleDraw/GetBroker?search={Uri.EscapeDataString((q ?? "").Trim())}&pageSize={(limit > 0 ? limit : 50)}");
            return PickerJson(r);
        }

        [HttpGet]
        public async Task<ActionResult> GetParty(string q = "", int limit = 0, string fieldValue = "", string fieldText = "", string value = "", int p = 1)
        {
            if (string.IsNullOrEmpty(q)) q = value;
            var r = await Services.GetAsync<dynamic>($"/api/TeaSampleDraw/GetParty?search={Uri.EscapeDataString((q ?? "").Trim())}&pageSize={(limit > 0 ? limit : 50)}");
            return PickerJson(r);
        }

        [HttpGet]
        public async Task<ActionResult> GetWarehouse(string q = "", int limit = 0, string fieldValue = "", string fieldText = "", string value = "", int p = 1)
        {
            if (string.IsNullOrEmpty(q)) q = value;
            var r = await Services.GetAsync<dynamic>($"/api/TeaSampleDraw/GetWarehouse?search={Uri.EscapeDataString((q ?? "").Trim())}&pageSize={(limit > 0 ? limit : 50)}");
            return PickerJson(r);
        }

        [HttpGet]
        public async Task<ActionResult> GetAvailableAWRStock(string awrType, string mark = "", string grade = "")
        {
            var r = await Services.GetAsync<dynamic>($"/api/TeaSampleDraw/GetAvailableAWRStock?awrType={awrType}&mark={mark}&grade={grade}");
            return JsonExactOrSessionExpired(r);
        }

        [HttpGet]
        public async Task<ActionResult> GetDrawQty(string saleCentre, decimal bagChest, string grade, string docDate)
        {
            var r = await Services.GetAsync<dynamic>(
                $"/api/TeaSampleDraw/GetDrawQty?saleCentre={saleCentre}&bagChest={bagChest}&grade={grade}&docDate={docDate}");
            return JsonExactOrSessionExpired(r);
        }
    }

    public class SampleDrawDocRow
    {
        public string docno { get; set; }
        public DateTime? docdt { get; set; }
        public string saleCentre { get; set; }
        public string saleCentreName { get; set; }
        public string brokCode { get; set; }
        public string brokerName { get; set; }
        public string warehouse { get; set; }
        public string unit { get; set; }
        public string tranCode { get; set; }
        public decimal totalBagChest { get; set; }
        public decimal totalDrawQty { get; set; }
    }
}
