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

        // GET: Packing
        public async Task<ActionResult> Index(string blendType, string searchString, int? page = 1, int pageSize = 15)
        {
            var sdsd = (List<AEDV>)Session["User_AEDV"];
            ViewBag.Permission = sdsd?.FirstOrDefault(l => l.Controller == "PacketTeaPurchaseEntry");
            ViewBag.CurrentFilter = searchString;
            ViewBag.PageSize = pageSize;
            ViewBag.Page = page ?? 1;
            ViewBag.BlendType = blendType;
            ViewBag.BlendTypes = BlendTypes;

            var response = await Services.GetAsync<PageModel<BLEND_PACKING_DATA>>(
                $"/api/BlendPacking/GetByPage?blendType={blendType}&unit={CurrentUnit}&search={searchString}&page={page}&pageSize={pageSize}");

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
        public async Task<ActionResult> InsertOrUpdate(string docno = "", string docdt = "", string blendType = "")
        {
            ViewBag.BlendTypes = BlendTypes;

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
                var effectiveBlendType = string.IsNullOrEmpty(blendType) ? "PT" : blendType;
                var model = new BLEND_PACKING_DATA
                {
                    LOCA = CurrentLoca,
                    GLOCA = CurrentLoca,
                    UNIT = UnitForBlendType(effectiveBlendType),
                    BLEND_TYPE = effectiveBlendType,
                    DOCDT = DateTime.Today,
                    Details = new List<T_BLEND_PACKING>()
                };
                ViewBag.IsEdit = false;
                return View(model);
            }

            var response = await Services.GetAsync<dynamic>(
                $"/api/BlendPacking/GetByDocNo?docno={docno}&docdt={docdt}&blendType={blendType}&unit={CurrentUnit}");

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

        [HttpGet]
        public async Task<ActionResult> GetMark(string search = "")
        {
            var r = await Services.GetAsync<dynamic>($"/api/TeaBlend/GetMark?search={search}&pageSize=50");
            return JsonExact(r.Data);
        }

        [HttpGet]
        public async Task<ActionResult> GetBlendGrade(string search = "")
        {
            var r = await Services.GetAsync<dynamic>($"/api/TeaBlend/GetBlendGrade?search={search}&pageSize=50");
            return JsonExact(r.Data);
        }

        [HttpGet]
        public async Task<ActionResult> GetAllocation(string search = "")
        {
            var r = await Services.GetAsync<dynamic>($"/api/TeaBlend/GetAllocation?search={search}&pageSize=50");
            return JsonExact(r.Data);
        }

        [HttpGet]
        public async Task<ActionResult> GetSalesCentre(string search = "")
        {
            var r = await Services.GetAsync<dynamic>($"/api/BlendPacking/GetSalesCentre?search={search}&pageSize=50");
            return JsonExact(r.Data);
        }

        [HttpGet]
        public async Task<ActionResult> GetChestSize(string search = "")
        {
            var r = await Services.GetAsync<dynamic>($"/api/BlendPacking/GetChestSize?search={search}&pageSize=50");
            return JsonExact(r.Data);
        }

        [HttpGet]
        public async Task<ActionResult> GetFinalBlendList(string blendType = "", string search = "")
        {
            var r = await Services.GetAsync<dynamic>(
                $"/api/BlendPacking/GetFinalBlendList?blendType={blendType}&unit={CurrentUnit}&search={search}");
            return JsonExact(r.Data);
        }

        [HttpGet]
        public async Task<ActionResult> GetFinalBlendRowValues(string docno, string docdt, string blendType, string excludeDocNo = "", string excludeDocDt = "")
        {
            var r = await Services.GetAsync<dynamic>(
                $"/api/BlendPacking/GetFinalBlendRowValues?docno={docno}&docdt={docdt}&blendType={blendType}&excludeDocNo={excludeDocNo}&excludeDocDt={excludeDocDt}");
            return JsonExact(r.Data);
        }
    }
}
