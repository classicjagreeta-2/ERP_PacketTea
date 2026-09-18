using Newtonsoft.Json;
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
    // Tea (PT) transaction sub-type only. Modeled 1:1 on AwrEntryController /
    // MasterBlendEntryController.
    public class TeaSampleDrawEntryController : Controller
    {
        public static readonly Dictionary<string, string> AwrTypes = AwrEntryController.AwrTypes;

        private static readonly Dictionary<string, string> AwrTypeUnit = new Dictionary<string, string>
        {
            { "PT", "GORA" }, { "TT", "JSTI" }, { "WT", "JSTI" }, { "BT", "BGCH" }, { "ST", "TTSI" }, { "TB", "JSTI" },
        };

        private string CurrentUnit => SessionHelper.GetUser()?.CurentUnit ?? "";
        private string CurrentLoca => SessionHelper.GetUser()?.Loca ?? "";
        private string UnitForAwrType(string awrType) =>
            !string.IsNullOrEmpty(CurrentUnit) ? CurrentUnit :
            (awrType != null && AwrTypeUnit.TryGetValue(awrType, out var u) ? u : "");

        private async Task<Dictionary<string, string>> GetAllowedAwrTypesAsync()
        {
            var response = await Services.GetAsync<List<string>>("/api/TeaSampleDraw/GetAllowedAwrTypes");
            if (!response.IsSuccessStatusCode || response.Data == null)
                return AwrTypes;
            return AwrTypes.Where(t => response.Data.Contains(t.Key))
                            .ToDictionary(t => t.Key, t => t.Value);
        }

        // Same pattern as AwrEntryController.GetUnitsForUserAsync -- backs the list
        // page's "New" inline row Unit picker.
        private async Task<List<UnitOption>> GetUnitsForUserAsync()
        {
            var response = await Services.GetAsync<List<UnitOption>>("/api/TeaSampleDraw/GetUnitsForUser");
            return (response.IsSuccessStatusCode ? response.Data : null) ?? new List<UnitOption>();
        }

        // GET: TeaSampleDrawEntry
        public async Task<ActionResult> Index(string awrType, string searchString, int? page = 1, int pageSize = 15, string sortBy = "", string sortDir = "")
        {
            var sdsd = (List<AEDV>)Session["User_AEDV"];
            ViewBag.Permission = sdsd?.FirstOrDefault(l => l.Controller == "PacketTeaPurchaseEntry");
            ViewBag.CurrentFilter = searchString;
            ViewBag.SortBy = sortBy;
            ViewBag.SortDir = sortDir;
            ViewBag.PageSize = pageSize;
            ViewBag.Page = page ?? 1;
            ViewBag.AwrType = awrType;
            ViewBag.AwrTypes = await GetAllowedAwrTypesAsync();
            ViewBag.UnitList = await GetUnitsForUserAsync();

            var response = await Services.GetAsync<PageModel<dynamic>>(
                $"/api/TeaSampleDraw/GetByPage?awrType={awrType}&unit={CurrentUnit}&search={searchString}&page={page}&pageSize={pageSize}&sortBy={sortBy}&sortDir={sortDir}");

            var json = response?.Data != null ? JsonConvert.SerializeObject(response.Data) : null;
            var wrapper = json != null ? JsonConvert.DeserializeObject<GetByPageResult>(json) : null;
            var list = wrapper?.value?.results ?? new List<SampleDrawDocRow>();
            ViewBag.RowCount = wrapper?.value?.rowCount ?? 0;

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
        public async Task<ActionResult> InsertOrUpdate(string docno = "", string docdt = "", string awrType = "", string unit = "", bool view = false)
        {
            var allowedAwrTypes = await GetAllowedAwrTypesAsync();
            ViewBag.AwrTypes = allowedAwrTypes;
            ViewBag.LockedFromList = string.IsNullOrEmpty(docno) && !string.IsNullOrEmpty(unit) && !string.IsNullOrEmpty(awrType);
            ViewBag.IsView = view;

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
                var effectiveAwrType = !string.IsNullOrEmpty(awrType) ? awrType :
                    allowedAwrTypes.ContainsKey("PT") ? "PT" :
                    allowedAwrTypes.Keys.FirstOrDefault() ?? "PT";
                var model = new T_SAMPLE_DRAW_PT_DATA { T_SAMPLE_DRAW = new List<T_SAMPLE_DRAW>() };
                ViewBag.IsEdit = false;
                ViewBag.NewUnit = !string.IsNullOrEmpty(unit) ? unit : UnitForAwrType(effectiveAwrType);
                ViewBag.NewAwrType = effectiveAwrType;
                return View(model);
            }

            var response = await Services.GetAsync<dynamic>(
                $"/api/TeaSampleDraw/GetByDocNo?docno={docno}&docdt={docdt}&awrType={awrType}&unit={CurrentUnit}");

            if (!response.IsSuccessStatusCode || response.Data == null)
            {
                TempData["toastrError"] = response.Message ?? "Record not found.";
                return RedirectToAction("Index", new { awrType });
            }

            var json = JsonConvert.SerializeObject(response.Data);
            var wrapper = JsonConvert.DeserializeObject<GetByDocNoResult>(json);

            var editModel = new T_SAMPLE_DRAW_PT_DATA { T_SAMPLE_DRAW = wrapper.rows ?? new List<T_SAMPLE_DRAW>() };
            ViewBag.IsEdit = true;
            ViewBag.NewUnit = editModel.T_SAMPLE_DRAW.FirstOrDefault()?.UNIT;
            ViewBag.NewAwrType = editModel.T_SAMPLE_DRAW.FirstOrDefault()?.TRAN_CODE ?? awrType;
            return View(editModel);
        }

        private class GetByDocNoResult { public List<T_SAMPLE_DRAW> rows { get; set; } }

        // GET: TeaSampleDrawEntry/GetRowDetail (AJAX, fired by the Index list's "+" toggle)
        [HttpGet]
        public async Task<ActionResult> GetRowDetail(string docno = "", string docdt = "", string awrType = "")
        {
            var r = await Services.GetAsync<dynamic>(
                $"/api/TeaSampleDraw/GetByDocNo?docno={docno}&docdt={docdt}&awrType={awrType}&unit={CurrentUnit}");
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
        public async Task<ActionResult> Delete(string docno, string docdt, string awrType)
        {
            var response = await Services.PostAsync<dynamic>(
                $"/api/TeaSampleDraw/Delete?docno={docno}&docdt={docdt}&awrType={awrType}&unit={CurrentUnit}", new { });

            TempData[response.IsSuccessStatusCode ? "toastrSuccess" : "toastrError"] =
                response.IsSuccessStatusCode ? "Deleted successfully." : (response.Message ?? "Delete failed.");

            return RedirectToAction("Index", new { awrType });
        }

        // =====================================================================
        // AJAX lookup passthroughs
        // =====================================================================
        private ActionResult JsonExact(object data) =>
            Content(JsonConvert.SerializeObject(data), "application/json");

        // Same fix as AwrEntryController.JsonExactOrSessionExpired -- only a real 401 from the
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

        [HttpGet]
        public async Task<ActionResult> GetSalesCentre(string search = "")
        {
            var r = await Services.GetAsync<dynamic>($"/api/TeaSampleDraw/GetSalesCentre?search={search}&pageSize=50");
            return JsonExactOrSessionExpired(r);
        }

        [HttpGet]
        public async Task<ActionResult> GetBroker(string search = "")
        {
            var r = await Services.GetAsync<dynamic>($"/api/TeaSampleDraw/GetBroker?search={search}&pageSize=50");
            return JsonExactOrSessionExpired(r);
        }

        [HttpGet]
        public async Task<ActionResult> GetParty(string search = "")
        {
            var r = await Services.GetAsync<dynamic>($"/api/TeaSampleDraw/GetParty?search={search}&pageSize=50");
            return JsonExactOrSessionExpired(r);
        }

        [HttpGet]
        public async Task<ActionResult> GetWarehouse(string search = "")
        {
            var r = await Services.GetAsync<dynamic>($"/api/TeaSampleDraw/GetWarehouse?search={search}&pageSize=50");
            return JsonExactOrSessionExpired(r);
        }

        [HttpGet]
        public async Task<ActionResult> GetAvailableAwrStock(string awrType, string mark = "", string grade = "")
        {
            var r = await Services.GetAsync<dynamic>($"/api/TeaSampleDraw/GetAvailableAwrStock?awrType={awrType}&mark={mark}&grade={grade}");
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
