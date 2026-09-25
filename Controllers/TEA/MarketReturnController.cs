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
            ViewBag.Permission = AEDV.ForScreen(sdsd, "MarketReturn");
            ViewBag.CurrentFilter = searchString;
            ViewBag.SortBy = sortBy;
            ViewBag.SortDir = sortDir;
            ViewBag.PageSize = pageSize;
            ViewBag.Type = type;
            ViewBag.Unit = unit;
            // Allowed Types and the Units the user is linked to -- independent lookups, run side by side.
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
                $"/api/MarketReturn/GetByPage?type={Uri.EscapeDataString(type ?? "")}&unit={unitFilter}&search={Uri.EscapeDataString(searchString ?? "")}&page={pageNo}&pageSize={pageSize}&sortBy={Uri.EscapeDataString(sortBy ?? "")}&sortDir={Uri.EscapeDataString(sortDir ?? "")}");

            var json = response?.Data != null ? JsonConvert.SerializeObject(response.Data) : null;
            var wrapper = json != null ? JsonConvert.DeserializeObject<GetByPageResult>(json) : null;
            var list = wrapper?.value?.results ?? new List<MretuDocRow>();
            ViewBag.RowCount = wrapper?.value?.rowCount ?? 0;

            if (isChunkRequest)
            {
                // A failed chunk must not leave a toast queued for the next full page load; an
                // empty response tells the list's scroll loader to stop.
                return PartialView("_ListRows", (response?.IsSuccessStatusCode ?? false) ? list : new List<MretuDocRow>());
            }

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
            var sdsd = (List<AEDV>)Session["User_AEDV"];
            var permission = AEDV.ForScreen(sdsd, "MarketReturn");
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

            var allowedTypes = await GetAllowedTypesAsync();
            ViewBag.Types = allowedTypes;
            ViewBag.LockedFromList = string.IsNullOrEmpty(docno) && !string.IsNullOrEmpty(unit);

            bool isNewEntry = string.IsNullOrEmpty(docno);

            // The list page's New button already looks disabled without the Add right, but this
            // URL is reachable directly -- block it here too (enforce rights server-side).
            if (isNewEntry && !(permission?.Add ?? false))
            {
                TempData["toastrError"] = "You do not have permission to add a new entry.";
                return RedirectToAction("Index", new { type });
            }

            // Doc Date picker's lower bound -- the later of the financial-year start and the AEDV
            // back-date allowance (Aday for a new entry, Eday while editing). Save() is the actual
            // enforcement point; this just steers the date picker.
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
            var backDateFloor = (permission ?? new AEDV()).MinDocDate(isNewEntry);
            var effectiveMin = fyStartDate.HasValue && fyStartDate.Value > backDateFloor ? fyStartDate.Value : backDateFloor;
            ViewBag.MinDocDate = effectiveMin.ToString("yyyy-MM-dd");

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

            // The record must belong to a Unit the user is linked to -- checked alongside the load,
            // against the row's own Unit (or the one the list passed).
            var unitsTask = GetUnitsForUserAsync();
            var response = await Services.SalesGetAsync<dynamic>($"/api/MarketReturn/GetByDocNo?docno={Uri.EscapeDataString(docno)}&unit={Uri.EscapeDataString(unit ?? "")}");
            var userUnits = await unitsTask;
            if (!response.IsSuccessStatusCode || response.Data == null)
            {
                TempData["toastrError"] = response.Message ?? "Record not found.";
                return RedirectToAction("Index", new { type });
            }

            var json = JsonConvert.SerializeObject(response.Data);
            var wrapper = JsonConvert.DeserializeObject<GetByDocNoResult>(json);

            var editModel = new T_MRETU_DATA
            {
                T_MRETU_HED = wrapper.head ?? new T_MRETU_HED(),
                T_MRETU_DET = wrapper.details ?? new List<T_MRETU>()
            };
            var recordUnit = !string.IsNullOrEmpty(editModel.T_MRETU_HED.UNIT) ? editModel.T_MRETU_HED.UNIT : unit;
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
                var editErr = AEDV.CheckAddEdit(permission, false, editModel.T_MRETU_HED.DATE_ORA ?? DateTime.Today);
                if (editErr != null)
                {
                    TempData["toastrError"] = editErr;
                    return RedirectToAction("Index", new { type });
                }
            }

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
            // Server-side AEDV Add/Edit + back-date enforcement -- the date picker's `min`
            // (see InsertOrUpdate) only steers well-behaved clients; this is the actual gate.
            var sdsd = (List<AEDV>)Session["User_AEDV"];
            var permission = AEDV.ForScreen(sdsd, "MarketReturn");
            bool isNew = model?.IsNew ?? string.IsNullOrEmpty(model?.T_MRETU_HED?.DOCNO);
            var permErr = AEDV.CheckAddEdit(permission, isNew, model?.T_MRETU_HED?.DATE_ORA ?? DateTime.Today);
            if (permErr != null)
                return Json(new { success = false, message = permErr });

            // The Unit posted from the entry screen (the one picked on the list for a NEW record --
            // or the picked bill's own Unit, see popLines -- and the record's own for an edit) must
            // be one the user is linked to.
            var postedUnit = (model?.T_MRETU_HED?.UNIT ?? "").Trim();
            if (postedUnit.Length > 0 && !UnitScope.IsAllowed(postedUnit, await GetUnitsForUserAsync()))
                return Json(new { success = false, message = $"You do not have permission for Unit {postedUnit}." });

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
            var sdsd = (List<AEDV>)Session["User_AEDV"];
            var permission = AEDV.ForScreen(sdsd, "MarketReturn");
            if (!(permission?.Delete ?? false))
            {
                TempData["toastrError"] = "You do not have permission to delete this entry.";
                return RedirectToAction("Index", new { type });
            }

            // The row's Unit (Doc No repeats across units) must be one the user is linked to.
            if (!UnitScope.IsAllowed(unit, await GetUnitsForUserAsync()))
            {
                TempData["toastrError"] = $"You do not have permission for Unit {unit}.";
                return RedirectToAction("Index", new { type });
            }

            var response = await Services.SalesPostAsync<dynamic>($"/api/MarketReturn/Delete?docno={Uri.EscapeDataString(docno ?? "")}&unit={Uri.EscapeDataString(unit ?? "")}", new { });

            TempData[response.IsSuccessStatusCode ? "toastrSuccess" : "toastrError"] =
                response.IsSuccessStatusCode ? "Deleted successfully." : (response.Message ?? "Delete failed.");

            return RedirectToAction("Index", new { type });
        }

        // GET: MarketReturn/GetRowDetail (AJAX, fired by the Index list's "+" toggle)
        // Uses the API's lean GetRowLines ({ details: [...] } only -- no head read / whole-master
        // reads); the row's own Unit is passed (a Doc No repeats across units).
        [HttpGet]
        public async Task<ActionResult> GetRowDetail(string docno = "", string unit = "")
        {
            var r = await Services.SalesGetAsync<dynamic>($"/api/MarketReturn/GetRowLines?docno={Uri.EscapeDataString(docno ?? "")}&unit={Uri.EscapeDataString(unit ?? "")}");
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

        // inputpicker endpoints (Party / Transporter / Item) -- the plugin's own (q, limit, ..., p)
        // contract and { data, count } response. The API matches `q` against every column the
        // dropdown shows (Code and Name).
        [HttpGet]
        public async Task<ActionResult> GetParty(string q = "", int limit = 0, string fieldValue = "", string fieldText = "", string value = "", int p = 1)
        {
            if (string.IsNullOrEmpty(q)) q = value;
            var r = await Services.SalesGetAsync<dynamic>($"/api/MarketReturn/GetParty?search={Uri.EscapeDataString((q ?? "").Trim())}&pageSize={(limit > 0 ? limit : 50)}");
            return PickerJson(r);
        }

        [HttpGet]
        public async Task<ActionResult> GetTransporter(string q = "", int limit = 0, string fieldValue = "", string fieldText = "", string value = "", int p = 1)
        {
            if (string.IsNullOrEmpty(q)) q = value;
            var r = await Services.SalesGetAsync<dynamic>($"/api/MarketReturn/GetTransporter?search={Uri.EscapeDataString((q ?? "").Trim())}&pageSize={(limit > 0 ? limit : 50)}");
            return PickerJson(r);
        }

        [HttpGet]
        public async Task<ActionResult> GetItemMaster(string q = "", int limit = 0, string fieldValue = "", string fieldText = "", string value = "", int p = 1)
        {
            if (string.IsNullOrEmpty(q)) q = value;
            var r = await Services.SalesGetAsync<dynamic>($"/api/MarketReturn/GetItemMaster?search={Uri.EscapeDataString((q ?? "").Trim())}&pageSize={(limit > 0 ? limit : 50)}");
            return PickerJson(r);
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
