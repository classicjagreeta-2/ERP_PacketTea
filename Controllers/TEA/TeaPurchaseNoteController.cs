using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PacketTea.Models;
using PacketTea.Models.PT;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Finance.Controllers.TEA
{
    // "Tea Procurement - Tea Purchase Debit Note / Credit Note" -- ported from VB6
    // trn_credit_note.frm (INVTYPE 2/3, T_DNCN_HED.TRAN_CODE 36/37). Two menus, one
    // screen: DebitNote (36) and CreditNote (37). List and entry screen follow Master
    // Blend Entry: the list's "New" row picks Unit + Type (purchase type), which are
    // then locked on the entry screen. Data lives in the factory schema, so every call
    // goes through Services.GetAsync/PostAsync like AWR / Sample Draw.
    public class TeaPurchaseNoteController : Controller
    {
        private static readonly Dictionary<string, string> TranNames = new Dictionary<string, string>
        {
            { "36", "Tea Purchase Debit Note" },
            { "37", "Tea Purchase Credit Note" },
        };
        private static readonly Dictionary<string, string> TranActions = new Dictionary<string, string>
        {
            { "36", "DebitNote" },
            { "37", "CreditNote" },
        };

        private static string TranOf(string tran) => tran == "37" ? "37" : "36";
        private static string E(string s) => Uri.EscapeDataString(s ?? "");
        private const string Api = "/api/TeaPurchaseNote/";

        private async Task<Dictionary<string, string>> GetAllowedTypesAsync()
        {
            var r = await Services.GetAsync<Dictionary<string, string>>(Api + "GetAllowedTypes");
            return (r.IsSuccessStatusCode ? r.Data : null) ?? new Dictionary<string, string>();
        }

        private async Task<List<UnitOption>> GetUnitsForUserAsync()
        {
            var r = await Services.GetAsync<List<UnitOption>>(Api + "GetUnitsForUser");
            return (r.IsSuccessStatusCode ? r.Data : null) ?? new List<UnitOption>();
        }

        public ActionResult Index() => RedirectToAction("DebitNote");

        // GET: TeaPurchaseNote/DebitNote (menu "Tea Purchase Debit Note")
        public Task<ActionResult> DebitNote(string type, string searchString, int? page = 1, int pageSize = 15, string sortBy = "", string sortDir = "")
            => List("36", type, searchString, page, pageSize, sortBy, sortDir);

        // GET: TeaPurchaseNote/CreditNote (menu "Tea Purchase Credit Note")
        public Task<ActionResult> CreditNote(string type, string searchString, int? page = 1, int pageSize = 15, string sortBy = "", string sortDir = "")
            => List("37", type, searchString, page, pageSize, sortBy, sortDir);

        private async Task<ActionResult> List(string tran, string type, string searchString, int? page, int pageSize, string sortBy, string sortDir)
        {
            ViewBag.Tran = tran;
            ViewBag.TranName = TranNames[tran];
            ViewBag.ListAction = TranActions[tran];
            ViewBag.CurrentFilter = searchString;
            ViewBag.SortBy = sortBy;
            ViewBag.SortDir = sortDir;
            ViewBag.PageSize = pageSize;
            ViewBag.Page = page ?? 1;
            ViewBag.Type = type;
            ViewBag.Types = await GetAllowedTypesAsync();
            ViewBag.UnitList = await GetUnitsForUserAsync();

            var response = await Services.GetAsync<JObject>(
                $"{Api}GetByPage?tran={tran}&type={E(type)}&search={E(searchString)}&page={page ?? 1}&pageSize={pageSize}&sortBy={E(sortBy)}&sortDir={E(sortDir)}");

            var value = response?.Data?["value"];
            var list = value?["results"]?.ToObject<List<PurNoteRow>>() ?? new List<PurNoteRow>();
            ViewBag.RowCount = value?["rowCount"]?.Value<int>() ?? 0;

            if (!(response?.IsSuccessStatusCode ?? false))
            {
                TempData["toastrError"] = !string.IsNullOrEmpty(response?.Message)
                    ? response.Message
                    : $"Unable to load the {TranNames[tran]} list (API returned {response?.StatusCode}).";
            }
            return View("Index", list);
        }

        // GET: TeaPurchaseNote/InsertOrUpdate
        // New: `unit` + `type` come from the list's "New" row and are locked here.
        // Edit/View: the note's key (unit, docno, docdt).
        public async Task<ActionResult> InsertOrUpdate(string tran = "36", string unit = "", string type = "", string docno = "", string docdt = "", bool view = false)
        {
            tran = TranOf(tran);
            ViewBag.Tran = tran;
            ViewBag.TranName = TranNames[tran];
            ViewBag.ListAction = TranActions[tran];
            ViewBag.IsView = view;

            if (string.IsNullOrEmpty(docno))
            {
                if (string.IsNullOrEmpty(unit) || string.IsNullOrEmpty(type))
                {
                    TempData["toastrError"] = "Select Unit and Type from the New row first.";
                    return RedirectToAction(TranActions[tran]);
                }
                var ctx = await Services.GetAsync<JObject>($"{Api}GetNewContext?tran={tran}&unit={E(unit)}&type={E(type)}");
                if (!ctx.IsSuccessStatusCode || ctx.Data == null)
                {
                    TempData["toastrError"] = ctx.Message ?? "Unable to open a new note.";
                    return RedirectToAction(TranActions[tran]);
                }
                ViewBag.IsEdit = false;
                ViewBag.DocJson = JsonConvert.SerializeObject(new { head = ctx.Data, lines = new object[0] });
                return View();
            }

            var r = await Services.GetAsync<JObject>($"{Api}GetByDocNo?tran={tran}&unit={E(unit)}&docno={E(docno)}&docdt={E(docdt)}");
            if (!r.IsSuccessStatusCode || r.Data == null)
            {
                TempData["toastrError"] = r.Message ?? "Record not found.";
                return RedirectToAction(TranActions[tran]);
            }
            ViewBag.IsEdit = true;
            ViewBag.DocJson = JsonConvert.SerializeObject(r.Data);
            return View();
        }

        // POST: TeaPurchaseNote/Save (JSON body). The API recomputes every figure and
        // posts the accounts voucher. dryRun=true runs it all and rolls back.
        [HttpPost]
        public async Task<ActionResult> Save(bool dryRun = false)
        {
            T_PUR_NOTE_DATA model;
            try
            {
                Request.InputStream.Position = 0;
                using (var reader = new StreamReader(Request.InputStream))
                    model = JsonConvert.DeserializeObject<T_PUR_NOTE_DATA>(reader.ReadToEnd());
            }
            catch (Exception ex)
            {
                return JsonExact(new { success = false, message = "Invalid data: " + ex.Message });
            }
            if (model == null) return JsonExact(new { success = false, message = "Nothing to save." });

            var r = await Services.PostAsync<JObject>(Api + "SaveOrUpdate" + (dryRun ? "?dryRun=true" : ""), model);
            return JsonExact(new
            {
                success = r.IsSuccessStatusCode,
                message = r.IsSuccessStatusCode ? "Saved successfully." : (r.Message ?? "Save failed."),
                data = r.Data
            });
        }

        // POST: TeaPurchaseNote/Delete
        [HttpPost]
        public async Task<ActionResult> Delete(string tran, string unit, string docno, string docdt)
        {
            tran = TranOf(tran);
            var r = await Services.PostAsync<JObject>(
                $"{Api}Delete?tran={tran}&unit={E(unit)}&docno={E(docno)}&docdt={E(docdt)}", new { });
            TempData[r.IsSuccessStatusCode ? "toastrSuccess" : "toastrError"] =
                r.IsSuccessStatusCode ? $"{docno} deleted." : (r.Message ?? "Delete failed.");
            return RedirectToAction(TranActions[tran]);
        }

        // =====================================================================
        // AJAX passthroughs (lookups search every displayed column)
        // =====================================================================
        private ActionResult JsonExact(object data) =>
            Content(JsonConvert.SerializeObject(data), "application/json");

        // 440 = session expired (prompts re-login), 500 = any other API failure.
        private ActionResult JsonExactOrSessionExpired<T>(ResponseApiModel<T> r)
        {
            if (!r.IsSuccessStatusCode)
            {
                bool isAuthFailure = string.Equals(r.StatusCode, HttpStatusCode.Unauthorized.ToString(), StringComparison.OrdinalIgnoreCase);
                Response.StatusCode = isAuthFailure ? 440 : 500;
                Response.TrySkipIisCustomErrors = true;
                return JsonExact(new
                {
                    sessionExpired = isAuthFailure,
                    message = isAuthFailure ? "Your session has expired. Please log in again."
                            : !string.IsNullOrEmpty(r.Message) ? r.Message : $"Unable to load data (API returned {r.StatusCode})."
                });
            }
            return JsonExact(r.Data);
        }

        private async Task<ActionResult> Lookup(string path) =>
            JsonExactOrSessionExpired(await Services.GetAsync<JToken>(Api + path));

        [HttpGet] public Task<ActionResult> GetRowDetail(string tran = "36", string unit = "", string docno = "", string docdt = "") =>
            Lookup($"GetRowDetail?tran={TranOf(tran)}&unit={E(unit)}&docno={E(docno)}&docdt={E(docdt)}");
        [HttpGet] public Task<ActionResult> GetParty(string search = "", string code = "") =>
            Lookup($"GetParty?search={E(search)}&code={E(code)}");
        [HttpGet] public Task<ActionResult> GetBills(string pcd = "", string unit = "", string type = "", string search = "") =>
            Lookup($"GetBills?pcd={E(pcd)}&unit={E(unit)}&type={E(type)}&search={E(search)}");
        [HttpGet] public Task<ActionResult> GetBillLines(string blno = "", string bldt = "", string pcd = "", string unit = "", string type = "") =>
            Lookup($"GetBillLines?blno={E(blno)}&bldt={E(bldt)}&pcd={E(pcd)}&unit={E(unit)}&type={E(type)}");
        [HttpGet] public Task<ActionResult> GetGrade(string search = "", string code = "") =>
            Lookup($"GetGrade?search={E(search)}&code={E(code)}");
        [HttpGet] public Task<ActionResult> GetMark(string search = "", string code = "") =>
            Lookup($"GetMark?search={E(search)}&code={E(code)}");
        [HttpGet] public Task<ActionResult> GetSalesCentre(string search = "", string code = "") =>
            Lookup($"GetSalesCentre?search={E(search)}&code={E(code)}");
        [HttpGet] public Task<ActionResult> GetRates(string pcd = "", string saleCentre = "", string type = "", string unit = "", string grades = "") =>
            Lookup($"GetRates?pcd={E(pcd)}&saleCentre={E(saleCentre)}&type={E(type)}&unit={E(unit)}&grades={E(grades)}");
    }

    public class PurNoteRow
    {
        public string DOCNO { get; set; }
        public DateTime? DOCDT { get; set; }
        public string UNIT { get; set; }
        public string SALE_TYPE { get; set; }
        public string TYPE_NAME { get; set; }
        public string PCD { get; set; }
        public string PARTY_NAME { get; set; }
        public string BLNO { get; set; }
        public DateTime? BLDT { get; set; }
        public string SALE_CENTRE_NAME { get; set; }
        public string QTY_RETURN { get; set; }
        public string JV_DOCNO { get; set; }
        public string IRN { get; set; }
        public decimal? TOTAL_AMT { get; set; }
    }
}
