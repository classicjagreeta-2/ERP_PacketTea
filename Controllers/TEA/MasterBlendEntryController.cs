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
        // to nothing, which is the point of the restriction.
        private async Task<Dictionary<string, string>> GetAllowedBlendTypesAsync()
        {
            var response = await Services.GetAsync<List<string>>("/api/TeaBlend/GetAllowedBlendTypes");
            if (!response.IsSuccessStatusCode || response.Data == null)
                return BlendTypes;

            return BlendTypes.Where(bt => response.Data.Contains(bt.Key))
                              .ToDictionary(bt => bt.Key, bt => bt.Value);
        }

        // User.UnitList for the PacketTea module (see JwtMiddleware.cs) -- backs the
        // list page's "New" inline row Unit picker (item 2 of the spec).
        private async Task<List<UnitOption>> GetUnitsForUserAsync()
        {
            var response = await Services.GetAsync<List<UnitOption>>("/api/TeaBlend/GetUnitsForUser");
            return (response.IsSuccessStatusCode ? response.Data : null) ?? new List<UnitOption>();
        }

        // GET: MasterBlendEntry
        public async Task<ActionResult> Index(string blendType, string searchString, int? page = 1, int pageSize = 15, string sortBy = "", string sortDir = "")
        {
            var sdsd = (List<AEDV>)Session["User_AEDV"];
            // "MasterBlendEntry" isn't a provisioned permission key yet (this is a new
            // screen); every other TEA controller in this app shares the
            // "PacketTeaPurchaseEntry" permission bucket, so match that convention
            // rather than always resolving to a missing entry and disabling New.
            ViewBag.Permission = sdsd?.FirstOrDefault(l => l.Controller == "PacketTeaPurchaseEntry");
            ViewBag.CurrentFilter = searchString;
            ViewBag.SortBy = sortBy;
            ViewBag.SortDir = sortDir;
            ViewBag.PageSize = pageSize;
            ViewBag.Page = page ?? 1;
            ViewBag.BlendType = blendType;
            ViewBag.BlendTypes = await GetAllowedBlendTypesAsync();
            ViewBag.UnitList = await GetUnitsForUserAsync();

            var response = await Services.GetAsync<PageModel<T_TEA_BLEND>>(
                $"/api/TeaBlend/GetByPage?blendType={blendType}&unit={CurrentUnit}&search={searchString}&page={page}&pageSize={pageSize}&sortBy={sortBy}&sortDir={sortDir}");

            var list = response?.Data?.value?.results ?? new List<T_TEA_BLEND>();
            ViewBag.RowCount = response?.Data?.value?.rowCount ?? 0;

            if (!response?.IsSuccessStatusCode ?? false)
            {
                TempData["toastrError"] = !string.IsNullOrEmpty(response?.Message)
                    ? response.Message
                    : $"Unable to load Master Blend list (API returned {response?.StatusCode}). " +
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
            // Same rights-filtered list Index() uses for the list page's "New" row
            // (GetAllowedBlendTypesAsync) -- using the raw BlendTypes dict here let a
            // user with rights to only e.g. PT/BT/TB pick TT/WT/ST straight from this
            // form's own Blend Type dropdown whenever it opens without going through
            // that "New" row (a brand-new entry not locked from the list).
            var allowedBlendTypes = await GetAllowedBlendTypesAsync();
            ViewBag.BlendTypes = allowedBlendTypes;
            ViewBag.LockedFromList = string.IsNullOrEmpty(docno) && !string.IsNullOrEmpty(unit) && !string.IsNullOrEmpty(blendType);
            ViewBag.IsView = view;

            // Financial-year bounds for the Doc Date picker + the "yy-yy" short
            // years the VB form baked into the DO No prefix (loca/type/yy-yy/nnnn).
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

            if (string.IsNullOrEmpty(docno))
            {
                // ⭐ New entry
                // Default to "PT" only when it's actually one of this user's allowed
                // types -- for a user with no PT rights (e.g. CORETT: TT/ST/WT), fall
                // back to whatever their first allowed type is instead, so the initial
                // Unit guess below (UnitForBlendType) isn't computed from a type they
                // can't even select in the dropdown.
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

        // POST: MasterBlendEntry/Save  (AJAX, body = TEA_BLEND_DATA as JSON)
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

            var response = await Services.PostAsync<dynamic>("/api/TeaBlend/SaveOrUpdate", model);
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
            var response = await Services.PostAsync<dynamic>(
                $"/api/TeaBlend/Delete?docno={docno}&docdt={docdt}&blendType={blendType}&unit={CurrentUnit}", new { });

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
            var r = await Services.GetAsync<dynamic>(
                $"/api/TeaBlend/GetRowDetail?docno={docno}&docdt={docdt}&blendType={blendType}&unit={CurrentUnit}");
            if (!r.IsSuccessStatusCode || r.Data == null)
                return JsonExact(new { success = false, message = r.Message ?? "Record not found." });

            // r.Data is a JObject (Services.GetAsync<dynamic> deserializes with
            // Newtonsoft) -- merge the success flag into it and send it straight
            // through JsonExact rather than round-tripping via MVC5's Json(),
            // for the same reason the lookup passthroughs below do (see the
            // NOTE above JsonExact).
            var obj = (Newtonsoft.Json.Linq.JObject)r.Data;
            obj["success"] = true;

            // GetRowDetail only carries the header fields that don't fit the main
            // grid -- it deliberately doesn't reuse GetByDocNo (see the note above
            // this action). The "+" expand also wants the item lines (Mark, Inv No,
            // Grade, Qty, ...) as a sub-grid, so fetch those the same way
            // InsertOrUpdate's edit mode does and merge them in under "details".
            // This only runs for the one row being expanded, not the whole list.
            var detResp = await Services.GetAsync<dynamic>(
                $"/api/TeaBlend/GetByDocNo?docno={docno}&docdt={docdt}&blendType={blendType}&unit={CurrentUnit}");
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

        // Services.GetAsync swallows a failed call (expired session token, refresh
        // failure, API unreachable, ...) into r.Data == null with r.IsSuccessStatusCode
        // == false -- returning JsonExact(r.Data) as-is for that case sends the browser
        // a plain 200 OK with body "null", which every picker's JS reads as "zero
        // results" with no visible error (this is exactly what made "Select Data" look
        // like it silently stopped fetching anything after the 45-minute token expired).
        // Surface it instead: a distinct status the frontend's .fail() handler can
        // recognize as "your session died, log in again" rather than "no matches".
        private ActionResult JsonExactOrSessionExpired<T>(ResponseApiModel<T> r)
        {
            if (!r.IsSuccessStatusCode)
            {
                Response.StatusCode = 440; // Login Timeout
                return JsonExact(new { sessionExpired = true, message = "Your session has expired. Please log in again." });
            }
            return JsonExact(r.Data);
        }

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
        public async Task<ActionResult> GetGarden(string search = "")
        {
            var r = await Services.GetAsync<dynamic>($"/api/TeaBlend/GetGarden?search={search}&pageSize=50");
            return JsonExactOrSessionExpired(r);
        }

        [HttpGet]
        public async Task<ActionResult> GetCategory(string search = "")
        {
            var r = await Services.GetAsync<dynamic>($"/api/TeaBlend/GetCategory?search={search}&pageSize=50");
            return JsonExactOrSessionExpired(r);
        }

        [HttpGet]
        public async Task<ActionResult> GetTransporter(string search = "")
        {
            var r = await Services.GetAsync<dynamic>($"/api/TeaBlend/GetTransporter?search={search}&pageSize=50");
            return JsonExactOrSessionExpired(r);
        }

        [HttpGet]
        public async Task<ActionResult> GetAvailableStock(string blendType, string garden = "", string mark = "", string category = "")
        {
            var r = await Services.GetAsync<dynamic>(
                $"/api/TeaBlend/GetAvailableStock?blendType={blendType}&garden={garden}&mark={mark}&category={category}");
            return JsonExactOrSessionExpired(r);
        }

        [HttpGet]
        public async Task<ActionResult> GenerateDoNo(string blendType, string fyShortFrom, string fyShortTo, string[] whCodes)
        {
            var qs = string.Join("&", (whCodes ?? new string[0]).Select(w => "whCodes=" + Uri.EscapeDataString(w)));
            var r = await Services.GetAsync<dynamic>(
                $"/api/TeaBlend/GenerateDoNo?blendType={blendType}&unit={CurrentUnit}&fyShortFrom={fyShortFrom}&fyShortTo={fyShortTo}&{qs}");
            return JsonExactOrSessionExpired(r);
        }
    }
}
