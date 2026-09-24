using Newtonsoft.Json;
using PacketTea;
using PacketTea.Models;
using PacketTea.Models.PT;
using PacketTea.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static PacketTea.Helpers;

namespace Finance.Controllers.TEA
{
    // "Packing Entry from Blend Sheet" -- ported from the VB6 trn_blend_Packing.frm
    // form (MDI menu item 21: ENTRYPRDBLD_Click). Backed by ClassicERPCoreAPI's
    // BlendPackingController -- see that controller's header comment for the full
    // scope notes (Finance-schema mirroring deferred, Return Tea mode out of
    // scope, the per-row "Inv No" picker fix).
    public class PackingController : Controller
    {
        public static readonly Dictionary<string, string> BlendTypes = MasterBlendEntryController.BlendTypes;

        private static readonly Dictionary<string, string> BlendTypeUnit = new Dictionary<string, string>
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
        private string UnitForBlendType(string blendType) =>
            !string.IsNullOrEmpty(CurrentUnit) ? CurrentUnit :
            (blendType != null && BlendTypeUnit.TryGetValue(blendType, out var u) ? u : "");

        // User.UnitList for the PacketTea module (see JwtMiddleware.cs) -- backs the list
        // page's Unit picker, same as MasterBlendEntryController/FinalBlendEntryController
        // (reuses the same generic, module-scoped API endpoint rather than duplicating it).
        private async Task<List<UnitOption>> GetUnitsForUserAsync()
        {
            var response = await Services.GetAsync<List<UnitOption>>("/api/TeaBlend/GetUnitsForUser");
            return (response.IsSuccessStatusCode ? response.Data : null) ?? new List<UnitOption>();
        }

        // GET: Packing
        // Scoped to the Units the user is linked to (and to `unit` / `blendType` when given) --
        // see MasterBlendEntryController.Index.
        public async Task<ActionResult> Index(string blendType, string searchString, int? page = 1, int pageSize = 15, string unit = "")
        {
            var sdsd = (List<AEDV>)Session["User_AEDV"];
            ViewBag.Permission = AEDV.ForScreen(sdsd, "Packing");
            ViewBag.CurrentFilter = searchString;
            ViewBag.PageSize = pageSize;
            ViewBag.Page = page ?? 1;
            ViewBag.BlendType = blendType;
            // Packing Types the user has rights to (M_UNIT_USER_RIGHT) -- same as Master Blend
            // Entry; the two lookups are independent, so run them side by side.
            var allowedTask = MasterBlendEntryController.GetAllowedBlendTypesAsync();
            var unitsTask = GetUnitsForUserAsync();
            await Task.WhenAll(allowedTask, unitsTask);
            var allowedTypes = allowedTask.Result;
            ViewBag.BlendTypes = allowedTypes;
            var userUnits = unitsTask.Result;
            ViewBag.UnitList = userUnits;
            var unitFilter = Uri.EscapeDataString(UnitScope.ListFilter(unit, userUnits));
            // The list covers every permitted type as ONE comma-separated list, not the single
            // `blendType` in the URL (see MasterBlendEntryController.Index).
            var typeFilter = Uri.EscapeDataString(string.Join(",", allowedTypes.Keys));

            // Chunked list: a full page view always starts at the first chunk; further chunks
            // arrive as AJAX calls (see below) and return just the table rows.
            var isChunkRequest = Request.IsAjaxRequest();
            var pageNo = isChunkRequest ? Math.Max(page ?? 1, 1) : 1;
            ViewBag.Page = pageNo;
            ViewBag.RowOffset = (pageNo - 1) * pageSize;

            var response = await Services.GetAsync<PageModel<BLEND_PACKING_DATA>>(
                $"/api/BlendPacking/GetByPage?blendType={typeFilter}&unit={unitFilter}&search={searchString}&page={pageNo}&pageSize={pageSize}");

            var list = response?.Data?.value?.results ?? new List<BLEND_PACKING_DATA>();
            ViewBag.RowCount = response?.Data?.value?.rowCount ?? 0;

            if (isChunkRequest)
            {
                // A failed chunk must not leave a toast queued for the next full page load; an
                // empty response tells the list's scroll loader to stop.
                return PartialView("_ListRows", (response?.IsSuccessStatusCode ?? false) ? list : new List<BLEND_PACKING_DATA>());
            }

            if (!response?.IsSuccessStatusCode ?? false)
            {
                TempData["toastrError"] = !string.IsNullOrEmpty(response?.Message)
                    ? response.Message
                    : $"Unable to load Packing list (API returned {response?.StatusCode}).";
            }

            return View(list);
        }

        // GET: Packing/InsertOrUpdate
        // `unit` is only ever populated when this was opened from the list page's Unit
        // picker -- Unit + Blend Type were already chosen there, so the header repeats
        // them read-only instead of leaving Unit unset (same pattern as
        // MasterBlendEntryController/FinalBlendEntryController.InsertOrUpdate).
        public async Task<ActionResult> InsertOrUpdate(string docno = "", string docdt = "", string blendType = "", string unit = "", bool view = false)
        {
            var sdsd = (List<AEDV>)Session["User_AEDV"];
            var permission = AEDV.ForScreen(sdsd, "Packing");
            ViewBag.Permission = permission;

            // `view` is set by the list's View button -- same fetch as Edit, but the form renders
            // read-only. It only applies to an existing record and needs the View right.
            view = view && !string.IsNullOrEmpty(docno);
            if (view && !(permission?.View ?? false))
            {
                TempData["toastrError"] = "You do not have permission to view this entry.";
                return RedirectToAction("Index", new { blendType });
            }
            ViewBag.IsView = view;

            ViewBag.BlendTypes = BlendTypes;
            ViewBag.LockedFromList = string.IsNullOrEmpty(docno) && !string.IsNullOrEmpty(unit) && !string.IsNullOrEmpty(blendType);

            bool isNewEntry = string.IsNullOrEmpty(docno);

            // The list page's New/Unit picker already hides New when the user has no Add
            // right, but this URL is reachable directly -- block it here too (item 1 of the
            // AEDV spec: enforce rights server-side, not just by hiding buttons).
            if (isNewEntry && !(permission?.Add ?? false))
            {
                TempData["toastrError"] = "You do not have permission to add a new entry.";
                return RedirectToAction("Index", new { blendType });
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

            // Doc Date picker's lower bound -- the later of the financial-year start and the
            // AEDV back-date allowance (Aday for a new entry, Eday while editing). Save() is
            // the actual enforcement point; this just steers the date picker so most users
            // never hit the server-side rejection.
            var backDateFloor = (permission ?? new AEDV()).MinDocDate(isNewEntry);
            var effectiveMin = fyStartDate.HasValue && fyStartDate.Value > backDateFloor ? fyStartDate.Value : backDateFloor;
            ViewBag.MinDocDate = effectiveMin.ToString("yyyy-MM-dd");

            if (string.IsNullOrEmpty(docno))
            {
                var effectiveBlendType = string.IsNullOrEmpty(blendType) ? "PT" : blendType;
                var model = new BLEND_PACKING_DATA
                {
                    LOCA = CurrentLoca,
                    GLOCA = CurrentLoca,
                    // Honor the Unit explicitly chosen on the list page over the
                    // BlendTypeUnit stopgap guess -- see UnitForBlendType's comment.
                    UNIT = !string.IsNullOrEmpty(unit) ? unit : UnitForBlendType(effectiveBlendType),
                    BLEND_TYPE = effectiveBlendType,
                    DOCDT = DateTime.Today,
                    Details = new List<T_BLEND_PACKING>()
                };
                ViewBag.IsEdit = false;
                return View(model);
            }

            // The list's Blend No link / Edit button passes the row's Unit (a Doc No repeats across
            // units). It must be one the user is linked to -- checked alongside the load.
            var unitsTask = string.IsNullOrEmpty(unit) ? null : GetUnitsForUserAsync();
            var response = await Services.GetAsync<dynamic>(
                $"/api/BlendPacking/GetByDocNo?docno={Uri.EscapeDataString(docno)}&docdt={Uri.EscapeDataString(docdt ?? "")}&blendType={Uri.EscapeDataString(blendType ?? "")}&unit={Uri.EscapeDataString(!string.IsNullOrEmpty(unit) ? unit : CurrentUnit)}");
            if (unitsTask != null && !UnitScope.IsAllowed(unit, await unitsTask))
            {
                TempData["toastrError"] = $"You do not have permission for Unit {unit}.";
                return RedirectToAction("Index", new { blendType });
            }

            if (!response.IsSuccessStatusCode || response.Data == null)
            {
                TempData["toastrError"] = response.Message ?? "Record not found.";
                return RedirectToAction("Index", new { blendType });
            }

            var json = JsonConvert.SerializeObject(response.Data);
            var wrapper = JsonConvert.DeserializeObject<GetByDocNoResult>(json);

            // Block opening Edit outright when the user has no Edit right, or the record's
            // own Doc Date has fallen outside the Eday back-date window -- View bypasses this
            // (read-only regardless of the Edit/back-date policy).
            if (!view)
            {
                var editErr = AEDV.CheckAddEdit(permission, false, wrapper.head?.DOCDT ?? DateTime.Today);
                if (editErr != null)
                {
                    TempData["toastrError"] = editErr;
                    return RedirectToAction("Index", new { blendType });
                }
            }

            var editModel = wrapper.head ?? new BLEND_PACKING_DATA();
            editModel.BLEND_TYPE = blendType;
            editModel.Details = wrapper.details ?? new List<T_BLEND_PACKING>();
            ViewBag.IsEdit = true;
            return View(editModel);
        }

        private class GetByDocNoResult
        {
            public BLEND_PACKING_DATA head { get; set; }
            public List<T_BLEND_PACKING> details { get; set; }
        }

        // POST: Packing/Save (AJAX, body = BLEND_PACKING_DATA as JSON)
        [HttpPost]
        public async Task<JsonResult> Save(BLEND_PACKING_DATA model)
        {
            // Server-side AEDV Add/Edit + back-date enforcement -- the date picker's `min`
            // (see InsertOrUpdate) only steers well-behaved clients; this is the actual gate.
            var sdsd = (List<AEDV>)Session["User_AEDV"];
            var permission = AEDV.ForScreen(sdsd, "Packing");
            bool isNew = model?.IsNew ?? string.IsNullOrEmpty(model?.DOCNO);
            var permErr = AEDV.CheckAddEdit(permission, isNew, model?.DOCDT ?? DateTime.Today);
            if (permErr != null)
                return Json(new { success = false, message = permErr });

            // A NEW packing doc is stamped with the Unit picked on the list (posted from the
            // entry screen), which must be one the user is linked to -- see
            // MasterBlendEntryController.Save.
            if (isNew && !string.IsNullOrWhiteSpace(model?.UNIT)
                && !UnitScope.IsAllowed(model.UNIT, await GetUnitsForUserAsync()))
                return Json(new { success = false, message = $"You do not have permission for Unit {model.UNIT}." });

            if (model != null)
            {
                model.LOCA = string.IsNullOrEmpty(model.LOCA) ? CurrentLoca : model.LOCA;
                model.GLOCA = string.IsNullOrEmpty(model.GLOCA) ? CurrentLoca : model.GLOCA;
                model.UNIT = string.IsNullOrEmpty(model.UNIT) ? UnitForBlendType(model.BLEND_TYPE) : model.UNIT;
            }

            var response = await Services.PostAsync<dynamic>("/api/BlendPacking/SaveOrUpdate", model);
            return Json(new
            {
                success = response.IsSuccessStatusCode,
                message = response.IsSuccessStatusCode ? "Saved successfully." : (response.Message ?? "Save failed."),
                data = response.Data
            });
        }

        // POST: Packing/Delete
        [HttpPost]
        public async Task<ActionResult> Delete(string docno, string docdt, string blendType, string unit = "")
        {
            var sdsd = (List<AEDV>)Session["User_AEDV"];
            var permission = AEDV.ForScreen(sdsd, "Packing");
            if (!(permission?.Delete ?? false))
            {
                TempData["toastrError"] = "You do not have permission to delete this entry.";
                return RedirectToAction("Index", new { blendType });
            }

            // The row's Unit (Doc No repeats across units) must be one the user is linked to.
            if (!string.IsNullOrEmpty(unit) && !UnitScope.IsAllowed(unit, await GetUnitsForUserAsync()))
            {
                TempData["toastrError"] = $"You do not have permission for Unit {unit}.";
                return RedirectToAction("Index", new { blendType });
            }

            var response = await Services.PostAsync<dynamic>(
                $"/api/BlendPacking/Delete?docno={docno}&docdt={docdt}&blendType={blendType}&unit={Uri.EscapeDataString(!string.IsNullOrEmpty(unit) ? unit : CurrentUnit)}", new { });

            TempData[response.IsSuccessStatusCode ? "toastrSuccess" : "toastrError"] =
                response.IsSuccessStatusCode ? "Deleted successfully." : (response.Message ?? "Delete failed.");

            return RedirectToAction("Index", new { blendType });
        }

        // =====================================================================
        // AJAX lookup passthroughs. Mark/Grade/Category/Allocation/Transporter
        // are reused from TeaBlendController's generic master-data endpoints
        // (same masters, no Packing-specific filtering) -- only Sales Centre,
        // Chest Size and the Final Blend picker/row-values are Packing-specific.
        // =====================================================================
        private ActionResult JsonExact(object data) =>
            Content(JsonConvert.SerializeObject(data), "application/json");

        // Reshapes a plain-array lookup response into { data, count } for jquery.inputpicker
        // (Root UI convention -- see CLAUDE.md's "lookup pickers" rule). These lookups don't
        // support true server-side paging (the underlying API returns up to `limit` matches,
        // no rowCount of its own), so `count` is just what came back -- inputpicker always
        // shows a single page here, same as the plain top-N list this replaced.
        private ActionResult PickerJson(ResponseApiModel<dynamic> r)
        {
            var arr = (r?.IsSuccessStatusCode == true && r.Data != null)
                ? (Newtonsoft.Json.Linq.JArray)r.Data
                : new Newtonsoft.Json.Linq.JArray();
            return JsonExact(new { data = arr, count = arr.Count });
        }

        [HttpGet]
        public async Task<ActionResult> GetMark(string q = "", int limit = 0, string fieldValue = "", string fieldText = "", string value = "", int p = 1)
        {
            if (string.IsNullOrEmpty(q)) q = value;
            var r = await Services.GetAsync<dynamic>($"/api/TeaBlend/GetMark?search={q}&pageSize={(limit > 0 ? limit : 50)}");
            return PickerJson(r);
        }

        [HttpGet]
        public async Task<ActionResult> GetBlendGrade(string q = "", int limit = 0, string fieldValue = "", string fieldText = "", string value = "", int p = 1)
        {
            if (string.IsNullOrEmpty(q)) q = value;
            var r = await Services.GetAsync<dynamic>($"/api/TeaBlend/GetBlendGrade?search={q}&pageSize={(limit > 0 ? limit : 50)}");
            return PickerJson(r);
        }

        [HttpGet]
        public async Task<ActionResult> GetAllocation(string q = "", int limit = 0, string fieldValue = "", string fieldText = "", string value = "", int p = 1)
        {
            if (string.IsNullOrEmpty(q)) q = value;
            var r = await Services.GetAsync<dynamic>($"/api/TeaBlend/GetAllocation?search={q}&pageSize={(limit > 0 ? limit : 50)}");
            return PickerJson(r);
        }

        [HttpGet]
        public async Task<ActionResult> GetSalesCentre(string q = "", int limit = 0, string fieldValue = "", string fieldText = "", string value = "", int p = 1)
        {
            if (string.IsNullOrEmpty(q)) q = value;
            var r = await Services.GetAsync<dynamic>($"/api/BlendPacking/GetSalesCentre?search={q}&pageSize={(limit > 0 ? limit : 50)}");
            return PickerJson(r);
        }

        [HttpGet]
        public async Task<ActionResult> GetChestSize(string q = "", int limit = 0, string fieldValue = "", string fieldText = "", string value = "", int p = 1)
        {
            if (string.IsNullOrEmpty(q)) q = value;
            var r = await Services.GetAsync<dynamic>($"/api/BlendPacking/GetChestSize?search={q}&pageSize={(limit > 0 ? limit : 50)}");
            return PickerJson(r);
        }

        [HttpGet]
        // inputpicker endpoint for the Blend No field (header + each grid row) -- same
        // (q, limit, ..., p) contract and { data, count } response as GetMark & co.
        // Only Final Blends for THIS entry's Unit + Packet Type (Blend Type) that still have
        // qty left to pack (the API drops fully packed blends). `unit` is the entry screen's
        // Unit; it must be one the user is linked to, otherwise nothing is offered.
        // The permission lookup and the list call run side by side (one round trip saved);
        // the list is simply discarded when the unit turns out not to be permitted.
        public async Task<ActionResult> GetFinalBlendList(string q = "", int limit = 0, string fieldValue = "", string fieldText = "",
                                                          string value = "", int p = 1, string blendType = "", string unit = "")
        {
            if (string.IsNullOrWhiteSpace(blendType) || string.IsNullOrWhiteSpace(unit))
                return PickerJson(null);

            var unitsTask = GetUnitsForUserAsync();
            var listTask = Services.GetAsync<dynamic>(
                $"/api/BlendPacking/GetFinalBlendList?blendType={Uri.EscapeDataString(blendType)}&unit={Uri.EscapeDataString(unit.Trim())}" +
                $"&search={Uri.EscapeDataString(q ?? "")}&top={(limit > 0 ? limit : 30)}");
            await Task.WhenAll(unitsTask, listTask);

            if (!UnitScope.IsAllowed(unit, unitsTask.Result)) return PickerJson(null);
            return PickerJson(listTask.Result);
        }

        // GET: Packing/GetRowDetail (AJAX, Index list's "+" toggle) -- the document's packing
        // lines. Round-trips the API's response through the typed model (same as InsertOrUpdate's
        // edit load) so the JS sees the C# property names (MARK, MarkName, ...) rather than the
        // API's CamelCase-mangled keys.
        [HttpGet]
        public async Task<ActionResult> GetRowDetail(string docno = "", string docdt = "", string blendType = "", string unit = "")
        {
            var r = await Services.GetAsync<dynamic>(
                $"/api/BlendPacking/GetRowLines?docno={Uri.EscapeDataString(docno ?? "")}&docdt={Uri.EscapeDataString(docdt ?? "")}&blendType={Uri.EscapeDataString(blendType ?? "")}&unit={Uri.EscapeDataString(unit ?? "")}");
            if (!r.IsSuccessStatusCode || r.Data == null)
                return JsonExact(new { success = false, message = r.Message ?? "Record not found." });

            var wrapper = JsonConvert.DeserializeObject<GetByDocNoResult>(JsonConvert.SerializeObject(r.Data));
            return JsonExact(new { success = true, details = wrapper?.details ?? new List<T_BLEND_PACKING>() });
        }

        [HttpGet]
        public async Task<ActionResult> GetFinalBlendRowValues(string docno, string docdt, string blendType, string excludeDocNo = "", string excludeDocDt = "", string unit = "")
        {
            var r = await Services.GetAsync<dynamic>(
                $"/api/BlendPacking/GetFinalBlendRowValues?docno={docno}&docdt={docdt}&blendType={blendType}&excludeDocNo={excludeDocNo}&excludeDocDt={excludeDocDt}&unit={Uri.EscapeDataString(unit ?? "")}");
            return JsonExact(r.Data);
        }
    }
}
