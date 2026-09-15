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

        // User.UnitList for the PacketTea module (see JwtMiddleware.cs) -- backs the list
        // page's "New" inline row Unit picker, same as MasterBlendEntryController (reuses
        // the same generic, module-scoped API endpoint rather than duplicating it).
        private async Task<List<UnitOption>> GetUnitsForUserAsync()
        {
            var response = await Services.GetAsync<List<UnitOption>>("/api/TeaBlend/GetUnitsForUser");
            return (response.IsSuccessStatusCode ? response.Data : null) ?? new List<UnitOption>();
        }

        // GET: FinalBlendEntry
        public async Task<ActionResult> Index(string blendType, string searchString, int? page = 1, int pageSize = 15)
        {
            var sdsd = (List<AEDV>)Session["User_AEDV"];
            ViewBag.Permission = sdsd?.FirstOrDefault(l => l.Controller == "PacketTeaPurchaseEntry");
            ViewBag.CurrentFilter = searchString;
            ViewBag.PageSize = pageSize;
            ViewBag.Page = page ?? 1;
            ViewBag.BlendType = blendType;
            ViewBag.BlendTypes = BlendTypes;
            ViewBag.UnitList = await GetUnitsForUserAsync();

            var response = await Services.GetAsync<PageModel<T_TEA_BLEND>>(
                $"/api/FinalBlend/GetByPage?blendType={blendType}&unit={CurrentUnit}&search={searchString}&page={page}&pageSize={pageSize}");

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
            ViewBag.BlendTypes = BlendTypes;
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

            if (string.IsNullOrEmpty(docno))
            {
                // New entry -- Master Blend, and everything it carries, is picked
                // on-screen (spec §3.5.1); nothing to clone until then.
                var effectiveBlendType = string.IsNullOrEmpty(blendType) ? "PT" : blendType;
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
                $"/api/FinalBlend/GetByDocNo?docno={docno}&docdt={docdt}&blendType={blendType}&unit={CurrentUnit}");

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

            var response = await Services.PostAsync<dynamic>("/api/FinalBlend/SaveOrUpdate", model);
            return Json(new
            {
                success = response.IsSuccessStatusCode,
                message = response.IsSuccessStatusCode ? "Saved successfully." : (response.Message ?? "Save failed."),
                data = response.Data
            });
        }

        // POST: FinalBlendEntry/Delete
        [HttpPost]
        public async Task<ActionResult> Delete(string docno, string docdt, string blendType)
        {
            var response = await Services.PostAsync<dynamic>(
                $"/api/FinalBlend/Delete?docno={docno}&docdt={docdt}&blendType={blendType}&unit={CurrentUnit}", new { });

            TempData[response.IsSuccessStatusCode ? "toastrSuccess" : "toastrError"] =
                response.IsSuccessStatusCode ? "Deleted successfully." : (response.Message ?? "Delete failed.");

            return RedirectToAction("Index", new { blendType });
        }

        // GET: FinalBlendEntry/GetRowDetail (AJAX, Index list's "+" toggle)
        [HttpGet]
        public async Task<ActionResult> GetRowDetail(string docno = "", string docdt = "", string blendType = "")
        {
            var r = await Services.GetAsync<dynamic>(
                $"/api/FinalBlend/GetRowDetail?docno={docno}&docdt={docdt}&blendType={blendType}&unit={CurrentUnit}");
            if (!r.IsSuccessStatusCode || r.Data == null)
                return JsonExact(new { success = false, message = r.Message ?? "Record not found." });

            var obj = (Newtonsoft.Json.Linq.JObject)r.Data;
            obj["success"] = true;

            var detResp = await Services.GetAsync<dynamic>(
                $"/api/FinalBlend/GetByDocNo?docno={docno}&docdt={docdt}&blendType={blendType}&unit={CurrentUnit}");
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

        [HttpGet]
        public async Task<ActionResult> GetMasterBlendList(string blendType = "", string search = "")
        {
            var r = await Services.GetAsync<dynamic>(
                $"/api/FinalBlend/GetMasterBlendList?blendType={blendType}&unit={CurrentUnit}&search={search}");
            return JsonExact(r.Data);
        }

        [HttpGet]
        public async Task<ActionResult> GetMasterBlendDetail(string docno, string docdt)
        {
            var r = await Services.GetAsync<dynamic>(
                $"/api/FinalBlend/GetMasterBlendDetail?docno={docno}&docdt={docdt}&unit={CurrentUnit}");
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
            return JsonExact(r.Data);
        }

        [HttpGet]
        public async Task<ActionResult> GetWarehouse(string search = "")
        {
            var r = await Services.GetAsync<dynamic>($"/api/TeaBlend/GetWarehouse?search={search}&pageSize=50");
            return JsonExact(r.Data);
        }

        [HttpGet]
        public async Task<ActionResult> GetAllocation(string search = "")
        {
            var r = await Services.GetAsync<dynamic>($"/api/TeaBlend/GetAllocation?search={search}&pageSize=50");
            return JsonExact(r.Data);
        }

        [HttpGet]
        public async Task<ActionResult> GetBlendGrade(string search = "")
        {
            var r = await Services.GetAsync<dynamic>($"/api/TeaBlend/GetBlendGrade?search={search}&pageSize=50");
            return JsonExact(r.Data);
        }

        [HttpGet]
        public async Task<ActionResult> GetMark(string search = "")
        {
            var r = await Services.GetAsync<dynamic>($"/api/TeaBlend/GetMark?search={search}&pageSize=50");
            return JsonExact(r.Data);
        }

        [HttpGet]
        public async Task<ActionResult> GetTransporter(string search = "")
        {
            var r = await Services.GetAsync<dynamic>($"/api/TeaBlend/GetTransporter?search={search}&pageSize=50");
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
