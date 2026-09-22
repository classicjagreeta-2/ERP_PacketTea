using ClosedXML.Excel;
using PacketTea.Models;
using PacketTea.Models.PT;
using PacketTea.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Finance.Controllers.TEA
{
    // "Stock Report" -- ported from VB6 fullmove_depo.frm (Item Wise). Parameters:
    // From / To, Unit (All or a checked list of manufacturing units), Only Packet
    // Quantity / Only Net Weight / Both, Reporting Sequence (Product Code / Product
    // Name / Custom). VB6's "Stock Report" / "Excel Upload" choice is the Graphics /
    // Excel button pair. The API (StockReport/GetReport, Sales schema) builds the rows.
    public class StockReportController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        private async Task<(StockReportResult Report, string Error)> LoadAsync(string fromDate, string toDate, string units, string qtyWt, string order)
        {
            var unitList = (units ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(u => u.Trim())
                                        .Where(u => u.Length > 0 && !u.Equals("All", StringComparison.OrdinalIgnoreCase))
                                        .ToList();
            var response = await Services.SalesPostAsync<StockReportResult>("/api/StockReport/GetReport",
                new { FromDate = fromDate, ToDate = toDate, Units = unitList, QtyWt = qtyWt, Order = order });
            if (!response.IsSuccessStatusCode || response.Data == null)
                return (null, !string.IsNullOrEmpty(response.Message) ? response.Message : "Unable to build the report.");
            if (response.Data.Rows == null || response.Data.Rows.Count == 0)
                return (null, "No record to print");
            return (response.Data, null);
        }

        // HTML: paged landscape page (Views/StockReport/Graphics.cshtml) in a new tab.
        public async Task<ActionResult> Html(string fromDate, string toDate, string units, string qtyWt, string order)
        {
            var (report, error) = await LoadAsync(fromDate, toDate, units, qtyWt, order);
            if (error != null)
            {
                ViewBag.Error = error;
                return View("Graphics", new StockReportResult());
            }
            report.RunBy = SessionHelper.GetUser()?.getUserName ?? "";
            report.RunAt = DateTime.Now.ToString("dd-MMM-yyyy HH:mm");
            return View("Graphics", report);
        }

        // Graphics: Crystal PDF from ~/CrystalReport/StockReport.rpt (data source
        // StockReportCrystal / CrystalReport/StockReport.xsd), shown inline in a new tab.
        public async Task<ActionResult> Graphics(string fromDate, string toDate, string units, string qtyWt, string order)
        {
            var rpt = Server.MapPath("~/CrystalReport/StockReport.rpt");
            if (!System.IO.File.Exists(rpt))
                return FullsStockActualController.CrystalMissing("StockReport.rpt", Url.Action("Html", new { fromDate, toDate, units, qtyWt, order }));

            var (report, error) = await LoadAsync(fromDate, toDate, units, qtyWt, order);
            if (error != null)
                return Content("<div style='font-family:Arial;margin:40px'><b>Stock Report</b><br/><br/>" +
                               HttpUtility.HtmlEncode(error) + "</div>", "text/html");
            try
            {
                var pdf = CrystalPdf.Render(rpt, StockReportCrystal.Build(report), new Dictionary<string, string>
                {
                    ["HEADER1"] = report.Company,
                    ["HEADER2"] = report.Address,
                    ["TITLE"] = "Stock Report for the period " + report.FromDate + " To " + report.ToDate,
                    ["UNIT_TEXT"] = "[Unit: " + report.Units + "]",
                    ["RUN_INFO"] = "Rundate " + DateTime.Now.ToString("dd-MMM-yyyy HH:mm") + "   User " + (SessionHelper.GetUser()?.getUserName ?? ""),
                    ["SKIPPED"] = report.Skipped != null && report.Skipped.Count > 0
                        ? "Not included (table not found in this schema): " + string.Join(", ", report.Skipped) : "",
                });
                Response.AppendHeader("Content-Disposition", "inline; filename=StockReport.pdf");
                return File(pdf, "application/pdf");
            }
            catch (Exception ex)
            {
                return Content("Crystal report error:\n\n" + ex, "text/plain");
            }
        }

        public async Task<ActionResult> Excel(string fromDate, string toDate, string units, string qtyWt, string order)
        {
            var (report, error) = await LoadAsync(fromDate, toDate, units, qtyWt, order);
            if (error != null)
                return Json(new { success = false, message = error }, JsonRequestBehavior.AllowGet);
            try
            {
                using (var wb = BuildWorkbook(report))
                {
                    var name = "StockReport-" + (report.FromDate ?? "").Replace("/", "") + "-" + (report.ToDate ?? "").Replace("/", "") + ".xlsx";
                    var r = ClassicExcel.SaveAndOpen(wb, name, Request);
                    return Json(new { success = true, opened = r.Opened, fileName = r.FileName }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DownloadExcel(string file)
        {
            var bytes = ClassicExcel.Read(file);
            if (bytes == null) return HttpNotFound();
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", System.IO.Path.GetFileName(file));
        }

        // Text: character-format (dot-matrix) listing, as Fulls Stock Actual's Text --
        // saved to C:\classic and opened in the default .txt program; downloaded
        // through DownloadText when it cannot be opened on this machine.
        public async Task<ActionResult> Text(string fromDate, string toDate, string units, string qtyWt, string order)
        {
            var (report, error) = await LoadAsync(fromDate, toDate, units, qtyWt, order);
            if (error != null)
                return Json(new { success = false, message = error }, JsonRequestBehavior.AllowGet);
            try
            {
                var text = BuildText(report, SessionHelper.GetUser()?.getUserName ?? "");
                var name = "StockReport-" + (report.FromDate ?? "").Replace("/", "") + "-" + (report.ToDate ?? "").Replace("/", "") + ".txt";
                var r = ClassicExcel.SaveTextAndOpen(text, name, Request);
                return Json(new { success = true, opened = r.Opened, fileName = r.FileName }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DownloadText(string file)
        {
            var bytes = ClassicExcel.Read(file);
            if (bytes == null) return HttpNotFound();
            return File(bytes, "text/plain", System.IO.Path.GetFileName(file));
        }

        // Same 19 movement columns as the HTML / Crystal pages. Code 6, description 20
        // and type 10, one space apart, then right-aligned 10-wide numbers (zeros blank)
        // with no separator, so a line is 228 characters -- a 15" condensed page.
        // Header on every 66-line page, pages separated by a form feed.
        private const int TxtNumW = 10, TxtPageLines = 66;

        private static string TxtRow(string code, string desc, string type, IEnumerable<string> nums)
        {
            Func<string, int, string> fit = (s, w) => { s = s ?? ""; return (s.Length > w ? s.Substring(0, w) : s).PadRight(w); };
            var sb = new System.Text.StringBuilder();
            sb.Append(fit(code, 6)).Append(' ').Append(fit(desc, 20)).Append(' ').Append(fit(type, 10));
            foreach (var n in nums) sb.Append((n ?? "").PadLeft(TxtNumW));
            return sb.ToString().TrimEnd();
        }

        private static string BuildText(StockReportResult r, string user)
        {
            Func<decimal, string> n = v => v == 0 ? "" : v.ToString("0.00");
            Func<StockReportRow, IEnumerable<string>> vals = x => new[] {
                x.OP, x.PROD, x.PURCH, x.MRETU, x.TOTAL_RECT, x.LOADOUT, x.CHECKIN, x.DIRECT_SALE, x.INDIRECT_SALE,
                x.TRANS, x.PURCH_RETU, x.TOTAL_DESP, x.ADJ, x.SAMPLE, x.BKG_LKG, x.SHT, x.TOTAL_OUT, x.CL_STOCK, x.GAIN_LOSS }.Select(n);
            string[] h1 = { "Opening", "", "", "Market", "Total", "Load", "Check", "Direct", "", "", "Purch.", "Total", "", "", "Bkg./", "", "Total", "Closing", "Gain/" };
            string[] h2 = { "Balance", "Production", "Purchase", "Return", "Receipt", "Out", "In", "Sale", "Sale", "Transfers", "Return", "Despatch", "Adjust.", "Sample", "Lkg.", "Shortage", "Out", "Stock", "Loss" };
            int width = 6 + 1 + 20 + 1 + 10 + 19 * TxtNumW;
            var rule = new string('-', width);

            var body = new List<string>();
            foreach (var x in r.Rows) body.Add(TxtRow(x.ITCD, x.DESCN, x.TypeText, vals(x)));
            body.Add(rule);
            foreach (var x in r.Totals) body.Add(TxtRow("", x.DESCN, x.TypeText, vals(x)));
            body.Add(rule);
            if (r.Skipped != null && r.Skipped.Count > 0)
                body.Add("Not included (table not found in this schema): " + string.Join(", ", r.Skipped));
            body.Add("");
            body.Add("Report: Stock Report   user: " + user + "   Time: " + DateTime.Now.ToString("HH:mm") + "   Date: " + DateTime.Now.ToString("dd-MMM-yyyy"));

            const int headLines = 7;
            var perPage = TxtPageLines - headLines;
            var pages = Math.Max(1, (body.Count + perPage - 1) / perPage);
            var sb = new System.Text.StringBuilder();
            for (int p = 0; p < pages; p++)
            {
                if (p > 0) sb.Append('\f');
                sb.AppendLine(r.Company);
                sb.AppendLine(r.Address);
                var title = "STOCK REPORT FOR THE PERIOD " + r.FromDate + " TO " + r.ToDate + "   [Unit: " + r.Units + "]";
                sb.AppendLine(title.PadRight(width - 8) + "Page:" + (p + 1).ToString().PadLeft(3));
                sb.AppendLine(rule);
                sb.AppendLine(TxtRow("Item", "Item", "", h1));
                sb.AppendLine(TxtRow("Code", "Description", "Type", h2));
                sb.AppendLine(rule);
                foreach (var line in body.Skip(p * perPage).Take(perPage)) sb.AppendLine(line);
            }
            return sb.ToString();
        }

        // VB6 generaterep ("Excel Upload"): Courier New, company / address / period on
        // rows 1-3, a two-row header on rows 5-6 frozen at A7, data from row 8, the
        // grand total row(s) bold between rules, negatives red and zeros blank. VB6
        // built 22 columns then deleted Load Out / Check In / Direct Sale, Purchase
        // Return and Adjustment..Total Out; the sheet here has the 13 that remained.
        private static XLWorkbook BuildWorkbook(StockReportResult r)
        {
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Stock Report");
            ws.Style.Font.FontName = "Courier New";

            ws.Cell(1, 1).Value = r.Company;
            ws.Cell(1, 1).Style.Font.FontSize = 15;
            ws.Cell(2, 1).Value = r.Address;
            ws.Cell(2, 1).Style.Font.FontSize = 12;
            ws.Cell(3, 1).Value = "Stock Report From " + r.FromDate + " TO " + r.ToDate + "   [Unit: " + r.Units + "]";

            string[] top = { "Item", "Item", "", "Opening", "", "", "Market", "", "", "", "Total", "Closing", "Gain/" };
            string[] bottom = { "Code", "Description", "Type", "Balance", "Production", "Purchase", "Return", "Total",
                                "Sale", "Transfers", "Despatch", "Stock", "Loss" };
            int cols = bottom.Length;
            for (int c = 0; c < cols; c++)
            {
                ws.Cell(5, c + 1).Value = top[c];
                ws.Cell(6, c + 1).Value = bottom[c];
            }
            ws.Range(5, 1, 6, cols).Style.Font.Bold = true;
            ws.SheetView.FreezeRows(6);

            int row = 8;
            Action<StockReportRow> write = x =>
            {
                ws.Cell(row, 1).SetValue(x.ITCD ?? "");
                ws.Cell(row, 2).Value = x.DESCN ?? "";
                ws.Cell(row, 3).Value = x.TypeText;
                decimal[] v = { x.OP, x.PROD, x.PURCH, x.MRETU, x.TOTAL_RECT, x.INDIRECT_SALE, x.TRANS, x.TOTAL_DESP, x.CL_STOCK, x.GAIN_LOSS };
                for (int k = 0; k < v.Length; k++) ws.Cell(row, 4 + k).Value = v[k];
                row++;
            };
            foreach (var x in r.Rows) write(x);
            int totFirst = row;
            foreach (var x in r.Totals) write(x);
            if (row > totFirst)
            {
                var tot = ws.Range(totFirst, 1, row - 1, cols);
                tot.Style.Font.Bold = true;
                tot.Style.Border.TopBorder = XLBorderStyleValues.None;
                ws.Range(totFirst, 1, totFirst, cols).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Range(row - 1, 1, row - 1, cols).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            }
            ws.Range(8, 4, Math.Max(8, row), cols).Style.NumberFormat.Format = "0.00;[Red]-0.00;";
            ws.Columns(1, cols).AdjustToContents(5, row);
            ws.Column(1).Width = Math.Max(ws.Column(1).Width, 8);
            return wb;
        }
    }
}
