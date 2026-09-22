using Newtonsoft.Json;
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
    // "Final Blend Entry" -- ported from the VB6 trn_blend_sheet.frm form
    // (MDI menu item 20: "Tea Blending Against Master Sheet", ENTRYBLDISS_Click,
    // pBlend_Type suffixed "Y" / APPROVED = 'Y'). Sibling of MasterBlendEntryController
    // (APPROVED = 'N'), which this screen issues against by picking one of its
    // records (spec §3.5) -- backed by ClassicERPCoreAPI's FinalBlendController.
    //
    // KNOWN GAP: the VB6 stock-availability engine (PurchaseStkSql/FinishedStkSql)
    // was never supplied and is NOT reproduced here -- see FinalBlendController's
    // header comment on the API side for the exact scope of what's stubbed.
    public class FinalBlendEntryController : Controller
    {
        // Same six Blend Types as Master Blend Entry -- a Final Blend is always
        // raised against a Master Blend of the same type.
        public static readonly Dictionary<string, string> BlendTypes = MasterBlendEntryController.BlendTypes;

        // Same VB6-menu-derived Blend-Type -> Unit mapping as MasterBlendEntryController
        // (see that class for the full rationale) -- duplicated rather than shared since
        // neither controller currently factors this out to a common base.
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

        // The Unit picked on the list page (the "New" row, or the row being edited/viewed/
        // deleted) wins over the session's CurrentUnit, so every lookup on the entry screen
        // is narrowed to the sheet's own unit rather than whichever unit the user logged into.
        private string UnitOrCurrent(string unit) => !string.IsNullOrEmpty(unit) ? unit : CurrentUnit;

        // Packet/Blend Type codes (M_SALETYPE.CODE, TRN_TYPE='P') the current user has
        // rights to, per FACT_JSTIL2027.M_UNIT_USER_RIGHT -- identical rule to Master
        // Blend Entry's list (see MasterBlendEntryController.GetAllowedBlendTypesAsync
        // for the fallback rationale). Reuses the same API endpoint rather than a Final
        // Blend-specific one: the rights table is per user and type, not per screen.
        private async Task<Dictionary<string, string>> GetAllowedBlendTypesAsync()
        {
            var response = await Services.GetAsync<List<string>>("/api/TeaBlend/GetAllowedBlendTypes");
            if (!response.IsSuccessStatusCode || response.Data == null)
                return BlendTypes;

            return BlendTypes.Where(bt => response.Data.Contains(bt.Key))
                              .ToDictionary(bt => bt.Key, bt => bt.Value);
        }

        // User.UnitList for the PacketTea module (see JwtMiddleware.cs) -- backs the list
        // page's "New" inline row Unit picker, same as MasterBlendEntryController (reuses
        // the same generic, module-scoped API endpoint rather than duplicating it).
        private async Task<List<UnitOption>> GetUnitsForUserAsync()
        {
            var response = await Services.GetAsync<List<UnitOption>>("/api/TeaBlend/GetUnitsForUser");
            return (response.IsSuccessStatusCode ? response.Data : null) ?? new List<UnitOption>();
        }

        // AEDV rights + back-date days (Aday/Eday) -- same shared "PacketTeaPurchaseEntry"
        // permission bucket as MasterBlendEntryController (see its comment).
        private AEDV Permission =>
            ((List<AEDV>)Session["User_AEDV"])?.FirstOrDefault(l => l.Controller == "PacketTeaPurchaseEntry");

        // Doc Date picker for a new entry: the FY bounds, narrowed to no earlier than the
        // back-date policy allows (Aday days back) and no later than today.
        private void SetNewDocDateBounds(AEDV perm)
        {
            var backDateMin = perm?.MinDocDate(true) ?? DateTime.Today;
            var min = backDateMin;
            var max = DateTime.Today;
            if (DateTime.TryParse((string)ViewBag.FyStart, out var fs) && fs > min) min = fs;
            if (DateTime.TryParse((string)ViewBag.FyEnd, out var fe) && fe < max) max = fe;
            ViewBag.BackDateMin = backDateMin.ToString("yyyy-MM-dd");
            ViewBag.DocDtMin = min.ToString("yyyy-MM-dd");
            ViewBag.DocDtMax = max.ToString("yyyy-MM-dd");
        }

        // GET: FinalBlendEntry
        public async Task<ActionResult> Index(string blendType, string searchString, int? page = 1, int pageSize = 15, string sortBy = "", string sortDir = "")
        {
            ViewBag.Permission = Permission;
            ViewBag.CurrentFilter = searchString;
            ViewBag.SortBy = sortBy;
            ViewBag.SortDir = sortDir;
            ViewBag.PageSize = pageSize;
            ViewBag.Page = page ?? 1;
            ViewBag.BlendType = blendType;
            ViewBag.BlendTypes = await GetAllowedBlendTypesAsync();
            ViewBag.UnitList = await GetUnitsForUserAsync();

            var response = await Services.GetAsync<PageModel<T_TEA_BLEND>>(
                $"/api/FinalBlend/GetByPage?blendType={blendType}&unit={CurrentUnit}&search={searchString}&page={page}&pageSize={pageSize}&sortBy={sortBy}&sortDir={sortDir}");

            var list = response?.Data?.value?.results ?? new List<T_TEA_BLEND>();
            ViewBag.RowCount = response?.Data?.value?.rowCount ?? 0;

            if (!response?.IsSuccessStatusCode ?? false)
            {
                TempData["toastrError"] = !string.IsNullOrEmpty(response?.Message)
                    ? response.Message
                    : $"Unable to load Final Blend list (API returned {response?.StatusCode}).";
            }

            return View(list);
        }

        // GET: FinalBlendEntry/InsertOrUpdate
        // `unit` is only ever populated when this was opened from the list page's "New"
        // inline row -- Unit + Blend Type were already chosen there, so the header repeats
        // them read-only instead of leaving Unit unset and Blend Type re-editable (same
        // pattern as MasterBlendEntryController.InsertOrUpdate).
        // `view` is set when opened via the list page's "View" action -- same fetch as
        // Edit, but the whole form renders read-only.
        public async Task<ActionResult> InsertOrUpdate(string docno = "", string docdt = "", string blendType = "", string unit = "", bool view = false)
        {
            // Rights-filtered here too, not just on the list: reaching this screen without
            // going through the list's "New" row would otherwise offer every blend type in
            // its own dropdown regardless of M_UNIT_USER_RIGHT (same fix as Master Blend).
            var allowedBlendTypes = await GetAllowedBlendTypesAsync();
            ViewBag.BlendTypes = allowedBlendTypes;
            ViewBag.LockedFromList = string.IsNullOrEmpty(docno) && !string.IsNullOrEmpty(unit) && !string.IsNullOrEmpty(blendType);
            ViewBag.IsView = view;

            string fy = Session["SelectedfinancialYear"]?.ToString();
            if (!string.IsNullOrEmpty(fy) && fy.Contains("-"))
            {
                var parts = fy.Split('-');
                string startDigits = new string(parts[0].Where(char.IsDigit).ToArray());
                string endDigits = new string(parts[1].Where(char.IsDigit).ToArray());
                if (startDigits.Length >= 4 && endDigits.Length >= 4)
                {
                    ViewBag.FyShortFrom = startDigits.Substring(startDigits.Length - 2);
                    ViewBag.FyShortTo = endDigits.Substring(endDigits.Length - 2);
                    ViewBag.FyStart = new DateTime(int.Parse(startDigits.Substring(startDigits.Length - 4)), 4, 1).ToString("yyyy-MM-dd");
                    ViewBag.FyEnd = new DateTime(int.Parse(endDigits.Substring(endDigits.Length - 4)), 3, 31).ToString("yyyy-MM-dd");
                }
            }

            var perm = Permission;
            ViewBag.Permission = perm;

            if (string.IsNullOrEmpty(docno))
            {
                if (!(perm?.Can('A') ?? false))
                {
                    TempData["toastrWarning"] = "You do not have permission to add a new Final Blend entry.";
                    return RedirectToAction("Index", new { blendType });
                }
                SetNewDocDateBounds(perm);

                // New entry -- Master Blend, and everything it carries, is picked
                // on-screen (spec §3.5.1); nothing to clone until then.
                // Default to "PT" only when the user actually has rights to it --
                // otherwise fall back to their first allowed type, so the Unit guess
                // below isn't derived from a type they can't select (same as Master Blend).
                var effectiveBlendType = !string.IsNullOrEmpty(blendType) ? blendType :
                    allowedBlendTypes.ContainsKey("PT") ? "PT" :
                    allowedBlendTypes.Keys.FirstOrDefault() ?? "PT";
                var model = new TEA_BLEND_DATA
                {
                    T_TEA_BLEND = new T_TEA_BLEND
                    {
                        LOCA = CurrentLoca,
                        GLOCA = CurrentLoca,
                        // Honor the Unit explicitly chosen on the list page's "New" row over
                        // the BlendTypeUnit stopgap guess (see UnitForBlendType's comment).
                        UNIT = !string.IsNullOrEmpty(unit) ? unit : UnitForBlendType(effectiveBlendType),
                        BLEND_TYPE = effectiveBlendType,
                        DOCDT = DateTime.Today,
                        APPROVED = "Y"
                    },
                    T_TEA_BLEND_DET = new List<T_TEA_BLEND_DET>()
                };
                ViewBag.IsEdit = false;
                return View(model);
            }

            // Edit mode
            var response = await Services.GetAsync<dynamic>(
                $"/api/FinalBlend/GetByDocNo?docno={docno}&docdt={docdt}&blendType={blendType}&unit={UnitOrCurrent(unit)}&forView={view}");

            if (!response.IsSuccessStatusCode || response.Data == null)
            {
                TempData[response.FailureToastKey] = response.Message ?? "Record not found.";
                return RedirectToAction("Index", new { blendType });
            }

            var json = JsonConvert.SerializeObject(response.Data);
            var wrapper = JsonConvert.DeserializeObject<GetByDocNoResult>(json);

            // AEDV "E" right + back-date policy (Eday) -- View mode stays open to everyone.
            if (!view && wrapper.head != null)
            {
                var denied = AEDV.CheckAddEdit(perm, false, wrapper.head.DOCDT);
                if (denied != null)
                {
                    TempData["toastrWarning"] = denied;
                    return RedirectToAction("Index", new { blendType });
                }
            }

            var editModel = new TEA_BLEND_DATA
            {
                T_TEA_BLEND = wrapper.head,
                T_TEA_BLEND_DET = wrapper.details ?? new List<T_TEA_BLEND_DET>()
            };
            ViewBag.IsEdit = true;
            return View(editModel);
        }

        private class GetByDocNoResult
        {
            public T_TEA_BLEND head { get; set; }
            public List<T_TEA_BLEND_DET> details { get; set; }
        }

        // POST: FinalBlendEntry/Save (AJAX, body = TEA_BLEND_DATA as JSON)
        [HttpPost]
        public async Task<JsonResult> Save(TEA_BLEND_DATA model)
        {
            if (model?.T_TEA_BLEND != null)
            {
                model.T_TEA_BLEND.LOCA = string.IsNullOrEmpty(model.T_TEA_BLEND.LOCA) ? CurrentLoca : model.T_TEA_BLEND.LOCA;
                model.T_TEA_BLEND.GLOCA = string.IsNullOrEmpty(model.T_TEA_BLEND.GLOCA) ? CurrentLoca : model.T_TEA_BLEND.GLOCA;
                model.T_TEA_BLEND.UNIT = string.IsNullOrEmpty(model.T_TEA_BLEND.UNIT)
                    ? UnitForBlendType(model.T_TEA_BLEND.BLEND_TYPE)
                    : model.T_TEA_BLEND.UNIT;
            }

            // Re-checked here, not just on the form: the post can be replayed, or the page
            // left open past the back-date window.
            if (model?.T_TEA_BLEND != null)
            {
                var denied = AEDV.CheckAddEdit(Permission, string.IsNullOrEmpty(model.T_TEA_BLEND.DOCNO), model.T_TEA_BLEND.DOCDT);
                if (denied != null)
                    return Json(new { success = false, message = denied, warning = true });
            }

            var response = await Services.PostAsync<dynamic>("/api/FinalBlend/SaveOrUpdate", model);
            return Json(new
            {
                success = response.IsSuccessStatusCode,
                message = response.IsSuccessStatusCode ? "Saved successfully." : (response.Message ?? "Save failed."),
                warning = response.IsValidationFailure,
                data = response.Data
            });
        }

        // POST: FinalBlendEntry/Delete
        [HttpPost]
        public async Task<ActionResult> Delete(string docno, string docdt, string blendType, string unit = "")
        {
            if (!(Permission?.Can('D') ?? false))
            {
                TempData["toastrWarning"] = "You do not have permission to delete this entry.";
                return RedirectToAction("Index", new { blendType });
            }

            var response = await Services.PostAsync<dynamic>(
                $"/api/FinalBlend/Delete?docno={docno}&docdt={docdt}&blendType={blendType}&unit={UnitOrCurrent(unit)}", new { });

            TempData[response.IsSuccessStatusCode ? "toastrSuccess" : response.FailureToastKey] =
                response.IsSuccessStatusCode ? "Deleted successfully." : (response.Message ?? "Delete failed.");

            return RedirectToAction("Index", new { blendType });
        }

        // GET: FinalBlendEntry/CanEdit (AJAX, Index list's Blend No link / Edit button) --
        // checked before navigating so a packed/locked sheet is refused in place.
        [HttpGet]
        public async Task<ActionResult> CanEdit(string docno = "", string docdt = "", string blendType = "", string unit = "")
        {
            // AEDV "E" right + back-date policy first -- no API round trip needed to refuse.
            if (DateTime.TryParse(docdt, out var docDate))
            {
                var denied = AEDV.CheckAddEdit(Permission, false, docDate);
                if (denied != null)
                    return JsonExact(new { canEdit = false, message = denied });
            }

            var r = await Services.GetAsync<dynamic>(
                $"/api/FinalBlend/CanEdit?docno={docno}&docdt={docdt}&blendType={blendType}&unit={UnitOrCurrent(unit)}");
            return JsonExactOrSessionExpired(r);
        }

        // GET: FinalBlendEntry/GetRowDetail (AJAX, Index list's "+" toggle)
        [HttpGet]
        public async Task<ActionResult> GetRowDetail(string docno = "", string docdt = "", string blendType = "", string unit = "")
        {
            var r = await Services.GetAsync<dynamic>(
                $"/api/FinalBlend/GetRowDetail?docno={docno}&docdt={docdt}&blendType={blendType}&unit={UnitOrCurrent(unit)}");
            if (!r.IsSuccessStatusCode || r.Data == null)
                return JsonExact(new { success = false, message = r.Message ?? "Record not found." });

            var obj = (Newtonsoft.Json.Linq.JObject)r.Data;
            obj["success"] = true;

            var detResp = await Services.GetAsync<dynamic>(
                $"/api/FinalBlend/GetByDocNo?docno={docno}&docdt={docdt}&blendType={blendType}&unit={UnitOrCurrent(unit)}");
            List<T_TEA_BLEND_DET> details = null;
            if (detResp.IsSuccessStatusCode && detResp.Data != null)
            {
                var detJson = JsonConvert.SerializeObject(detResp.Data);
                details = JsonConvert.DeserializeObject<GetByDocNoResult>(detJson)?.details;
            }
            obj["details"] = Newtonsoft.Json.Linq.JArray.FromObject(details ?? new List<T_TEA_BLEND_DET>());

            return JsonExact(obj);
        }

        // =====================================================================
        // Master Blend picker (spec §3.5) + shared master-data lookup passthroughs.
        // See MasterBlendEntryController's JsonExact/NOTE for why these route
        // through Newtonsoft instead of MVC5's own Json(...).
        // =====================================================================
        private ActionResult JsonExact(object data) =>
            Content(JsonConvert.SerializeObject(data), "application/json");

        // A failed call (expired token, API unreachable, ...) comes back as Data == null
        // with IsSuccessStatusCode == false; forwarding that as a plain 200 "null" reads
        // to every picker's JS as "no matches" with no visible error. Surface it as a
        // distinct status the frontend can tell apart -- same as MasterBlendEntryController.
        private ActionResult JsonExactOrSessionExpired<T>(ResponseApiModel<T> r)
        {
            if (!r.IsSuccessStatusCode)
            {
                // Only an auth failure is a dead session; anything else (API 500, bad
                // request, ...) is passed on with the API's own message so the picker can
                // show what actually went wrong instead of a misleading "session expired".
                if (r.StatusCode == "Unauthorized" || r.StatusCode == "Forbidden")
                {
                    Response.StatusCode = 440; // Login Timeout
                    return JsonExact(new { sessionExpired = true, message = "Your session has expired. Please log in again." });
                }
                Response.StatusCode = 502; // Bad Gateway -- the API call behind this proxy failed
                return JsonExact(new { message = r.Message ?? r.Title ?? ("Server error (" + (r.StatusCode ?? "no response") + ").") });
            }
            return JsonExact(r.Data);
        }

        [HttpGet]
        public async Task<ActionResult> GetMasterBlendList(string blendType = "", string unit = "", string search = "")
        {
            var r = await Services.GetAsync<dynamic>(
                $"/api/FinalBlend/GetMasterBlendList?blendType={blendType}&unit={UnitOrCurrent(unit)}&search={search}");
            return JsonExactOrSessionExpired(r);
        }

        [HttpGet]
        public async Task<ActionResult> GetMasterBlendDetail(string docno, string docdt, string blendType = "", string unit = "")
        {
            var r = await Services.GetAsync<dynamic>(
                $"/api/FinalBlend/GetMasterBlendDetail?docno={docno}&docdt={docdt}&blendType={blendType}&unit={UnitOrCurrent(unit)}");
            if (!r.IsSuccessStatusCode || r.Data == null)
                return JsonExact(new { success = false, message = r.Message ?? "Master Blend not found." });

            // IMPORTANT: round-trip through the typed head/details shape (same as
            // InsertOrUpdate's edit-mode load below) rather than forwarding the API's
            // raw dynamic JSON straight to the browser. The API serializes that
            // response with System.Text.Json's CamelCase policy, which mangles any
            // underscored name (BLEND_NO -> "blenD_NO", not "blendNo" -- it only
            // lowercases the leading run of uppercase letters and stops dead at the
            // first underscore) -- every other screen in this app is shielded from
            // that by exactly this round-trip (Newtonsoft's case-INsensitive property
            // binding matches "blenD_NO" to BLEND_NO regardless of the mangling); a
            // raw pass-through would have handed the grid JS keys like "blenD_NO"
            // that don't exist under any name it was written to read.
            var json = JsonConvert.SerializeObject(r.Data);
            var wrapper = JsonConvert.DeserializeObject<GetByDocNoResult>(json);
            return JsonExact(new { success = true, head = wrapper.head, details = wrapper.details ?? new List<T_TEA_BLEND_DET>() });
        }

        // Shared master-data pickers (Party/Warehouse/Allocation/Grade/Mark/Transporter)
        // are the same generic lookups Master Blend Entry uses -- proxied straight
        // through to TeaBlendController's endpoints rather than duplicating them.
        [HttpGet]
        public async Task<ActionResult> GetParty(string search = "")
        {
            var r = await Services.GetAsync<dynamic>($"/api/TeaBlend/GetParty?search={search}&pageSize=50");
            return JsonExactOrSessionExpired(r);
        }

        [HttpGet]
        public async Task<ActionResult> GetWarehouse(string search = "")
        {
            var r = await Services.GetAsync<dynamic>($"/api/TeaBlend/GetWarehouse?search={search}&pageSize=50");
            return JsonExactOrSessionExpired(r);
        }

        [HttpGet]
        public async Task<ActionResult> GetAllocation(string search = "")
        {
            var r = await Services.GetAsync<dynamic>($"/api/TeaBlend/GetAllocation?search={search}&pageSize=50");
            return JsonExactOrSessionExpired(r);
        }

        [HttpGet]
        public async Task<ActionResult> GetBlendGrade(string search = "")
        {
            var r = await Services.GetAsync<dynamic>($"/api/TeaBlend/GetBlendGrade?search={search}&pageSize=50");
            return JsonExactOrSessionExpired(r);
        }

        [HttpGet]
        public async Task<ActionResult> GetMark(string search = "")
        {
            var r = await Services.GetAsync<dynamic>($"/api/TeaBlend/GetMark?search={search}&pageSize=50");
            return JsonExactOrSessionExpired(r);
        }

        [HttpGet]
        public async Task<ActionResult> GetTransporter(string search = "")
        {
            var r = await Services.GetAsync<dynamic>($"/api/TeaBlend/GetTransporter?search={search}&pageSize=50");
            return JsonExactOrSessionExpired(r);
        }

        [HttpGet]
        public async Task<ActionResult> GenerateDoNo(string blendType, string fyShortFrom, string fyShortTo, string[] whCodes, string unit = "")
        {
            var qs = string.Join("&", (whCodes ?? new string[0]).Select(w => "whCodes=" + Uri.EscapeDataString(w)));
            var r = await Services.GetAsync<dynamic>(
                $"/api/TeaBlend/GenerateDoNo?blendType={blendType}&unit={UnitOrCurrent(unit)}&fyShortFrom={fyShortFrom}&fyShortTo={fyShortTo}&{qs}");
            return JsonExactOrSessionExpired(r);
        }
    }
}
