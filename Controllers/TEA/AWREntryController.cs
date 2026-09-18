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
    // "AWR Entry" -- ported from VB6 TRN_AWR.frm, Packet Tea (PT) transaction
    // sub-type only. Modeled 1:1 on MasterBlendEntryController -- see that
    // class's comments for the rationale behind the rights-filtered type list,
    // the AWRTypeUnit stopgap, and the session-expiry-surfacing lookups.
    //
    // Scope note (agreed plan, Phase 1): Damage/Shortage and Package Detail
    // sub-grids, and every JST-only field, are intentionally not exposed here.
    public class AWREntryController : Controller
    {
        // Same value domain as MasterBlendEntryController.BlendTypes -- the user
        // confirmed AWR Type reuses it exactly, just under a different label.
        public static readonly Dictionary<string, string> AWRTypes = new Dictionary<string, string>
        {
            { "PT", "Packet Tea" },
            { "TT", "Tea Trading" },
            { "WT", "Web Tea" },
            { "BT", "Bagicha Tea" },
            { "ST", "Sale Tea" },
            { "TB", "Birla Tea" }
        };

        private static readonly Dictionary<string, string> AWRTypeUnit = new Dictionary<string, string>
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
        private string UnitForAWRType(string awrType) =>
            !string.IsNullOrEmpty(CurrentUnit) ? CurrentUnit :
            (awrType != null && AWRTypeUnit.TryGetValue(awrType, out var u) ? u : "");

        private async Task<Dictionary<string, string>> GetAllowedAWRTypesAsync()
        {
            var response = await Services.GetAsync<List<string>>("/api/TeaAWR/GetAllowedAWRTypes");
            if (!response.IsSuccessStatusCode || response.Data == null)
                return AWRTypes;
            return AWRTypes.Where(t => response.Data.Contains(t.Key))
                            .ToDictionary(t => t.Key, t => t.Value);
        }

        private async Task<List<UnitOption>> GetUnitsForUserAsync()
        {
            var response = await Services.GetAsync<List<UnitOption>>("/api/TeaAWR/GetUnitsForUser");
            return (response.IsSuccessStatusCode ? response.Data : null) ?? new List<UnitOption>();
        }

        // GET: AWREntry
        public async Task<ActionResult> Index(string awrType, string searchString, int? page = 1, int pageSize = 15, string sortBy = "", string sortDir = "")
        {
            var sdsd = (List<AEDV>)Session["User_AEDV"];
            ViewBag.Permission = sdsd?.FirstOrDefault(l => l.Controller == "PacketTeaPurchaseEntry");
            ViewBag.CurrentFilter = searchString;
            ViewBag.SortBy = sortBy;
            ViewBag.SortDir = sortDir;
            ViewBag.PageSize = pageSize;
            ViewBag.Page = page ?? 1;
            ViewBag.AWRType = awrType;
            ViewBag.AWRTypes = await GetAllowedAWRTypesAsync();
            ViewBag.UnitList = await GetUnitsForUserAsync();

            var response = await Services.GetAsync<PageModel<dynamic>>(
                $"/api/TeaAWR/GetByPage?awrType={awrType}&unit={CurrentUnit}&search={searchString}&page={page}&pageSize={pageSize}&sortBy={sortBy}&sortDir={sortDir}");

            var json = response?.Data != null ? JsonConvert.SerializeObject(response.Data) : null;
            var wrapper = json != null ? JsonConvert.DeserializeObject<GetByPageResult>(json) : null;
            var list = wrapper?.value?.results ?? new List<AWRDocRow>();
            ViewBag.RowCount = wrapper?.value?.rowCount ?? 0;

            if (!response?.IsSuccessStatusCode ?? false)
            {
                TempData["toastrError"] = !string.IsNullOrEmpty(response?.Message)
                    ? response.Message
                    : $"Unable to load AWR Entry list (API returned {response?.StatusCode}).";
            }

            return View(list);
        }

        private class GetByPageResult { public GetByPageValue value { get; set; } }
        private class GetByPageValue { public List<AWRDocRow> results { get; set; } public int rowCount { get; set; } }

        // GET: AWREntry/InsertOrUpdate
        public async Task<ActionResult> InsertOrUpdate(string docno = "", string awrDate = "", string awrType = "", string unit = "", bool view = false)
        {
            var allowedAWRTypes = await GetAllowedAWRTypesAsync();
            ViewBag.AWRTypes = allowedAWRTypes;
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
                var effectiveAWRType = !string.IsNullOrEmpty(awrType) ? awrType :
                    allowedAWRTypes.ContainsKey("PT") ? "PT" :
                    allowedAWRTypes.Keys.FirstOrDefault() ?? "PT";
                var model = new T_AWR_PT_DATA
                {
                    T_AWR = new List<T_AWR>()
                };
                ViewBag.IsEdit = false;
                ViewBag.NewUnit = !string.IsNullOrEmpty(unit) ? unit : UnitForAWRType(effectiveAWRType);
                ViewBag.NewAWRType = effectiveAWRType;
                return View(model);
            }

            var response = await Services.GetAsync<dynamic>(
                $"/api/TeaAWR/GetByDocNo?docno={docno}&awrDate={awrDate}&awrType={awrType}&unit={CurrentUnit}");

            if (!response.IsSuccessStatusCode || response.Data == null)
            {
                TempData["toastrError"] = response.Message ?? "Record not found.";
                return RedirectToAction("Index", new { awrType });
            }

            var json = JsonConvert.SerializeObject(response.Data);
            var wrapper = JsonConvert.DeserializeObject<GetByDocNoResult>(json);

            var editModel = new T_AWR_PT_DATA { T_AWR = wrapper.rows ?? new List<T_AWR>() };
            ViewBag.IsEdit = true;
            ViewBag.NewUnit = editModel.T_AWR.FirstOrDefault()?.UNIT;
            ViewBag.NewAWRType = editModel.T_AWR.FirstOrDefault()?.TRAN_TYPE ?? awrType;
            return View(editModel);
        }

        private class GetByDocNoResult { public List<T_AWR> rows { get; set; } }

        // GET: AWREntry/GetRowDetail (AJAX, fired by the Index list's "+" toggle)
        [HttpGet]
        public async Task<ActionResult> GetRowDetail(string docno = "", string awrDate = "", string awrType = "")
        {
            var r = await Services.GetAsync<dynamic>(
                $"/api/TeaAWR/GetByDocNo?docno={docno}&awrDate={awrDate}&awrType={awrType}&unit={CurrentUnit}");
            if (!r.IsSuccessStatusCode || r.Data == null)
                return JsonExact(new { success = false, message = r.Message ?? "Record not found." });

            var obj = (Newtonsoft.Json.Linq.JObject)r.Data;
            obj["success"] = true;
            return JsonExact(obj);
        }

        // POST: AWREntry/Save
        [HttpPost]
        public async Task<JsonResult> Save(T_AWR_PT_DATA model)
        {
            // LOCA is never posted by the client (InsertOrUpdate.cshtml's payload builder
            // doesn't collect it -- there's no form field for it), so every row arrived with
            // LOCA null. The legacy VB6 form always supplied it on every insert, and every
            // existing T_AWR row has it set (all "JST" in this company's data), so stamp it
            // here from the session -- same source (SessionHelper.GetUser().Loca) several
            // other PT models already default to -- rather than trusting/requiring the client.
            foreach (var r in model?.T_AWR ?? new List<T_AWR>())
                r.LOCA = CurrentLoca;

            var response = await Services.PostAsync<dynamic>("/api/TeaAWR/SaveOrUpdate", model);
            return Json(new
            {
                success = response.IsSuccessStatusCode,
                message = response.IsSuccessStatusCode ? "Saved successfully." : (response.Message ?? "Save failed."),
                data = response.Data
            });
        }

        // POST: AWREntry/Delete
        [HttpPost]
        public async Task<ActionResult> Delete(string docno, string awrDate, string awrType)
        {
            var response = await Services.PostAsync<dynamic>(
                $"/api/TeaAWR/Delete?docno={docno}&awrDate={awrDate}&awrType={awrType}&unit={CurrentUnit}", new { });

            TempData[response.IsSuccessStatusCode ? "toastrSuccess" : "toastrError"] =
                response.IsSuccessStatusCode ? "Deleted successfully." : (response.Message ?? "Delete failed.");

            return RedirectToAction("Index", new { awrType });
        }

        // =====================================================================
        // AJAX lookup passthroughs -- same JsonExact/JsonExactOrSessionExpired
        // rationale as MasterBlendEntryController (see its comments).
        // =====================================================================
        private ActionResult JsonExact(object data) =>
            Content(JsonConvert.SerializeObject(data), "application/json");

        // Only a real 401 from the API means the session actually expired -- ResponseApiModel.StatusCode
        // is the raw HttpStatusCode name (e.g. "Unauthorized", "InternalServerError"). Every other failure
        // (a genuine bug/exception in the lookup endpoint, a bad schema, etc.) used to be reported as
        // "session expired" too, which sent users off to re-login for problems that had nothing to do with
        // their session and hid the real error. Now only 401 gets that message; anything else surfaces the
        // API's actual message so it can be diagnosed.
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
        public async Task<ActionResult> GetWarehouse(string search = "")
        {
            var r = await Services.GetAsync<dynamic>($"/api/TeaAWR/GetWarehouse?search={search}&pageSize=50");
            return JsonExactOrSessionExpired(r);
        }

        [HttpGet]
        public async Task<ActionResult> GetSalesCentre(string search = "")
        {
            var r = await Services.GetAsync<dynamic>($"/api/TeaAWR/GetSalesCentre?search={search}&pageSize=50");
            return JsonExactOrSessionExpired(r);
        }

        [HttpGet]
        public async Task<ActionResult> GetDespatchData(string awrType, string mode = "Purchase")
        {
            var r = await Services.GetAsync<dynamic>($"/api/TeaAWR/GetDespatchData?awrType={awrType}&mode={mode}");
            return JsonExactOrSessionExpired(r);
        }
    }

    // Small view-model for the list page (mirrors TeaAWRController.GetByPage's
    // anonymous document-summary shape) -- kept here rather than under
    // Models\PT since it's purely a list-page projection, not a saved entity.
    public class AWRDocRow
    {
        public string docno { get; set; }
        public string awrNo { get; set; }
        public DateTime awrDate { get; set; }
        public DateTime? arrivalDate { get; set; }
        public string warehouse { get; set; }
        public string warehouseName { get; set; }
        public string saleCentre { get; set; }
        public string saleCentreName { get; set; }
        public string unit { get; set; }
        public string tranType { get; set; }
        public decimal totalBagChest { get; set; }
        public decimal totalNetWt { get; set; }
        public decimal totalBagRcvd { get; set; }
    }
}
