using Newtonsoft.Json;
using PacketTea.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace PacketTea.Utility
{
    // Edit lock on the entry screens (LOCKEDBYUSERID / LOCKEDUNTIL) -- the ERP_Finance
    // Voucher Entry lock, done once. The API's RecordLockController does the work; see its
    // header comment. Here:
    //   * InsertOrUpdate (Edit, not View): AcquireAsync after the record loads -- refused
    //     (toast + back to the list) while another user holds it; then ClientConfig() feeds
    //     Views/Shared/_RecordLock.cshtml, whose script renews the lock every 60 s and
    //     releases it on Cancel / Back / closing the tab.
    //   * Save (edit) and Delete: AcquireAsync again first (the server-side check), and
    //     ReleaseAsync after a successful Save.
    //   * List pages: LocksAsync -> ViewBag.Locks; _ListRows greys out the rows another user
    //     holds (LockedBy), and _RecordLock.cshtml stops Edit / Delete on them.
    // A screen is its MVC controller name, the same name AEDV.ForScreen uses.
    public static class RecordLock
    {
        // Screens whose tables are in the Sales schema (called through SalesGetAsync).
        private static readonly HashSet<string> SalesScreens = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ProductionEntry", "MarketReturn", "OtherInvoice", "PacketTeaNote",
        };

        // Screens whose tables are in the Finance schema (called through FinanceGetAsync).
        private static readonly HashSet<string> FinanceScreens = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "PacketTeaInvoice",
        };

        public static readonly HashSet<string> Screens = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "MasterBlendEntry", "FinalBlendEntry", "Packing", "AWREntry", "TeaSampleDrawEntry",
            "TeaPurchaseNote", "PacketTeaPurchaseEntry",
            "ProductionEntry", "MarketReturn", "OtherInvoice", "PacketTeaNote",
            "PacketTeaInvoice",
        };

        private class LockReply
        {
            public bool ok { get; set; }
            public string lockedBy { get; set; }
            public string message { get; set; }
        }

        private class LockRow
        {
            public string KEY { get; set; }
            public string LOCKEDBY { get; set; }
        }

        private static string Date(object d)
        {
            if (d == null) return "";
            if (d is DateTime dt) return dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var parsed = DateText.Parse(d.ToString());
            return parsed.HasValue ? parsed.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : d.ToString().Trim();
        }

        private static string Query(string screen, string unit, string docno, object docdt, string type, string docYear) =>
            $"screen={Uri.EscapeDataString(screen ?? "")}" +
            $"&unit={Uri.EscapeDataString(unit?.Trim() ?? "")}" +
            $"&docno={Uri.EscapeDataString(docno?.Trim() ?? "")}" +
            $"&docdt={Uri.EscapeDataString(Date(docdt))}" +
            $"&type={Uri.EscapeDataString(type?.Trim() ?? "")}" +
            $"&docYear={Uri.EscapeDataString(docYear?.Trim() ?? "")}";

        private static Task<ResponseApiModel<T>> Get<T>(string screen, string url) =>
            SalesScreens.Contains(screen ?? "") ? Services.SalesGetAsync<T>(url)
            : FinanceScreens.Contains(screen ?? "") ? Services.FinanceGetAsync<T>(url)
            : Services.GetAsync<T>(url);

        // Take (or renew) the lock. Null when the caller now holds it, otherwise the message
        // to show ("...being edited by X"). A failed lock call (API down / not updated) does
        // not block editing -- it is logged and treated as free; the lock only coordinates
        // users, it is not a permission.
        public static async Task<string> AcquireAsync(string screen, string unit, string docno, object docdt = null,
                                                      string type = null, string docYear = null)
        {
            try
            {
                var r = await Get<LockReply>(screen, "/api/RecordLock/Acquire?" + Query(screen, unit, docno, docdt, type, docYear));
                if (r.IsSuccessStatusCode && r.Data != null && !r.Data.ok)
                    return r.Data.message ?? $"This record is currently being edited by {r.Data.lockedBy}.";
                if (!r.IsSuccessStatusCode)
                    NLog.LogManager.GetCurrentClassLogger().Warn($"RecordLock.Acquire({screen}, {unit}, {docno}) failed: {r.StatusCode} {r.Message}");
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Warn(ex, $"RecordLock.Acquire({screen}, {unit}, {docno})");
            }
            return null;
        }

        // Clear the caller's own lock (the API never clears another user's).
        public static async Task ReleaseAsync(string screen, string unit, string docno, object docdt = null,
                                              string type = null, string docYear = null)
        {
            try
            {
                await Get<object>(screen, "/api/RecordLock/Release?" + Query(screen, unit, docno, docdt, type, docYear));
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Warn(ex, $"RecordLock.Release({screen}, {unit}, {docno})");
            }
        }

        // Documents of the screen locked by other users: Key(...) -> user. Empty on failure.
        public static async Task<Dictionary<string, string>> LocksAsync(string screen)
        {
            try
            {
                var r = await Get<List<LockRow>>(screen, "/api/RecordLock/GetLocks?screen=" + Uri.EscapeDataString(screen));
                if (r.IsSuccessStatusCode && r.Data != null)
                    return r.Data.Where(x => !string.IsNullOrEmpty(x.KEY))
                                 .GroupBy(x => x.KEY, StringComparer.OrdinalIgnoreCase)
                                 .ToDictionary(g => g.Key, g => g.First().LOCKEDBY, StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Warn(ex, $"RecordLock.Locks({screen})");
            }
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        // A list row's key, in the order of the API's RecordLockController.Screens Keys for
        // that screen (Unit, Doc No, then Doc Date / Type / Doc Year as the screen has them).
        public static string Key(params object[] parts) =>
            string.Join("|", parts.Select(p => p is DateTime ? Date(p) : (p?.ToString() ?? "").Trim()));

        // Who (other than the current user) holds the row's lock, or null. `locks` is ViewBag.Locks.
        public static string LockedBy(object locks, params object[] parts) =>
            locks is Dictionary<string, string> d && d.TryGetValue(Key(parts), out var who) ? who : null;

        // Attributes for a list row's <tr> held by another user (blank when it isn't):
        // greyed out, tooltip, and data-lockedby for _RecordLock.cshtml's click guard.
        public static IHtmlString RowAttrs(object locks, params object[] parts)
        {
            var who = LockedBy(locks, parts);
            if (who == null) return new HtmlString("");
            var enc = HttpUtility.HtmlAttributeEncode(who);
            return new HtmlString($" data-lockedby=\"{enc}\" title=\"Being edited by {enc}\"");
        }

        // What _RecordLock.cshtml needs to keep the lock alive on an entry screen.
        public static string ClientConfig(string screen, string unit, string docno, object docdt = null,
                                          string type = null, string docYear = null) =>
            JsonConvert.SerializeObject(new
            {
                screen,
                unit = unit?.Trim() ?? "",
                docno = docno?.Trim() ?? "",
                docdt = Date(docdt),
                type = type?.Trim() ?? "",
                docYear = docYear?.Trim() ?? "",
            });
    }
}
