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
using System.Web.Script.Serialization;
using static PacketTea.Helpers;

namespace Finance.Controllers.TEA
{
    // "Master Blend Entry" -- ported from the VB6 trn_blend_sheet.frm form
    // (MDI menu item 19: Master Blend Sheet Entry, pBlend_Type without the
    // "Y" suffix / APPROVED = 'N'). "Blending Against Master Sheet" and
    // "Packing Entry From Blend Sheet" (menu items 20-21) are a later phase.
    public class MasterBlendEntryController : Controller
    {
        // Six blend types the VB MDI menu exposed as separate menu entries/forms;
        // collapsed here into one screen with a "Blend Type" dropdown.
        public static readonly Dictionary<string, string> BlendTypes = new Dictionary<string, string>
        {
            { "PT", "Packet Tea" },
            { "TT", "Tea Trading" },
            { "WT", "Web Tea" },
            { "BT", "Bagicha Tea" },
            { "ST", "Sale Tea" },
            { "TB", "Birla Tea" }
        };

        // SessionHelper.GetUser().CurentUnit is never assigned anywhere in this app (searched
        // the whole solution -- ModuleController's company/location/FY selection sets Loca,
        // CurentLocation, CurentCompany, DocYear, etc., but no CurentUnit), so CurrentUnit
        // below is always blank. T_TEA_BLEND.UNIT is NOT NULL, so an unguarded save left it
        // NULL and Oracle rejected the insert with "ORA-01400: cannot insert NULL into ...
        // UNIT". The VB6 form this screen replaces exposed each Blend Type as its own menu
        // item, each hard-wired to one specific unit; this mirrors that exact mapping, as
        // observed with zero exceptions across all 308 existing rows in FACT_JSTIL2027
        // (M_UNIT: GORA="Packet Tea Division", BGCH="Bagicha Tea Division",
        // TTSI="TT South India", JSTI="Head Office"). This is a stopgap for the current
        // company/location -- the real fix is wiring CurentUnit into the session during
        // company/location selection so a different company's units resolve correctly too.
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

        // Blend Type codes (M_SALETYPE.CODE, TRN_TYPE='P') the current user has rights to,
        // per FACT_JSTIL2027.M_UNIT_USER_RIGHT -- item 1 of the "Master Blend Entry list
        // form" spec. Falls back to the full BlendTypes list on any API failure (schema
        // not reachable, etc.) rather than locking every user out of the screen; an empty
        // -but-successful- response (user genuinely has zero rights rows) does filter down
        // to nothing, which is the point of the restriction. Shared (static) so Final Blend
        // Entry and Packing Entry offer exactly the same Packet Types.
        // The API returns { CODE: M_SALETYPE.DESCN }, so the names shown are the table's own
        // (renaming a type in M_SALETYPE shows up here); BlendTypes' fixed names are only the
        // fallback when the call fails.
        internal static async Task<Dictionary<string, string>> GetAllowedBlendTypesAsync()
        {
            var response = await Services.GetAsync<Dictionary<string, string>>("/api/TeaBlend/GetAllowedBlendTypes");
            if (!response.IsSuccessStatusCode || response.Data == null)
                return BlendTypes;
            return response.Data;
        }

        // Every Blend Type for an entry screen's dropdown (an older record may carry a type the
        // user no longer holds), named from M_SALETYPE.DESCN wherever the API returned one.
        internal static async Task<Dictionary<string, string>> GetBlendTypeNamesAsync()
        {
            var names = new Dictionary<string, string>(BlendTypes);
            foreach (var t in await GetAllowedBlendTypesAsync())
                names[t.Key] = t.Value;
            return names;
        }

        // User.UnitList for the PacketTea module (see JwtMiddleware.cs) -- backs the
        // list page's "New" inline row Unit picker (item 2 of the spec).
        private async Task<List<UnitOption>> GetUnitsForUserAsync()
        {
            var response = await Services.GetAsync<List<UnitOption>>("/api/TeaBlend/GetUnitsForUser");
            return (response.IsSuccessStatusCode ? response.Data : null) ?? new List<UnitOption>();
        }

        // Save-time Unit for a NEW record, shared by Master / Final Blend and Packing: the record is
        // stored under the Unit M_SALETYPE gives its type (CODE = type, TRN_TYPE = 'P') -- e.g. PT
        // is saved as GORA even when the list's "New" row was opened under JSTI (user's request,
        // 2026-10-02). The posted Unit wins only when it is one of the type's own units. A type with
        // no M_SALETYPE row (or an API failure reading it) keeps the posted Unit. The caller checks
        // the *posted* (list-picked) Unit against the user's USER_SCHEMA_LINK units.
        // Returns (unit to store, error message or null -- currently always null).
        internal static async Task<(string Unit, string Error)> ResolveTypeUnitAsync(string type, string postedUnit)
        {
            postedUnit = (postedUnit ?? "").Trim();
            if (string.IsNullOrWhiteSpace(type)) return (postedUnit, null);
            var resp = await Services.GetAsync<List<string>>("/api/TeaBlend/GetUnitsForType?type=" + Uri.EscapeDataString(type.Trim()));
            var typeUnits = (resp.IsSuccessStatusCode ? resp.Data : null) ?? new List<string>();
            if (typeUnits.Count == 0) return (postedUnit, null);
            if (postedUnit.Length == 0) return (typeUnits[0], null);
            var match = typeUnits.FirstOrDefault(u => string.Equals(u.Trim(), postedUnit, StringComparison.OrdinalIgnoreCase));
            return (match ?? typeUnits[0], null);
        }

        // GET: MasterBlendEntry/GetUnitForType?type=PT
        // Shared by every list's "New" row (Scripts/list-new-row-pickers.js): picking a Packet /
        // Blend / AWR Type takes its Unit from M_SALETYPE (CODE = type, TRN_TYPE = 'P'). The
        // user's own linked unit wins when the type has several; `allowed` is false when none of
        // the type's units is one the user is linked to (the screen then refuses to continue).
        // No M_SALETYPE row for the code -> unit "" (the caller keeps whatever Unit is picked).
        [HttpGet]
        public async Task<JsonResult> GetUnitForType(string type)
        {
            var resp = await Services.GetAsync<List<string>>("/api/TeaBlend/GetUnitsForType?type=" + Uri.EscapeDataString(type ?? ""));
            var typeUnits = (resp.IsSuccessStatusCode ? resp.Data : null) ?? new List<string>();
            if (typeUnits.Count == 0)
                return Json(new { unit = "", allowed = true }, JsonRequestBehavior.AllowGet);

            var mine = await GetUnitsForUserAsync();
            var pick = typeUnits.FirstOrDefault(u => UnitScope.IsAllowed(u, mine));
            return Json(new { unit = pick ?? typeUnits[0], allowed = pick != null }, JsonRequestBehavior.AllowGet);
        }

        // GET: MasterBlendEntry
        // The list is scoped to the Units the user is linked to in USER_SCHEMA_LINK (and to the
        // one `unit` / `blendType` asked for, when given) -- never to "everything", which is what
        // the blank CurrentUnit used to send.
        public async Task<ActionResult> Index(string blendType, string searchString, int? page = 1, int pageSize = 15, string unit = "")
        {
            var sdsd = (List<AEDV>)Session["User_AEDV"];
            // This screen's own "MasterBlendEntry" row only -- see AEDV.ForScreen.
            ViewBag.Permission = AEDV.ForScreen(sdsd, "MasterBlendEntry");
            ViewBag.CurrentFilter = searchString;
            ViewBag.PageSize = pageSize;
            ViewBag.Page = page ?? 1;
            ViewBag.BlendType = blendType;
            // The two lookups are independent -- run them side by side instead of back to back.
            var allowedTask = GetAllowedBlendTypesAsync();
            var unitsTask = GetUnitsForUserAsync();
            await Task.WhenAll(allowedTask, unitsTask);
            var allowedTypes = allowedTask.Result;
            ViewBag.BlendTypes = allowedTypes;
            var userUnits = unitsTask.Result;
            ViewBag.UnitList = userUnits;
            var unitFilter = Uri.EscapeDataString(UnitScope.ListFilter(unit, userUnits));

            // Packet Type scoping mirrors Unit scoping: every Blend Type the user has rights to,
            // NOT the single `blendType` in the URL (a stray ?blendType=PT used to hide every
            // other type's sheets). They go to GetByPage as ONE comma-separated list, so the API
            // filters, sorts and pages once (this used to fire one call per type, each asking for
            // up to 5000 rows, then merge and page here). A user with no M_UNIT_USER_RIGHT rows
            // gets no type restriction (blank list) rather than an empty list -- the Unit scope
            // above still applies.
            // Chunked list: a full page view always starts at the first chunk; further chunks
            // arrive as AJAX calls (see below) and return just the table rows.
            var isChunkRequest = Request.IsAjaxRequest();
            var pageNo = isChunkRequest ? Math.Max(page ?? 1, 1) : 1;
            ViewBag.Page = pageNo;
            ViewBag.RowOffset = (pageNo - 1) * pageSize;
            var search = Uri.EscapeDataString(searchString ?? "");
            var typeFilter = Uri.EscapeDataString(string.Join(",", allowedTypes.Keys));

            string error = null;
            var response = await Services.GetAsync<PageModel<T_TEA_BLEND>>(
                $"/api/TeaBlend/GetByPage?blendType={typeFilter}&unit={unitFilter}&search={search}&page={pageNo}&pageSize={pageSize}");
            var list = response?.Data?.value?.results ?? new List<T_TEA_BLEND>();
            var rowCount = response?.Data?.value?.rowCount ?? 0;
            if (!(response?.IsSuccessStatusCode ?? false))
                error = !string.IsNullOrEmpty(response?.Message) ? response.Message : $"API returned {response?.StatusCode}";
            ViewBag.RowCount = rowCount;
            // Rows another user is editing are greyed out (see Utility/RecordLock.cs).
            ViewBag.Locks = await RecordLock.LocksAsync("MasterBlendEntry");

            if (isChunkRequest)
            {
                // A failed chunk must not leave a toast queued for the next full page load; an
                // empty response tells the list's scroll loader to stop.
                if (error != null) list = new List<T_TEA_BLEND>();
                return PartialView("_ListRows", list);
            }

            if (error != null)
            {
                TempData["toastrError"] = $"Unable to load Master Blend list ({error}). " +
                      "Check ClassicERPCoreAPI is running the latest build and the T_TEA_BLEND tables exist.";
            }

            return View(list);
        }

        // GET: MasterBlendEntry/InsertOrUpdate
        // `unit` is only ever populated when this was opened from the list page's "New"
        // inline row (item 2) -- Unit + Blend Type were already chosen there, so the header
        // repeats them read-only instead of leaving Unit unset and Blend Type re-editable.
        // `view` is set when opened via the list page's "View" action -- same fetch as
        // Edit, but the whole form renders read-only (see InsertOrUpdate.cshtml's IS_VIEW).
        public async Task<ActionResult> InsertOrUpdate(string docno = "", string docdt = "", string blendType = "", string unit = "", bool view = false)
        {
            var sdsd = (List<AEDV>)Session["User_AEDV"];
            var permission = AEDV.ForScreen(sdsd, "MasterBlendEntry");
            ViewBag.Permission = permission;
            // Names for the read-only Unit box (the Unit's description; the code is still what is saved).
            ViewBag.UnitList = await GetUnitsForUserAsync();

            ViewBag.BlendTypes = await GetBlendTypeNamesAsync();
            ViewBag.LockedFromList = string.IsNullOrEmpty(docno) && !string.IsNullOrEmpty(unit) && !string.IsNullOrEmpty(blendType);
            // View only applies to an existing record and needs the View right (as Packing).
            view = view && !string.IsNullOrEmpty(docno);
            if (view && !(permission?.View ?? false))
            {
                TempData["toastrError"] = "You do not have permission to view this entry.";
                return RedirectToAction("Index", new { blendType });
            }
            ViewBag.IsView = view;

            bool isNewEntry = string.IsNullOrEmpty(docno);

            // The list page's New/Unit-Type picker already hides New when the user has no
            // Add right, but this URL is reachable directly -- block it here too (item 1 of
            // the AEDV spec: enforce rights server-side, not just by hiding buttons).
            if (isNewEntry && !view && !(permission?.Add ?? false))
            {
                TempData["toastrError"] = "You do not have permission to add a new entry.";
                return RedirectToAction("Index", new { blendType });
            }

            // Financial-year bounds for the Doc Date picker + the "yy-yy" short
            // years the VB form baked into the DO No prefix (loca/type/yy-yy/nnnn).
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
                    ViewBag.FyShortFrom = startDigits.Substring(startDigits.Length - 2);
                    ViewBag.FyShortTo = endDigits.Substring(endDigits.Length - 2);
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
                // ⭐ New entry
                var effectiveBlendType = string.IsNullOrEmpty(blendType) ? "PT" : blendType;
                var model = new TEA_BLEND_DATA
                {
                    T_TEA_BLEND = new T_TEA_BLEND
                    {
                        LOCA = CurrentLoca,
                        GLOCA = CurrentLoca,
                        // Honor the Unit explicitly chosen on the list page's "New" row over
                        // the BlendTypeUnit stopgap guess -- see UnitForBlendType's own comment
                        // for why that guess exists at all.
                        UNIT = !string.IsNullOrEmpty(unit) ? unit : UnitForBlendType(effectiveBlendType),
                        BLEND_TYPE = effectiveBlendType,
                        DOCDT = DateTime.Today,
                        APPROVED = "N"
                    },
                    T_TEA_BLEND_DET = new List<T_TEA_BLEND_DET>()
                };
                ViewBag.IsEdit = false;
                return View(model);
            }

            // ⭐ Edit mode
            var response = await Services.GetAsync<dynamic>(
                $"/api/TeaBlend/GetByDocNo?docno={docno}&docdt={docdt}&blendType={blendType}&unit={CurrentUnit}");

            if (!response.IsSuccessStatusCode || response.Data == null)
            {
                TempData["toastrError"] = response.Message ?? "Record not found.";
                return RedirectToAction("Index", new { blendType });
            }

            var json = JsonConvert.SerializeObject(response.Data);
            var wrapper = JsonConvert.DeserializeObject<GetByDocNoResult>(json);

            // Block opening Edit outright when the user has no Edit right, or the record's
            // own Doc Date has fallen outside the Eday back-date window -- View bypasses this
            // (spec: View is read-only regardless of the Edit/back-date policy).
            if (!view)
            {
                var editErr = AEDV.CheckAddEdit(permission, false, wrapper.head?.DOCDT ?? DateTime.Today);
                if (editErr != null)
                {
                    TempData["toastrError"] = editErr;
                    return RedirectToAction("Index", new { blendType });
                }
            }

            var editModel = new TEA_BLEND_DATA
            {
                T_TEA_BLEND = wrapper.head,
                T_TEA_BLEND_DET = wrapper.details ?? new List<T_TEA_BLEND_DET>()
            };

            // Edit lock: refused while another user has this document open for editing.
            if (!view)
            {
                var head = wrapper.head;
                var lockUnit = !string.IsNullOrEmpty(head?.UNIT) ? head.UNIT : unit;
                var lockDocNo = !string.IsNullOrEmpty(head?.DOCNO) ? head.DOCNO : docno;
                var lockDate = (object)head?.DOCDT ?? docdt;
                var lockType = !string.IsNullOrEmpty(head?.BLEND_TYPE) ? head.BLEND_TYPE : blendType;
                var lockErr = await RecordLock.AcquireAsync("MasterBlendEntry", lockUnit, lockDocNo, lockDate, lockType);
                if (lockErr != null)
                {
                    TempData["toastrError"] = lockErr;
                    return RedirectToAction("Index", new { blendType });
                }
                ViewBag.RecordLock = RecordLock.ClientConfig("MasterBlendEntry", lockUnit, lockDocNo, lockDate, lockType);
            }
            ViewBag.IsEdit = true;
            return View(editModel);
        }

        private class GetByDocNoResult
        {
            public T_TEA_BLEND head { get; set; }
            public List<T_TEA_BLEND_DET> details { get; set; }
        }

        // POST: MasterBlendEntry/Save  (AJAX, body = TEA_BLEND_DATA as JSON)
        [HttpPost]
        public async Task<JsonResult> Save(TEA_BLEND_DATA model)
        {
            // Server-side AEDV Add/Edit + back-date enforcement -- the date picker's `min`
            // (see InsertOrUpdate) only steers well-behaved clients; this is the actual gate.
            var sdsd = (List<AEDV>)Session["User_AEDV"];
            var permission = AEDV.ForScreen(sdsd, "MasterBlendEntry");
            bool isNew = model?.IsNew ?? !(model?.T_TEA_BLEND?.ID > 0);
            var permErr = AEDV.CheckAddEdit(permission, isNew, model?.T_TEA_BLEND?.DOCDT ?? DateTime.Today);
            if (permErr != null)
                return Json(new { success = false, message = permErr });

            // A NEW sheet is stamped with the Unit picked on the list (posted from the entry
            // screen), which must be one the user is linked to; only when none was posted does
            // it fall back to the Blend-Type -> Unit table below. Edits keep the record's own Unit.
            // The permission check is on the Unit picked on the list; the record itself is then
            // stored under the Blend Type's own Unit (ResolveTypeUnitAsync).
            if (isNew && !string.IsNullOrWhiteSpace(model?.T_TEA_BLEND?.UNIT)
                && !UnitScope.IsAllowed(model.T_TEA_BLEND.UNIT, await GetUnitsForUserAsync()))
                return Json(new { success = false, message = $"You do not have permission for Unit {model.T_TEA_BLEND.UNIT}." });
            if (isNew && model?.T_TEA_BLEND != null)
            {
                var tu = await ResolveTypeUnitAsync(model.T_TEA_BLEND.BLEND_TYPE, model.T_TEA_BLEND.UNIT);
                if (tu.Error != null) return Json(new { success = false, message = tu.Error });
                model.T_TEA_BLEND.UNIT = tu.Unit;
            }

            if (model?.T_TEA_BLEND != null)
            {
                model.T_TEA_BLEND.LOCA = string.IsNullOrEmpty(model.T_TEA_BLEND.LOCA) ? CurrentLoca : model.T_TEA_BLEND.LOCA;
                model.T_TEA_BLEND.GLOCA = string.IsNullOrEmpty(model.T_TEA_BLEND.GLOCA) ? CurrentLoca : model.T_TEA_BLEND.GLOCA;
                model.T_TEA_BLEND.UNIT = string.IsNullOrEmpty(model.T_TEA_BLEND.UNIT)
                    ? UnitForBlendType(model.T_TEA_BLEND.BLEND_TYPE)
                    : model.T_TEA_BLEND.UNIT;
            }

            // Edit lock: an edit may only be saved while no other user holds the document.
            var h = model?.T_TEA_BLEND;
            if (!isNew && h != null)
            {
                var lockErr = await RecordLock.AcquireAsync("MasterBlendEntry", h.UNIT, h.DOCNO, h.DOCDT, h.BLEND_TYPE);
                if (lockErr != null) return Json(new { success = false, message = lockErr });
            }

            var response = await Services.PostAsync<dynamic>("/api/TeaBlend/SaveOrUpdate", model);
            if (response.IsSuccessStatusCode && !isNew && h != null)
                await RecordLock.ReleaseAsync("MasterBlendEntry", h.UNIT, h.DOCNO, h.DOCDT, h.BLEND_TYPE);
            return Json(new
            {
                success = response.IsSuccessStatusCode,
                message = response.IsSuccessStatusCode ? "Saved successfully." : (response.Message ?? "Save failed."),
                data = response.Data
            });
        }

        // POST: MasterBlendEntry/Delete
        [HttpPost]
        public async Task<ActionResult> Delete(string docno, string docdt, string blendType)
        {
            var sdsd = (List<AEDV>)Session["User_AEDV"];
            var permission = AEDV.ForScreen(sdsd, "MasterBlendEntry");
            var delErr = AEDV.CheckDelete(permission, PacketTea.DateText.Parse(docdt));
            if (delErr != null)
            {
                TempData["toastrError"] = delErr;
                return RedirectToAction("Index", new { blendType });
            }

            // Not while another user has the document open for editing (the list posts no
            // Unit here; the API treats a blank Unit as any unit for this screen).
            var lockErr = await RecordLock.AcquireAsync("MasterBlendEntry", "", docno, docdt, blendType);
            if (lockErr != null)
            {
                TempData["toastrError"] = lockErr;
                return RedirectToAction("Index", new { blendType });
            }

            var response = await Services.PostAsync<dynamic>(
                $"/api/TeaBlend/Delete?docno={docno}&docdt={docdt}&blendType={blendType}&unit={CurrentUnit}", new { });
            if (!response.IsSuccessStatusCode)
                await RecordLock.ReleaseAsync("MasterBlendEntry", "", docno, docdt, blendType);

            TempData[response.IsSuccessStatusCode ? "toastrSuccess" : "toastrError"] =
                response.IsSuccessStatusCode ? "Deleted successfully." : (response.Message ?? "Delete failed.");

            return RedirectToAction("Index", new { blendType });
        }

        // GET: MasterBlendEntry/GetRowDetail (AJAX, fired by the Index list's "+"
        // toggle on every expand -- see ClassicERPCoreAPI's TeaBlendController
        // .GetRowDetail for why this doesn't reuse GetByDocNo).
        [HttpGet]
        public async Task<ActionResult> GetRowDetail(string docno = "", string docdt = "", string blendType = "")
        {
            // The "+" sub-grid only shows the item lines (Mark, Inv No, Grade, Qty, ...), so ask
            // the API's lean GetRowLines for just those -- ONE call. It used to call GetRowDetail
            // (header extras nobody displays any more) and then GetByDocNo (the whole edit-screen
            // payload, plus its edit guards) one after the other. Serialized through Newtonsoft
            // rather than MVC5's Json() -- see the NOTE above JsonExact.
            var r = await Services.GetAsync<dynamic>(
                $"/api/TeaBlend/GetRowLines?docno={Uri.EscapeDataString(docno ?? "")}&docdt={Uri.EscapeDataString(docdt ?? "")}&blendType={Uri.EscapeDataString(blendType ?? "")}&unit={CurrentUnit}");
            if (!r.IsSuccessStatusCode || r.Data == null)
                return JsonExact(new { success = false, message = r.Message ?? "Record not found." });

            var details = JsonConvert.DeserializeObject<GetByDocNoResult>(JsonConvert.SerializeObject(r.Data))?.details;
            return JsonExact(new { success = true, details = details ?? new List<T_TEA_BLEND_DET>() });
        }

        // =====================================================================
        // AJAX lookup passthroughs (Services.cs carries the auth headers that
        // the browser can't attach directly, so every picker call is proxied
        // through this controller).
        //
        // NOTE: Services.GetAsync<dynamic> deserializes the API's JSON with
        // Newtonsoft, which for an array/object body hands back a JArray/JObject.
        // MVC5's own Json(...) helper serializes with the *old*
        // System.Web.Script.Serialization.JavaScriptSerializer, which doesn't
        // know what a JArray/JObject is -- it reflects over their internal CLR
        // shape (the enumerator yields KeyValuePair<string, JToken>) instead of
        // their actual JSON content, so every field the picker JS reads
        // (row.CODE, row.NAME, ...) comes back undefined and every lookup
        // (Party/Warehouse/Allocation/Blend Grade/Blend Mark/Transporter/...)
        // renders with blank rows even though the row count is right. Route
        // through Newtonsoft (which already parsed the payload correctly) both
        // ways instead of handing it to the built-in serializer.
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
        public async Task<ActionResult> GetParty(string q = "", int limit = 0, string fieldValue = "", string fieldText = "", string value = "", int p = 1)
        {
            if (string.IsNullOrEmpty(q)) q = value;
            var r = await Services.GetAsync<dynamic>($"/api/TeaBlend/GetParty?search={Uri.EscapeDataString((q ?? "").Trim())}&pageSize={(limit > 0 ? limit : 50)}");
            return PickerJson(r);
        }

        [HttpGet]
        public async Task<ActionResult> GetWarehouse(string q = "", int limit = 0, string fieldValue = "", string fieldText = "", string value = "", int p = 1)
        {
            if (string.IsNullOrEmpty(q)) q = value;
            var r = await Services.GetAsync<dynamic>($"/api/TeaBlend/GetWarehouse?search={Uri.EscapeDataString((q ?? "").Trim())}&pageSize={(limit > 0 ? limit : 50)}");
            return PickerJson(r);
        }

        [HttpGet]
        public async Task<ActionResult> GetAllocation(string q = "", int limit = 0, string fieldValue = "", string fieldText = "", string value = "", int p = 1)
        {
            if (string.IsNullOrEmpty(q)) q = value;
            var r = await Services.GetAsync<dynamic>($"/api/TeaBlend/GetAllocation?search={Uri.EscapeDataString((q ?? "").Trim())}&pageSize={(limit > 0 ? limit : 50)}");
            return PickerJson(r);
        }

        [HttpGet]
        public async Task<ActionResult> GetBlendGrade(string q = "", int limit = 0, string fieldValue = "", string fieldText = "", string value = "", int p = 1)
        {
            if (string.IsNullOrEmpty(q)) q = value;
            var r = await Services.GetAsync<dynamic>($"/api/TeaBlend/GetBlendGrade?search={Uri.EscapeDataString((q ?? "").Trim())}&pageSize={(limit > 0 ? limit : 50)}");
            return PickerJson(r);
        }

        [HttpGet]
        public async Task<ActionResult> GetMark(string q = "", int limit = 0, string fieldValue = "", string fieldText = "", string value = "", int p = 1, string garden = "")
        {
            if (string.IsNullOrEmpty(q)) q = value;
            // `garden` (optional) narrows the marks to that garden -- "Select Data"'s dependent Mark dropdown.
            var r = await Services.GetAsync<dynamic>($"/api/TeaBlend/GetMark?search={Uri.EscapeDataString((q ?? "").Trim())}&pageSize={(limit > 0 ? limit : 50)}&garden={Uri.EscapeDataString(garden ?? "")}");
            return PickerJson(r);
        }

        [HttpGet]
        public async Task<ActionResult> GetGarden(string q = "", int limit = 0, string fieldValue = "", string fieldText = "", string value = "", int p = 1)
        {
            if (string.IsNullOrEmpty(q)) q = value;
            var r = await Services.GetAsync<dynamic>($"/api/TeaBlend/GetGarden?search={Uri.EscapeDataString((q ?? "").Trim())}&pageSize={(limit > 0 ? limit : 50)}");
            return PickerJson(r);
        }

        [HttpGet]
        public async Task<ActionResult> GetCategory(string q = "", int limit = 0, string fieldValue = "", string fieldText = "", string value = "", int p = 1)
        {
            if (string.IsNullOrEmpty(q)) q = value;
            var r = await Services.GetAsync<dynamic>($"/api/TeaBlend/GetCategory?search={Uri.EscapeDataString((q ?? "").Trim())}&pageSize={(limit > 0 ? limit : 50)}");
            return PickerJson(r);
        }

        [HttpGet]
        public async Task<ActionResult> GetTransporter(string q = "", int limit = 0, string fieldValue = "", string fieldText = "", string value = "", int p = 1)
        {
            if (string.IsNullOrEmpty(q)) q = value;
            var r = await Services.GetAsync<dynamic>($"/api/TeaBlend/GetTransporter?search={Uri.EscapeDataString((q ?? "").Trim())}&pageSize={(limit > 0 ? limit : 50)}");
            return PickerJson(r);
        }

        [HttpGet]
        public async Task<ActionResult> GetAvailableStock(string blendType, string garden = "", string mark = "", string category = "")
        {
            var r = await Services.GetAsync<dynamic>(
                $"/api/TeaBlend/GetAvailableStock?blendType={blendType}&garden={Uri.EscapeDataString(garden ?? "")}&mark={Uri.EscapeDataString(mark ?? "")}&category={Uri.EscapeDataString(category ?? "")}");
            return JsonExact(r.Data);
        }

        [HttpGet]
        public async Task<ActionResult> GenerateDoNo(string blendType, string fyShortFrom, string fyShortTo, string[] whCodes)
        {
            var qs = string.Join("&", (whCodes ?? new string[0]).Select(w => "whCodes=" + Uri.EscapeDataString(w)));
            var r = await Services.GetAsync<dynamic>(
                $"/api/TeaBlend/GenerateDoNo?blendType={blendType}&unit={CurrentUnit}&fyShortFrom={fyShortFrom}&fyShortTo={fyShortTo}&{qs}");
            return JsonExact(r.Data);
        }
    }
}
