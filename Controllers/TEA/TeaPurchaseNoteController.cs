using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PacketTea;
using PacketTea.Models;
using PacketTea.Models.PT;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using static PacketTea.Helpers;

namespace Finance.Controllers.TEA
{
    // "Tea Procurement - Tea Purchase Debit Note / Credit Note" -- ported from VB6
    // trn_credit_note.frm (INVTYPE 2/3, T_DNCN_HED.TRAN_CODE 36/37). Two menus, one
    // screen: DebitNote (36) and CreditNote (37). List and entry screen follow Master
    // Blend Entry / Packing Entry (see CLAUDE.md): the list's "New" row picks Unit + Type
    // (purchase type), which are then locked on the entry screen; the list is scoped to the
    // Units the user is linked to (USER_SCHEMA_LINK) and the purchase types they have rights
    // to (M_UNIT_USER_RIGHT), pages as a chunked infinite scroll, and Add/Edit/Delete/View +
    // the back-date window come from AEDV (screen key "TeaPurchaseNote", which falls back to
    // the shared "PacketTeaPurchaseEntry" row). Data lives in the factory schema, so every
    // call goes through Services.GetAsync/PostAsync like AWR / Sample Draw.
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

        private const string Screen = "TeaPurchaseNote";   // AEDV.ForScreen key

        private static string TranOf(string tran) => tran == "37" ? "37" : "36";
        private static string E(string s) => Uri.EscapeDataString(s ?? "");
        private const string Api = "/api/TeaPurchaseNote/";

        private AEDV Permission => AEDV.ForScreen((List<AEDV>)Session["User_AEDV"], Screen);

        private async Task<Dictionary<string, string>> GetAllowedTypesAsync()
        {
            var r = await Services.GetAsync<Dictionary<string, string>>(Api + "GetAllowedTypes");
            return (r.IsSuccessStatusCode ? r.Data : null) ?? new Dictionary<string, string>();
        }

        // The Units the user is linked to (USER_SCHEMA_LINK) -- backs the list scope, the
        // "New" row's Unit picker and the Unit checks on open / save / delete.
        private async Task<List<UnitOption>> GetUnitsForUserAsync()
        {
            var r = await Services.GetAsync<List<UnitOption>>(Api + "GetUnitsForUser");
            return (r.IsSuccessStatusCode ? r.Data : null) ?? new List<UnitOption>();
        }

        public ActionResult Index() => RedirectToAction("DebitNote");

        // GET: TeaPurchaseNote/DebitNote (menu "Tea Purchase Debit Note")
        public Task<ActionResult> DebitNote(string searchString, int? page = 1, int pageSize = 15, string sortBy = "", string sortDir = "", string unit = "")
            => List("36", unit, searchString, page, pageSize, sortBy, sortDir);

        // GET: TeaPurchaseNote/CreditNote (menu "Tea Purchase Credit Note")
        public Task<ActionResult> CreditNote(string searchString, int? page = 1, int pageSize = 15, string sortBy = "", string sortDir = "", string unit = "")
            => List("37", unit, searchString, page, pageSize, sortBy, sortDir);

        // Scoped to the Units the user is linked to (or the one `unit` asked for, if permitted)
        // and to every purchase type the user has rights to, both sent as comma-separated lists
        // -- same as PackingController.Index. The list's Unit / Type dropdowns are gone; Unit +
        // Type are picked in the "New" row only.
        private async Task<ActionResult> List(string tran, string unit, string searchString, int? page, int pageSize, string sortBy, string sortDir)
        {
            ViewBag.Permission = Permission;
            ViewBag.Tran = tran;
            ViewBag.TranName = TranNames[tran];
            ViewBag.ListAction = TranActions[tran];
            ViewBag.CurrentFilter = searchString;
            ViewBag.SortBy = sortBy;
            ViewBag.SortDir = sortDir;
            ViewBag.PageSize = pageSize;

            var typesTask = GetAllowedTypesAsync();
            var unitsTask = GetUnitsForUserAsync();
            await Task.WhenAll(typesTask, unitsTask);
            var allowedTypes = typesTask.Result;
            var userUnits = unitsTask.Result;
            ViewBag.Types = allowedTypes;
            ViewBag.UnitList = userUnits;
            var unitFilter = E(UnitScope.ListFilter(unit, userUnits));
            // ONE comma-separated `type`; "~" (matches no type) when the user has rights to none,
            // so the list comes back empty rather than (with a blank `type`) unfiltered.
            var typeFilter = E(allowedTypes.Count > 0 ? string.Join(",", allowedTypes.Keys) : UnitScope.NoUnits);

            // Chunked list: a full page view always starts at the first chunk; further chunks
            // arrive as AJAX calls (Scripts/list-infinite-scroll.js) and return just the rows.
            var isChunkRequest = Request.IsAjaxRequest();
            var pageNo = isChunkRequest ? Math.Max(page ?? 1, 1) : 1;
            ViewBag.Page = pageNo;
            ViewBag.RowOffset = (pageNo - 1) * pageSize;

            var response = await Services.GetAsync<JObject>(
                $"{Api}GetByPage?tran={tran}&type={typeFilter}&unit={unitFilter}&search={E(searchString)}&page={pageNo}&pageSize={pageSize}&sortBy={E(sortBy)}&sortDir={E(sortDir)}");

            var value = response?.Data?["value"];
            var list = value?["results"]?.ToObject<List<PurNoteRow>>() ?? new List<PurNoteRow>();
            ViewBag.RowCount = value?["rowCount"]?.Value<int>() ?? 0;

            if (isChunkRequest)
            {
                // A failed chunk must not leave a toast queued for the next full page load; an
                // empty response tells the list's scroll loader to stop.
                return PartialView("_ListRows", (response?.IsSuccessStatusCode ?? false) ? list : new List<PurNoteRow>());
            }

            if (!(response?.IsSuccessStatusCode ?? false))
            {
                TempData["toastrError"] = !string.IsNullOrEmpty(response?.Message)
                    ? response.Message
                    : $"Unable to load the {TranNames[tran]} list (API returned {response?.StatusCode}).";
            }
            return View("Index", list);
        }

        // Doc Date picker's lower bound -- the later of the financial-year start and the AEDV
        // back-date allowance (Aday for a new note, Eday while editing). Save() is the real
        // gate; this only steers the date picker (same as PackingController.InsertOrUpdate).
        private void SetMinDocDate(AEDV permission, bool isNew)
        {
            DateTime? fyStart = null;
            string fy = Session["SelectedfinancialYear"]?.ToString();
            if (!string.IsNullOrEmpty(fy) && fy.Contains("-"))
            {
                var parts = fy.Split('-');
                string startDigits = new string(parts[0].Where(char.IsDigit).ToArray());
                if (startDigits.Length >= 4)
                    fyStart = new DateTime(int.Parse(startDigits.Substring(startDigits.Length - 4)), 4, 1);
            }
            var backDateFloor = (permission ?? new AEDV()).MinDocDate(isNew);
            var effectiveMin = fyStart.HasValue && fyStart.Value > backDateFloor ? fyStart.Value : backDateFloor;
            ViewBag.MinDocDate = effectiveMin.ToString("yyyy-MM-dd");
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

            var permission = Permission;
            ViewBag.Permission = permission;

            bool isNew = string.IsNullOrEmpty(docno);

            // `view` is set by the list's View button / Doc No link -- same fetch as Edit, but the
            // form renders read-only. It only applies to an existing note and needs the View right.
            view = view && !isNew;
            if (view && !(permission?.View ?? false))
            {
                TempData["toastrError"] = "You do not have permission to view this entry.";
                return RedirectToAction(TranActions[tran]);
            }
            ViewBag.IsView = view;

            // The list hides New without the Add right, but this URL is reachable directly.
            if (isNew && !(permission?.Add ?? false))
            {
                TempData["toastrError"] = "You do not have permission to add a new entry.";
                return RedirectToAction(TranActions[tran]);
            }

            SetMinDocDate(permission, isNew);

            if (isNew)
            {
                if (string.IsNullOrEmpty(unit) || string.IsNullOrEmpty(type))
                {
                    TempData["toastrError"] = "Select Unit and Type from the New row first.";
                    return RedirectToAction(TranActions[tran]);
                }
                // Permission lookup and context call run side by side; the context is simply
                // discarded when the Unit turns out not to be permitted.
                var unitsTask = GetUnitsForUserAsync();
                var ctxTask = Services.GetAsync<JObject>($"{Api}GetNewContext?tran={tran}&unit={E(unit)}&type={E(type)}");
                await Task.WhenAll(unitsTask, ctxTask);
                var userUnits = unitsTask.Result;
                var ctx = ctxTask.Result;

                if (!UnitScope.IsAllowed(unit, userUnits))
                {
                    TempData["toastrError"] = $"You do not have permission for Unit {unit}.";
                    return RedirectToAction(TranActions[tran]);
                }
                if (!ctx.IsSuccessStatusCode || ctx.Data == null)
                {
                    TempData["toastrError"] = ctx.Message ?? "Unable to open a new note.";
                    return RedirectToAction(TranActions[tran]);
                }
                // The API books a type's notes under the Unit its purchases live in, which can
                // differ from the picked one -- that Unit is what gets stored, so it must be
                // one the user is linked to as well.
                var docUnit = (string)ctx.Data["UNIT"];
                if (!UnitScope.IsAllowed(docUnit, userUnits))
                {
                    TempData["toastrError"] = $"Type {type} is booked under Unit {docUnit}, which you are not linked to.";
                    return RedirectToAction(TranActions[tran]);
                }
                ViewBag.IsEdit = false;
                ViewBag.DocJson = JsonConvert.SerializeObject(new { head = ctx.Data, lines = new object[0] });
                return View();
            }

            var unitsTask2 = GetUnitsForUserAsync();
            var noteTask = Services.GetAsync<JObject>($"{Api}GetByDocNo?tran={tran}&unit={E(unit)}&docno={E(docno)}&docdt={E(docdt)}");
            await Task.WhenAll(unitsTask2, noteTask);
            var r = noteTask.Result;
            if (!r.IsSuccessStatusCode || r.Data == null)
            {
                TempData["toastrError"] = r.Message ?? "Record not found.";
                return RedirectToAction(TranActions[tran]);
            }

            // The note's own Unit must be one the user is linked to (a Doc No repeats across units).
            var head = r.Data["head"];
            var noteUnit = (string)head?["UNIT"];
            if (!UnitScope.IsAllowed(noteUnit, unitsTask2.Result))
            {
                TempData["toastrError"] = $"You do not have permission for Unit {noteUnit}.";
                return RedirectToAction(TranActions[tran]);
            }

            // Block opening Edit outright when the user has no Edit right, or the note's own Doc
            // Date has fallen outside the Eday back-date window, or its E-Invoice is generated --
            // View bypasses this (read-only regardless of the Edit / back-date policy).
            if (!view)
            {
                var editErr = AEDV.CheckAddEdit(permission, false, (DateTime?)head?["DOCDT"] ?? DateTime.Today);
                if (editErr == null && !string.IsNullOrWhiteSpace((string)head?["IRN"]))
                    editErr = "E-Invoice has been Generated for this, can't edit.";
                if (editErr != null)
                {
                    TempData["toastrError"] = editErr;
                    return RedirectToAction(TranActions[tran]);
                }
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

            // Server-side AEDV Add/Edit + back-date enforcement -- the Doc Date picker's `min`
            // (see InsertOrUpdate) only steers well-behaved clients; this is the actual gate.
            bool isNew = model.IsNew ?? string.IsNullOrWhiteSpace(model.DOCNO);
            var permErr = AEDV.CheckAddEdit(Permission, isNew, model.DOCDT ?? DateTime.Today);
            if (permErr != null) return JsonExact(new { success = false, message = permErr });

            // The Unit (a NEW note is stamped with the one picked on the list, posted from the
            // entry screen; an edit keeps the note's own) must be one the user is linked to.
            if (!UnitScope.IsAllowed(model.UNIT, await GetUnitsForUserAsync()))
                return JsonExact(new { success = false, message = $"You do not have permission for Unit {model.UNIT}." });

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
            if (!(Permission?.Delete ?? false))
            {
                TempData["toastrError"] = "You do not have permission to delete this entry.";
                return RedirectToAction(TranActions[tran]);
            }

            // The row's Unit (a Doc No repeats across units) must be one the user is linked to.
            if (!UnitScope.IsAllowed(unit, await GetUnitsForUserAsync()))
            {
                TempData["toastrError"] = $"You do not have permission for Unit {unit}.";
                return RedirectToAction(TranActions[tran]);
            }

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

        // Reshapes a plain-array lookup response into { data, count } for jquery.inputpicker
        // (Root UI convention -- see CLAUDE.md's "Lookup pickers" rule). The API returns up to its
        // top-100 matches (no rowCount of its own), so this pages that array in memory: `p`/`limit`
        // pick the slice and `count` is the number of matches, which is what the plugin's footer
        // shows and pages by. Same shape as PackingController.PickerJson.
        private ActionResult PickerJson(ResponseApiModel<JToken> r, int limit, int p)
        {
            var arr = (r?.IsSuccessStatusCode == true ? r.Data as JArray : null) ?? new JArray();
            var take = limit > 0 ? limit : 10;
            var page = arr.Skip((Math.Max(p, 1) - 1) * take).Take(take);
            return JsonExact(new { data = page, count = arr.Count });
        }

        // inputpicker endpoints (Party / Grade / Mark / Sales Centre). The plugin sends the typed
        // text as `q` (the API matches it, case-insensitively, against EVERY column the dropdown
        // shows) and -- when the field already holds a value -- that value as `value`, which is
        // looked up as an exact code so a loaded record's picker shows its own row.
        private async Task<ActionResult> Picker(string path, string q, string value, int limit, int p)
        {
            var arg = string.IsNullOrWhiteSpace(q) && !string.IsNullOrWhiteSpace(value)
                ? "code=" + E(value.Trim())
                : "search=" + E((q ?? "").Trim());
            return PickerJson(await Services.GetAsync<JToken>($"{Api}{path}?{arg}"), limit, p);
        }

        [HttpGet] public Task<ActionResult> GetRowDetail(string tran = "36", string unit = "", string docno = "", string docdt = "") =>
            Lookup($"GetRowDetail?tran={TranOf(tran)}&unit={E(unit)}&docno={E(docno)}&docdt={E(docdt)}");
        [HttpGet] public Task<ActionResult> GetParty(string q = "", int limit = 0, string fieldValue = "", string fieldText = "", string value = "", int p = 1) =>
            Picker("GetParty", q, value, limit, p);
        // Bill No: composite (depends on party / unit / type) -- stays a bespoke picker, not inputpicker.
        [HttpGet] public Task<ActionResult> GetBills(string pcd = "", string unit = "", string type = "", string search = "") =>
            Lookup($"GetBills?pcd={E(pcd)}&unit={E(unit)}&type={E(type)}&search={E(search)}");
        [HttpGet] public Task<ActionResult> GetBillLines(string blno = "", string bldt = "", string pcd = "", string unit = "", string type = "") =>
            Lookup($"GetBillLines?blno={E(blno)}&bldt={E(bldt)}&pcd={E(pcd)}&unit={E(unit)}&type={E(type)}");
        [HttpGet] public Task<ActionResult> GetGrade(string q = "", int limit = 0, string fieldValue = "", string fieldText = "", string value = "", int p = 1) =>
            Picker("GetGrade", q, value, limit, p);
        [HttpGet] public Task<ActionResult> GetMark(string q = "", int limit = 0, string fieldValue = "", string fieldText = "", string value = "", int p = 1) =>
            Picker("GetMark", q, value, limit, p);
        [HttpGet] public Task<ActionResult> GetSalesCentre(string q = "", int limit = 0, string fieldValue = "", string fieldText = "", string value = "", int p = 1) =>
            Picker("GetSalesCentre", q, value, limit, p);
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
