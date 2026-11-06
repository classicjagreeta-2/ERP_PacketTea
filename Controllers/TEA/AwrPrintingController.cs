using ClosedXML.Excel;
using PacketTea.Models;
using PacketTea.Models.PT;
using PacketTea.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Finance.Controllers.TEA
{
    // "AWR Printing" (Arrival & Weightment Report) -- ported from Classic ERP's Con_HO_03030100T
    // (menu 2.1 AWR Printing). Parameters: From / To date, Unit, AWR From / To. The old screen's
    // View Report / Excel are the Graphics (Crystal PDF from ~/CrystalReport/ArrivalNWeightment.rpt,
    // the old layout) and Excel buttons; HTML is a printable page. "Send In Mail" is not ported.
    // The API (AwrPrinting/GetReport, company schema: T_AWR) builds the rows.
    public class AwrPrintingController : Controller
    {
        private const string ReportTitle = "Arrival & Weightment Report";
        private const string Abbreviations = "ABBR.:B.R. BROKEN & REPAIRED CONDITION.   L.C. LEAKY CONDITION   O.S OUTWARDLY SOUND CONDITION.   W.C. WEAK CONDITION.   W.S. WATER STAINED.";
        private const string Department = "Tea Warehouse & Export Department";

        public ActionResult Index()
        {
            return View();
        }

        private static List<string> UnitList(string units) =>
            (units ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                         .Select(u => u.Trim())
                         .Where(u => u.Length > 0 && !u.Equals("All", StringComparison.OrdinalIgnoreCase))
                         .ToList();

        // The Unit list: the units the user is linked to for this company (same as the AWR Entry screen).
        public async Task<ActionResult> Units()
        {
            try
            {
                var response = await Services.GetAsync<List<AwrUnit>>("/api/AwrPrinting/GetUnits");
                if (!response.IsSuccessStatusCode || response.Data == null)
                    return Json(new { error = !string.IsNullOrEmpty(response.Message) ? response.Message : "Unable to load the unit list." }, JsonRequestBehavior.AllowGet);
                return Json(response.Data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = FullsStockActualController.ApiCallError(ex) }, JsonRequestBehavior.AllowGet);
            }
        }

        // The AWR From / To lists: the AWR numbers of the period and units picked.
        public async Task<ActionResult> AwrNos(string fromDate, string toDate, string units)
        {
            if (string.IsNullOrWhiteSpace(fromDate) || string.IsNullOrWhiteSpace(toDate))
                return Json(new object[0], JsonRequestBehavior.AllowGet);
            try
            {
                var u = UnitList(units);
                var response = await Services.PostAsync<List<AwrNoItem>>("/api/AwrPrinting/GetAwrNos",
                    new { FromDate = fromDate, ToDate = toDate, Units = u.Count == 0 ? null : u });
                if (!response.IsSuccessStatusCode || response.Data == null)
                    return Json(new { error = !string.IsNullOrEmpty(response.Message) ? response.Message : "Unable to load the AWR numbers." }, JsonRequestBehavior.AllowGet);
                return Json(response.Data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = FullsStockActualController.ApiCallError(ex) }, JsonRequestBehavior.AllowGet);
            }
        }

        private async Task<(AwrPrintingResult Report, string Error)> LoadAsync(string fromDate, string toDate, string units, string awrFrom, string awrTo)
        {
            var unitList = UnitList(units);
            ResponseApiModel<AwrPrintingResult> response;
            try
            {
                response = await Services.PostAsync<AwrPrintingResult>("/api/AwrPrinting/GetReport",
                    new { FromDate = fromDate, ToDate = toDate, Units = unitList.Count == 0 ? null : unitList, AwrFrom = awrFrom, AwrTo = awrTo });
            }
            catch (Exception ex)
            {
                return (null, FullsStockActualController.ApiCallError(ex));
            }
            if (!response.IsSuccessStatusCode || response.Data == null)
                return (null, !string.IsNullOrEmpty(response.Message) ? response.Message : "Unable to build the report.");
            if (response.Data.Rows == null || response.Data.Rows.Count == 0)
                return (null, "No record to print");
            return (response.Data, null);
        }

        // First heading line: the company's short print name (VNAME) when it holds a real name -- the
        // trial company has "." there -- otherwise the company name.
        private static string CompanyLine(AwrPrintingResult r) =>
            !string.IsNullOrWhiteSpace(r.CompanyV) && r.CompanyV.Trim().Length > 1 ? r.CompanyV.Trim() : (r.Company ?? "");

        private static string PeriodText(AwrPrintingResult r)
        {
            string voucher = "";
            if (!string.IsNullOrEmpty(r.AwrFrom))
                voucher = string.IsNullOrEmpty(r.AwrTo) ? " Voucher No " + r.AwrFrom : " of Voucher No From " + r.AwrFrom + " To " + r.AwrTo;
            else if (!string.IsNullOrEmpty(r.AwrTo))
                voucher = " Voucher No " + r.AwrTo;
            if (!string.IsNullOrEmpty(r.FromDate) && !string.IsNullOrEmpty(r.ToDate))
                return ReportTitle + voucher + " For the Period " + r.FromDate + " To " + r.ToDate;
            return ReportTitle + voucher;
        }

        // HTML: printable page (Views/AwrPrinting/Html.cshtml) in a new tab.
        public async Task<ActionResult> Html(string fromDate, string toDate, string units, string awrFrom, string awrTo)
        {
            var (report, error) = await LoadAsync(fromDate, toDate, units, awrFrom, awrTo);
            if (error != null)
            {
                ViewBag.Error = error;
                return View("Html", new AwrPrintingResult());
            }
            ViewBag.Title = PeriodText(report);
            ViewBag.RunBy = SessionHelper.GetUser()?.getUserName ?? "";
            ViewBag.RunAt = DateTime.Now.ToString("dd-MMM-yyyy HH:mm");
            return View("Html", report);
        }

        // Graphics: Crystal PDF from ~/CrystalReport/ArrivalNWeightment.rpt (the old layout; data table "Command").
        public async Task<ActionResult> Graphics(string fromDate, string toDate, string units, string awrFrom, string awrTo)
        {
            var rpt = Server.MapPath("~/CrystalReport/ArrivalNWeightment.rpt");
            if (!System.IO.File.Exists(rpt))
                return FullsStockActualController.CrystalMissing("ArrivalNWeightment.rpt", Url.Action("Html", new { fromDate, toDate, units, awrFrom, awrTo }));

            var (report, error) = await LoadAsync(fromDate, toDate, units, awrFrom, awrTo);
            if (error != null)
                return Content("<div style='font-family:Calibri,Arial;margin:40px'><b>" + ReportTitle + "</b><br/><br/>" +
                               HttpUtility.HtmlEncode(error) + "</div>", "text/html");
            try
            {
                var first = report.Rows[0];
                var pdf = CrystalPdf.Render(rpt, AwrPrintingCrystal.Build(report), new Dictionary<string, string>
                {
                    ["Header1"] = CompanyLine(report),
                    ["Header2"] = CompanyLine(report) == report.Company ? "" : report.Company,
                    ["Header3"] = ReportTitle.ToUpperInvariant(),
                    ["Header4"] = Abbreviations,
                    ["Header5"] = Department,
                    ["Header6"] = first.REG_ADDR ?? "",
                    ["Header7"] = first.PH_NO ?? "",
                    ["ReportFooter"] = "Report: AWR Printing   user: " + (SessionHelper.GetUser()?.getUserName ?? "") +
                                       "   Time: " + DateTime.Now.ToString("HH:mm") + "   Date: " + DateTime.Now.ToString("dd-MMM-yyyy"),
                });
                Response.AppendHeader("Content-Disposition", "inline; filename=AWRPrinting.pdf");
                return File(pdf, "application/pdf");
            }
            catch (Exception ex)
            {
                return Content("Crystal report error:\n\n" + ex, "text/plain");
            }
        }

        public async Task<ActionResult> Excel(string fromDate, string toDate, string units, string awrFrom, string awrTo)
        {
            var (report, error) = await LoadAsync(fromDate, toDate, units, awrFrom, awrTo);
            if (error != null)
                return Json(new { success = false, message = error }, JsonRequestBehavior.AllowGet);
            try
            {
                using (var wb = BuildWorkbook(report))
                {
                    var name = "AWRPrinting-" + (report.FromDate ?? "").Replace("/", "") + "-" + (report.ToDate ?? "").Replace("/", "") + ".xlsx";
                    var r = ClassicExcel.SaveAndOpen(wb, name, Request);
                    return Json(new { success = true, opened = r.Opened, fileName = r.FileName, token = r.Token }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DownloadExcel(string token)
        {
            var bytes = ClassicExcel.Take(token, out var name);
            if (bytes == null) return HttpNotFound("The report has expired - please generate it again.");
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", name);
        }

        private static XLWorkbook BuildWorkbook(AwrPrintingResult r)
        {
            var wb = new XLWorkbook();
            wb.Style.Font.FontName = "Calibri";
            var ws = wb.Worksheets.Add("AWR Printing");
            ws.Cell(1, 1).Value = CompanyLine(r);
            ws.Cell(1, 1).Style.Font.FontSize = 15; ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(2, 1).Value = PeriodText(r);

            string[] head = { "AWR No", "AWR Date", "Invoice No", "Mark", "Grade", "Received From", "Garden Serial", "Bags / Chests",
                              "Gross Wt (per bag)", "Net Wt", "Total Net Wt", "Tare", "Short / Excess", "Transporter", "Vehicle No",
                              "Way Bill", "Despatch No", "Consignment No", "Consignment Date", "Broker", "Remarks", "Damage" };
            for (int c = 0; c < head.Length; c++) ws.Cell(4, c + 1).Value = head[c];
            ws.Range(4, 1, 4, head.Length).Style.Font.Bold = true;
            ws.SheetView.FreezeRows(4);

            int row = 5;
            foreach (var x in r.Rows)
            {
                var v = new object[] { x.AWR_NO, x.AWR_DATE, x.INVNO, x.MARK, x.GRADE, x.REC_NAME, x.GARDEN_SERIAL, x.BAG_CHEST,
                                       x.TGROSS_WT, x.NET_WT, x.TNET_WT, x.TARE, x.SHORT_EXCESS, x.TransName, x.TNO,
                                       x.WAY_BILL, x.DESP_NO, x.CONS_NO1, x.CONS_DT1, x.BROK_NAME, x.AWR_REMARK, (x.SINGLELINE ?? "").Trim() };
                for (int c = 0; c < v.Length; c++)
                {
                    if (v[c] is decimal d) ws.Cell(row, c + 1).Value = d;
                    else ws.Cell(row, c + 1).SetValue(Convert.ToString(v[c]) ?? "");
                }
                row++;
            }
            ws.Range(5, 9, Math.Max(5, row), 13).Style.NumberFormat.Format = "0.000";
            ws.Columns(1, head.Length).AdjustToContents(4, row);
            return wb;
        }
    }
}
