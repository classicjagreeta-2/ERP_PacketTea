using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PacketTea.Models;
using PacketTea.Models.PT;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Finance.Controllers.TEA
{
    // "Tea Manufacturing / Sale - Packet Tea Debit Note / Credit Note" -- ported from
    // VB6 trn_credit_note.frm (INVTYPE 0/1, TRN_DNCN_HED.TRAN_CODE 34/35). Two menus,
    // one screen: DebitNote (34) and CreditNote (35). The list's "New" row picks the
    // Unit, which is then locked on the entry screen. Data lives in the Sales schema
    // (CLASSIC_CONTROL.SCHEMA_SALES, e.g. FIN_JSTIL2027), so every call goes through
    // Services.SalesGetAsync/SalesPostAsync like Other Invoice.
    public class PacketTeaNoteController : Controller
    {
        private static readonly Dictionary<string, string> TranNames = new Dictionary<string, string>
        {
            { "34", "Packet Tea Debit Note" },
            { "35", "Packet Tea Credit Note" },
        };
        private static readonly Dictionary<string, string> TranActions = new Dictionary<string, string>
        {
            { "34", "DebitNote" },
            { "35", "CreditNote" },
        };

        private static string TranOf(string tran) => tran == "35" ? "35" : "34";
        private static string E(string s) => Uri.EscapeDataString(s ?? "");
        private const string Api = "/api/PacketTeaNote/";

        private async Task<List<UnitOption>> GetUnitsForUserAsync()
        {
            var r = await Services.SalesGetAsync<List<UnitOption>>(Api + "GetUnitsForUser");
            return (r.IsSuccessStatusCode ? r.Data : null) ?? new List<UnitOption>();
        }

        public ActionResult Index() => RedirectToAction("DebitNote");

        // GET: PacketTeaNote/DebitNote (menu "Packet Tea Debit Note")
        public Task<ActionResult> DebitNote(string unit, string searchString, int? page = 1, int pageSize = 15, string sortBy = "", string sortDir = "")
            => List("34", unit, searchString, page, pageSize, sortBy, sortDir);

        // GET: PacketTeaNote/CreditNote (menu "Packet Tea Credit Note")
        public Task<ActionResult> CreditNote(string unit, string searchString, int? page = 1, int pageSize = 15, string sortBy = "", string sortDir = "")
            => List("35", unit, searchString, page, pageSize, sortBy, sortDir);

        private async Task<ActionResult> List(string tran, string unit, string searchString, int? page, int pageSize, string sortBy, string sortDir)
        {
            ViewBag.Tran = tran;
            ViewBag.TranName = TranNames[tran];
            ViewBag.ListAction = TranActions[tran];
            ViewBag.CurrentFilter = searchString;
            ViewBag.SortBy = sortBy;
            ViewBag.SortDir = sortDir;
            ViewBag.PageSize = pageSize;
            ViewBag.Page = page ?? 1;
            ViewBag.Unit = unit;
            ViewBag.UnitList = await GetUnitsForUserAsync();

            var response = await Services.SalesGetAsync<JObject>(
                $"{Api}GetByPage?tran={tran}&unit={E(unit)}&search={E(searchString)}&page={page ?? 1}&pageSize={pageSize}&sortBy={E(sortBy)}&sortDir={E(sortDir)}");

            var value = response?.Data?["value"];
            var list = value?["results"]?.ToObject<List<PtNoteRow>>() ?? new List<PtNoteRow>();
            ViewBag.RowCount = value?["rowCount"]?.Value<int>() ?? 0;

            if (!(response?.IsSuccessStatusCode ?? false))
            {
                TempData["toastrError"] = !string.IsNullOrEmpty(response?.Message)
                    ? response.Message
                    : $"Unable to load the {TranNames[tran]} list (API returned {response?.StatusCode}).";
            }
            return View("Index", list);
        }

        // GET: PacketTeaNote/InsertOrUpdate
        // New: `unit` comes from the list's "New" row and is locked here.
        // Edit/View: the note's key (unit, docno, docdt).
        public async Task<ActionResult> InsertOrUpdate(string tran = "34", string unit = "", string docno = "", string docdt = "", bool view = false)
        {
            tran = TranOf(tran);
            ViewBag.Tran = tran;
            ViewBag.TranName = TranNames[tran];
            ViewBag.ListAction = TranActions[tran];
            ViewBag.IsView = view;

            if (string.IsNullOrEmpty(docno))
            {
                if (string.IsNullOrEmpty(unit))
                {
                    TempData["toastrError"] = "Select Unit from the New row first.";
                    return RedirectToAction(TranActions[tran]);
                }
                var ctx = await Services.SalesGetAsync<JObject>($"{Api}GetNewContext?tran={tran}&unit={E(unit)}");
                if (!ctx.IsSuccessStatusCode || ctx.Data == null)
                {
                    TempData["toastrError"] = ctx.Message ?? "Unable to open a new note.";
                    return RedirectToAction(TranActions[tran]);
                }
                ViewBag.IsEdit = false;
                ViewBag.DocJson = JsonConvert.SerializeObject(new { head = ctx.Data, lines = new object[0] });
                return View();
            }

            var r = await Services.SalesGetAsync<JObject>($"{Api}GetByDocNo?tran={tran}&unit={E(unit)}&docno={E(docno)}&docdt={E(docdt)}");
            if (!r.IsSuccessStatusCode || r.Data == null)
            {
                TempData["toastrError"] = r.Message ?? "Record not found.";
                return RedirectToAction(TranActions[tran]);
            }
            ViewBag.IsEdit = true;
            ViewBag.DocJson = JsonConvert.SerializeObject(r.Data);
            return View();
        }

        // POST: PacketTeaNote/Save (JSON body). The API recomputes every figure and
        // posts the accounts voucher. dryRun=true runs it all and rolls back.
        [HttpPost]
        public async Task<ActionResult> Save(bool dryRun = false)
        {
            PT_NOTE_DATA model;
            try
            {
                Request.InputStream.Position = 0;
                using (var reader = new StreamReader(Request.InputStream))
                    model = JsonConvert.DeserializeObject<PT_NOTE_DATA>(reader.ReadToEnd());
            }
            catch (Exception ex)
            {
                return JsonExact(new { success = false, message = "Invalid data: " + ex.Message });
            }
            if (model == null) return JsonExact(new { success = false, message = "Nothing to save." });

            var r = await Services.SalesPostAsync<JObject>(Api + "SaveOrUpdate" + (dryRun ? "?dryRun=true" : ""), model);
            return JsonExact(new
            {
                success = r.IsSuccessStatusCode,
                message = r.IsSuccessStatusCode ? "Saved successfully." : (r.Message ?? "Save failed."),
                data = r.Data
            });
        }

        // POST: PacketTeaNote/Delete
        [HttpPost]
        public async Task<ActionResult> Delete(string tran, string unit, string docno, string docdt)
        {
            tran = TranOf(tran);
            var r = await Services.SalesPostAsync<JObject>(
                $"{Api}Delete?tran={tran}&unit={E(unit)}&docno={E(docno)}&docdt={E(docdt)}", new { });
            TempData[r.IsSuccessStatusCode ? "toastrSuccess" : "toastrError"] =
                r.IsSuccessStatusCode ? $"{docno} deleted." : (r.Message ?? "Delete failed.");
            return RedirectToAction(TranActions[tran]);
        }

        // =====================================================================
        // AJAX passthroughs (lookups search every displayed column, paged server-side)
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
            JsonExactOrSessionExpired(await Services.SalesGetAsync<JToken>(Api + path));

        [HttpGet] public Task<ActionResult> GetRowDetail(string tran = "34", string unit = "", string docno = "", string docdt = "") =>
            Lookup($"GetRowDetail?tran={TranOf(tran)}&unit={E(unit)}&docno={E(docno)}&docdt={E(docdt)}");
        [HttpGet] public Task<ActionResult> GetParty(string search = "", string unit = "", string code = "", int page = 1) =>
            Lookup($"GetParty?search={E(search)}&unit={E(unit)}&code={E(code)}&page={page}");
        [HttpGet] public Task<ActionResult> GetBills(string pcd = "", string unit = "", bool lastYear = false, string search = "", int page = 1) =>
            Lookup($"GetBills?pcd={E(pcd)}&unit={E(unit)}&lastYear={lastYear}&search={E(search)}&page={page}");
        [HttpGet] public Task<ActionResult> GetBillLines(string blno = "", string bldt = "", string unit = "", bool lastYear = false) =>
            Lookup($"GetBillLines?blno={E(blno)}&bldt={E(bldt)}&unit={E(unit)}&lastYear={lastYear}");
        [HttpGet] public Task<ActionResult> GetItem(string search = "", string unit = "", string code = "", int page = 1) =>
            Lookup($"GetItem?search={E(search)}&unit={E(unit)}&code={E(code)}&page={page}");
        [HttpGet] public Task<ActionResult> GetGstCodes(string unit = "") =>
            Lookup($"GetGstCodes?unit={E(unit)}");
    }

    public class PtNoteRow
    {
        public string UNIT { get; set; }
        public string DOCNO { get; set; }
        public DateTime? DOCDT { get; set; }
        public string PCD { get; set; }
        public string PARTY_NAME { get; set; }
        public string BLNO { get; set; }
        public DateTime? BLDT { get; set; }
        public string IRN { get; set; }
        public decimal? TAXABLE_AMT { get; set; }
        public decimal? TOTAL_AMT { get; set; }
    }
}
