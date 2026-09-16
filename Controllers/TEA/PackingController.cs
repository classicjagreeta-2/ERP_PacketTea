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

        // Packet Type codes (M_SALETYPE.CODE, TRN_TYPE='P') the current user has rights
        // to, per FACT_JSTIL2027.M_UNIT_USER_RIGHT -- the same rule the Master/Final
        // Blend lists apply, reusing the same API endpoint (the rights table is per user
        // and type, not per screen). Falls back to the full list on any API failure so a
        // transient outage doesn't lock every user out of the screen.
        private async Task<Dictionary<string, string>> GetAllowedBlendTypesAsync()
        {
            var response = await Services.GetAsync<List<string>>("/api/TeaBlend/GetAllowedBlendTypes");
            if (!response.IsSuccessStatusCode || response.Data == null)
                return BlendTypes;

            return BlendTypes.Where(bt => response.Data.Contains(bt.Key))
                              .ToDictionary(bt => bt.Key, bt => bt.Value);
        }

        // Units this user is provisioned for -- backs the list page's "New" inline row
        // Unit picker, same as the Master/Final Blend lists.
        private async Task<List<UnitOption>> GetUnitsForUserAsync()
        {
            var response = await Services.GetAsync<List<UnitOption>>("/api/TeaBlend/GetUnitsForUser");
            return (response.IsSuccessStatusCode ? response.Data : null) ?? new List<UnitOption>();
        }

        // GET: Packing
        public async Task<ActionResult> Index(string blendType, string searchString, int? page = 1, int pageSize = 15, string sortBy = "", string sortDir = "")
        {
            var sdsd = (List<AEDV>)Session["User_AEDV"];
            ViewBag.Permission = sdsd?.FirstOrDefault(l => l.Controller == "PacketTeaPurchaseEntry");
            ViewBag.CurrentFilter = searchString;
            ViewBag.SortBy = sortBy;
            ViewBag.SortDir = sortDir;
            ViewBag.PageSize = pageSize;
            ViewBag.Page = page ?? 1;
            ViewBag.BlendType = blendType;
            ViewBag.BlendTypes = await GetAllowedBlendTypesAsync();
            ViewBag.UnitList = await GetUnitsForUserAsync();

            var response = await Services.GetAsync<PageModel<BLEND_PACKING_DATA>>(
                $"/api/BlendPacking/GetByPage?blendType={blendType}&unit={CurrentUnit}&search={searchString}&page={page}&pageSize={pageSize}&sortBy={sortBy}&sortDir={sortDir}");

            var list = response?.Data?.value?.results ?? new List<BLEND_PACKING_DATA>();
            ViewBag.RowCount = response?.Data?.value?.rowCount ?? 0;

            if (!response?.IsSuccessStatusCode ?? false)
            {
                TempData["toastrError"] = !string.IsNullOrEmpty(response?.Message)
                    ? response.Message
                    : $"Unable to load Packing list (API returned {response?.StatusCode}).";
            }

            return View(list);
        }

        // GET: Packing/InsertOrUpdate
        // `unit` is only populated when this was opened from the list page's "New" inline
        // row -- Unit + Packet Type were already chosen there, so the Unit/Blend Type strip
        // shows them read-only instead of leaving Unit unset (same as Master/Final Blend).
        public async Task<ActionResult> InsertOrUpdate(string docno = "", string docdt = "", string blendType = "", string unit = "", bool view = false)
        {
            var allowedBlendTypes = await GetAllowedBlendTypesAsync();
            ViewBag.BlendTypes = allowedBlendTypes;
            ViewBag.IsView = view;
            ViewBag.LockedFromList = string.IsNullOrEmpty(docno) && !string.IsNullOrEmpty(unit) && !string.IsNullOrEmpty(blendType);

            string fy = Session["SelectedfinancialYear"]?.ToString();
            if (!string.IsNullOrEmpty(fy) && fy.Contains("-"))
            {
                var parts = fy.Split('-');
                string startDigits = new string(parts[0].Where(char.IsDigit).ToArray());
                string endDigits = new string(parts[1].Where(char.IsDigit).ToArray());
                if (startDigits.Length >= 4 && endDigits.Length >= 4)
                {
                    ViewBag.FyStart = new DateTime(int.Parse(startDigits.Substring(startDigits.Length - 4)), 4, 1).ToString("yyyy-MM-dd");
                    ViewBag.FyEnd = new DateTime(int.Parse(endDigits.Substring(endDigits.Length - 4)), 3, 31).ToString("yyyy-MM-dd");
                }
            }

            if (string.IsNullOrEmpty(docno))
            {
                // Default to "PT" only when the user actually has rights to it, else
                // their first allowed type -- so the Unit guess below isn't derived from
                // a Packet Type they cannot select (same as Master/Final Blend Entry).
                var effectiveBlendType = !string.IsNullOrEmpty(blendType) ? blendType :
                    allowedBlendTypes.ContainsKey("PT") ? "PT" :
                    allowedBlendTypes.Keys.FirstOrDefault() ?? "PT";
                var model = new BLEND_PACKING_DATA
                {
                    LOCA = CurrentLoca,
                    GLOCA = CurrentLoca,
                    // Honor the Unit explicitly chosen on the list page's "New" row over
                    // the BlendTypeUnit stopgap guess (see UnitForBlendType's comment).
                    UNIT = !string.IsNullOrEmpty(unit) ? unit : UnitForBlendType(effectiveBlendType),
                    BLEND_TYPE = effectiveBlendType,
                    DOCDT = DateTime.Today,
                    Details = new List<T_BLEND_PACKING>()
                };
                ViewBag.IsEdit = false;
                return View(model);
            }

            var response = await Services.GetAsync<dynamic>(
                $"/api/BlendPacking/GetByDocNo?docno={docno}&docdt={docdt}&blendType={blendType}&unit={CurrentUnit}&forView={view}");

            if (!response.IsSuccessStatusCode || response.Data == null)
            {
                TempData["toastrError"] = response.Message ?? "Record not found.";
                return RedirectToAction("Index", new { blendType });
            }

            var json = JsonConvert.SerializeObject(response.Data);
            var wrapper = JsonConvert.DeserializeObject<GetByDocNoResult>(json);

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
        public async Task<ActionResult> Delete(string docno, string docdt, string blendType)
        {
            var response = await Services.PostAsync<dynamic>(
                $"/api/BlendPacking/Delete?docno={docno}&docdt={docdt}&blendType={blendType}&unit={CurrentUnit}", new { });

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

        // A failed call comes back as Data == null with IsSuccessStatusCode == false;
        // forwarding that as a plain 200 "null" reads to the picker JS as "no matches"
        // with no visible error -- see MasterBlendEntryController for the full story.
        private ActionResult JsonExactOrSessionExpired<T>(ResponseApiModel<T> r)
        {
            if (!r.IsSuccessStatusCode)
            {
                Response.StatusCode = 440; // Login Timeout
                return JsonExact(new { sessionExpired = true, message = "Your session has expired. Please log in again." });
            }
            return JsonExact(r.Data);
        }

        // GET: Packing/GetRowDetail (AJAX, list page's "+" toggle). forView skips the API's
        // locked-period check, which only guards editing -- older documents must still show.
        [HttpGet]
        public async Task<ActionResult> GetRowDetail(string docno = "", string docdt = "", string blendType = "")
        {
            var r = await Services.GetAsync<dynamic>(
                $"/api/BlendPacking/GetByDocNo?docno={docno}&docdt={docdt}&blendType={blendType}&unit={CurrentUnit}&forView=true");
            if (!r.IsSuccessStatusCode || r.Data == null)
                return JsonExact(new { success = false, message = r.Message ?? "Record not found." });

            var wrapper = JsonConvert.DeserializeObject<GetByDocNoResult>(JsonConvert.SerializeObject(r.Data));
            return JsonExact(new { success = true, details = wrapper?.details ?? new List<T_BLEND_PACKING>() });
        }

        [HttpGet]
        public async Task<ActionResult> GetParty(string search = "")
        {
            var r = await Services.GetAsync<dynamic>($"/api/TeaBlend/GetParty?search={Uri.EscapeDataString(search ?? "")}&pageSize=50");
            return JsonExactOrSessionExpired(r);
        }

        [HttpGet]
        public async Task<ActionResult> GetMark(string search = "")
        {
            var r = await Services.GetAsync<dynamic>($"/api/TeaBlend/GetMark?search={search}&pageSize=50");
            return JsonExactOrSessionExpired(r);
        }

        // Backs the grid's Category column: the VB form resolved a row's category from
        // its grade's GRADE_TYPE, but a user can also override it per row from here.
        [HttpGet]
        public async Task<ActionResult> GetCategory(string search = "")
        {
            var r = await Services.GetAsync<dynamic>($"/api/TeaBlend/GetCategory?search={search}&pageSize=50");
            return JsonExactOrSessionExpired(r);
        }

        [HttpGet]
        public async Task<ActionResult> GetBlendGrade(string search = "")
        {
            var r = await Services.GetAsync<dynamic>($"/api/TeaBlend/GetBlendGrade?search={search}&pageSize=50");
            return JsonExactOrSessionExpired(r);
        }

        [HttpGet]
        public async Task<ActionResult> GetAllocation(string search = "")
        {
            var r = await Services.GetAsync<dynamic>($"/api/TeaBlend/GetAllocation?search={search}&pageSize=50");
            return JsonExactOrSessionExpired(r);
        }

        [HttpGet]
        public async Task<ActionResult> GetSalesCentre(string search = "")
        {
            var r = await Services.GetAsync<dynamic>($"/api/BlendPacking/GetSalesCentre?search={search}&pageSize=50");
            return JsonExactOrSessionExpired(r);
        }

        [HttpGet]
        public async Task<ActionResult> GetChestSize(string search = "")
        {
            var r = await Services.GetAsync<dynamic>($"/api/BlendPacking/GetChestSize?search={search}&pageSize=50");
            return JsonExactOrSessionExpired(r);
        }

        [HttpGet]
        public async Task<ActionResult> GetFinalBlendList(string blendType = "", string search = "")
        {
            var r = await Services.GetAsync<dynamic>(
                $"/api/BlendPacking/GetFinalBlendList?blendType={blendType}&unit={CurrentUnit}&search={search}");
            return JsonExactOrSessionExpired(r);
        }

        [HttpGet]
        public async Task<ActionResult> GetFinalBlendRowValues(string docno, string docdt, string blendType, string excludeDocNo = "", string excludeDocDt = "")
        {
            var r = await Services.GetAsync<dynamic>(
                $"/api/BlendPacking/GetFinalBlendRowValues?docno={docno}&docdt={docdt}&blendType={blendType}&excludeDocNo={excludeDocNo}&excludeDocDt={excludeDocDt}");
            return JsonExactOrSessionExpired(r);
        }
    }
}
