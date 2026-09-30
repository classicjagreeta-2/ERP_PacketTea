using PacketTea.Utility;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using static PacketTea.Helpers;

namespace Finance.Controllers.TEA
{
    // Browser side of the entry screens' edit lock (see Utility/RecordLock.cs): the
    // heartbeat in Views/Shared/_RecordLock.cshtml posts Renew every 60 s while an Edit
    // screen is open, and Release (via navigator.sendBeacon) on Cancel / Back / tab close.
    public class RecordLockController : Controller
    {
        private static readonly HashSet<string> NoAedvScreens =
            new HashSet<string>(System.StringComparer.OrdinalIgnoreCase) { "PacketTeaPurchaseEntry", "PacketTeaInvoice" };

        // Renew = take the lock again (it also re-takes a lock that lapsed while nobody else
        // grabbed it). { ok:false, message } when another user now holds it.
        [HttpPost]
        public async Task<JsonResult> Renew(string screen, string unit, string docno, string docdt = "",
                                            string type = "", string docYear = "")
        {
            var err = CheckCaller(screen);
            if (err != null) return Json(new { ok = false, message = err });
            // Only someone who may edit the screen may hold its locks. Packet Tea Purchase
            // Entry and Sales Invoice have no AEDV enforcement yet (see CLAUDE.md), so they
            // aren't gated here either.
            if (!NoAedvScreens.Contains(screen ?? "")
                && !(AEDV.ForScreen(Session["User_AEDV"] as List<AEDV>, screen)?.Edit ?? false))
                return Json(new { ok = false, message = "You do not have permission to edit this entry." });

            var msg = await RecordLock.AcquireAsync(screen, unit, docno, docdt, type, docYear);
            return Json(new { ok = msg == null, message = msg });
        }

        // Clears only the caller's own lock (the API checks), so no right is needed.
        [HttpPost]
        public async Task<ActionResult> Release(string screen, string unit, string docno, string docdt = "",
                                                string type = "", string docYear = "")
        {
            if (CheckCaller(screen) == null)
                await RecordLock.ReleaseAsync(screen, unit, docno, docdt, type, docYear);
            return new HttpStatusCodeResult(204);
        }

        private static string CheckCaller(string screen)
        {
            if (string.IsNullOrEmpty(SessionHelper.GetUser()?.getToken)) return "Your session has expired. Please log in again.";
            if (!RecordLock.Screens.Contains(screen ?? "")) return "Unknown screen.";
            return null;
        }
    }
}
