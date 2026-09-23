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
    // "Fulls Stock Actual" report -- ported from VB6 rep_dank_tpm.frm. Parameters:
    // As On, Unit (All = no filter, or a checked list of manufacturing units), Order (Item Code / Item Name / Custom).
    // The API (FullsStockActual/GetReport, Sales schema) computes the stock;
    // Graphics renders a Crystal PDF (~/CrystalReport/FullsStockActual.rpt), HTML
    // renders the VB6 printed layout as a paged page, both in a new tab; Excel
    // writes the VB6 EXPORTTOEXCEL sheet with ClosedXML.
    public class FullsStockActualController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        // Used by both report pages. A failure used to come back as an empty list, which
        // looked like an empty Unit picker with no reason given -- the API's message is
        // returned instead so the page can show it.
        public async Task<JsonResult> GetUnits()
        {
            if (string.IsNullOrWhiteSpace(SessionHelper.GetUser()?.Salesdb))
                return Json(new { error = "Sales schema is not set for this company (CLASSIC_CONTROL.SCHEMA_SALES). Open the menu page again, or ask for the control file to be updated." }, JsonRequestBehavior.AllowGet);

            var response = await Services.SalesGetAsync<List<FullsStockUnit>>("/api/FullsStockActual/GetUnits");
            if (!response.IsSuccessStatusCode || response.Data == null)
                return Json(new { error = string.IsNullOrEmpty(response.Message) ? "Unable to load the unit list." : response.Message }, JsonRequestBehavior.AllowGet);
            return Json(response.Data, JsonRequestBehavior.AllowGet);
        }

        private async Task<(FullsStockReport Report, string Error)> LoadAsync(string asOn, string units, string order)
        {
            if (string.IsNullOrWhiteSpace(SessionHelper.GetUser()?.Salesdb))
                return (null, "Sales schema is not set for this company (CLASSIC_CONTROL.SCHEMA_SALES).");

            // "All" (or nothing picked) is sent as null = no unit filter.
            var unitList = (units ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(u => u.Trim())
                                        .Where(u => u.Length > 0 && !u.Equals("All", StringComparison.OrdinalIgnoreCase))
                                        .ToList();
            ResponseApiModel<FullsStockReport> response;
            try
            {
                response = await Services.SalesPostAsync<FullsStockReport>("/api/FullsStockActual/GetReport",
                    new { AsOn = asOn, Units = unitList.Count == 0 ? null : unitList, Order = order });
            }
            catch (Exception ex)
            {
                return (null, ApiCallError(ex));
            }
            if (!response.IsSuccessStatusCode || response.Data == null)
                return (null, !string.IsNullOrEmpty(response.Message) ? response.Message : "Unable to build the report.");
            if (response.Data.Items == null || response.Data.Items.Count == 0)
                return (null, "No Record To Print");
            return (response.Data, null);
        }

        // An exception here (API down, HttpClient's 100s timeout) used to escape as a 500
        // page, which Excel/Text could only report as "Unable to generate the report.".
        internal static string ApiCallError(Exception ex)
        {
            if (ex is TaskCanceledException || ex.GetBaseException() is TaskCanceledException)
                return "The report server did not answer in time. Try a single unit, or try again later.";
            return "Could not reach the report server: " + ex.GetBaseException().Message;
        }

        // HTML: the VB6 printed layout as a paged page (Views/FullsStockActual/Graphics.cshtml),
        // opened in a new tab by the Index page.
        public async Task<ActionResult> Html(string asOn, string units, string order)
        {
            var (report, error) = await LoadAsync(asOn, units, order);
            if (error != null)
            {
                ViewBag.Error = error;
                return View("Graphics", new FullsStockReport());
            }
            report.RunBy = SessionHelper.GetUser()?.getUserName ?? "";
            report.RunAt = DateTime.Now.ToString("dd-MMM-yyyy HH:mm");
            return View("Graphics", report);
        }

        // Graphics: Crystal PDF from ~/CrystalReport/FullsStockActual.rpt (data source
        // FullsStockCrystal / CrystalReport/FullsStockActual.xsd), shown inline in a new tab.
        public async Task<ActionResult> Graphics(string asOn, string units, string order)
        {
            var rpt = Server.MapPath("~/CrystalReport/FullsStockActual.rpt");
            if (!System.IO.File.Exists(rpt))
                return CrystalMissing("FullsStockActual.rpt", Url.Action("Html", new { asOn, units, order }));

            var (report, error) = await LoadAsync(asOn, units, order);
            if (error != null)
                return Content("<div style='font-family:Arial;margin:40px'><b>Fulls Stock Actual</b><br/><br/>" +
                               HttpUtility.HtmlEncode(error) + "</div>", "text/html");
            try
            {
                var pdf = CrystalPdf.Render(rpt, FullsStockCrystal.Build(report), new Dictionary<string, string>
                {
                    ["HEADER1"] = report.Company,
                    ["HEADER2"] = report.Address,
                    ["TITLE"] = "FULLS STOCK (Actual) CLOSING REPORT AS ON " + report.AsOn,
                    ["UNIT_TEXT"] = "Unit: " + report.Unit,
                    ["RUN_INFO"] = "User: " + (SessionHelper.GetUser()?.getUserName ?? "") + "   Run: " + DateTime.Now.ToString("dd-MMM-yyyy HH:mm"),
                });
                Response.AppendHeader("Content-Disposition", "inline; filename=FullsStockActual.pdf");
                return File(pdf, "application/pdf");
            }
            catch (Exception ex)
            {
                return Content("Crystal report error:\n\n" + ex, "text/plain");
            }
        }

        internal static ContentResult CrystalMissing(string rptName, string htmlUrl)
        {
            return new ContentResult
            {
                ContentType = "text/html",
                Content = "<div style='font-family:Arial;margin:40px;max-width:640px'><b>Crystal layout not designed yet</b><br/><br/>" +
                          "The file <code>CrystalReport/" + rptName + "</code> does not exist on the server. " +
                          "Design it in Visual Studio from the matching <code>.xsd</code> in the same folder." +
                          "<br/><br/><a href='" + htmlUrl + "'>Open the HTML version instead</a></div>"
            };
        }

        // Excel: earlier Excel files in C:\classic are deleted, the workbook is saved
        // there and opened in Excel (ClassicExcel). When it cannot be opened on this
        // machine the page downloads it through DownloadExcel instead.
        public async Task<ActionResult> Excel(string asOn, string units, string order)
        {
            var (report, error) = await LoadAsync(asOn, units, order);
            if (error != null)
                return Json(new { success = false, message = error }, JsonRequestBehavior.AllowGet);
            try
            {
                using (var wb = BuildWorkbook(report))
                {
                    var name = "FullsStockActual-" + (report.AsOn ?? "").Replace("/", "") + ".xlsx";
                    var r = ClassicExcel.SaveAndOpen(wb, name, Request);
                    return Json(new { success = true, opened = r.Opened, fileName = r.FileName, token = r.Token }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Text: VB6 "Character Format" (JAGREPORT1_DMPPRN) -- a fixed-width dot-matrix
        // listing saved to C:\classic and opened in the default .txt program; downloaded
        // through DownloadText when it cannot be opened on this machine.
        public async Task<ActionResult> Text(string asOn, string units, string order)
        {
            var (report, error) = await LoadAsync(asOn, units, order);
            if (error != null)
                return Json(new { success = false, message = error }, JsonRequestBehavior.AllowGet);
            try
            {
                var text = BuildText(report, SessionHelper.GetUser()?.getUserName ?? "");
                var name = "FullsStockActual-" + (report.AsOn ?? "").Replace("/", "") + ".txt";
                var r = ClassicExcel.SaveTextAndOpen(text, name, Request);
                return Json(new { success = true, opened = r.Opened, fileName = r.FileName, token = r.Token }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DownloadText(string token)
        {
            var bytes = ClassicExcel.Take(token, out var name);
            if (bytes == null) return HttpNotFound("The report has expired - please generate it again.");
            return File(bytes, "text/plain", name);
        }

        // VB6 DMPPRN column widths (ITCD 6, name 30, MRP 6, MFG 10, batch 10, qty,
        // gross 10, net 10, DOD 10, BBD 10, age 5, status 3), one space apart, with qty
        // at 11 (VB6: 14) so a row fits the 132-column condensed line; VB6's always-zero
        // rate and blank stock-type columns are left out. Header as VB6 CREATE_HEADER;
        // 66-line pages separated by a form feed.
        private static readonly int[] TxtW = { 6, 30, 6, 10, 10, 11, 10, 10, 10, 10, 5, 3 };
        private static readonly bool[] TxtRight = { false, false, false, false, false, true, true, true, false, false, true, false };
        private const int TxtPageLines = 66;

        private static string TxtRow(params string[] cells)
        {
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < TxtW.Length; i++)
            {
                var c = i < cells.Length ? (cells[i] ?? "") : "";
                if (c.Length > TxtW[i]) c = c.Substring(0, TxtW[i]);
                if (i > 0) sb.Append(' ');
                sb.Append(TxtRight[i] ? c.PadLeft(TxtW[i]) : c.PadRight(TxtW[i]));
            }
            return sb.ToString().TrimEnd();
        }

        private static string BuildText(FullsStockReport r, string user)
        {
            Func<decimal, string> n = v => v.ToString("0.00");
            var ruleTo = TxtW.Take(11).Sum() + 10;   // VB6 LN rows: columns 0..10
            var body = new List<string>();
            foreach (var it in r.Items)
            {
                var first = true;
                foreach (var l in it.Lines)
                {
                    body.Add(TxtRow(first ? it.ITCD : "", first ? it.NAME : "", l.MRP, l.MFGDT, l.BATCHNO,
                                    n(l.QTY), n(l.GROSS_WEIGHT), n(l.NET_WEIGHT), l.DOD, l.BBD,
                                    string.IsNullOrEmpty(l.MFGDT) ? "0" : l.AGE.ToString(), l.STATUS));
                    first = false;
                }
                body.Add(new string('-', ruleTo));
                body.Add(TxtRow("", "Total", "", "", "", n(it.QTY), n(it.GROSS_WEIGHT), n(it.NET_WEIGHT)));
                body.Add(new string('-', ruleTo));
            }
            body.Add(TxtRow("", "Grand Total", "", "", "", n(r.GRAND_QTY), n(r.GRAND_GROSS_WEIGHT), n(r.GRAND_NET_WEIGHT)));
            body.Add(new string('-', ruleTo));
            body.Add(TxtRow(" ** - ", "DOD Date over"));
            body.Add(TxtRow("*** - ", "BBD Date over"));
            body.Add("");
            body.Add("Report: Fulls Stock Actual   user: " + user + "   Time: " + DateTime.Now.ToString("HH:mm") + "   Date: " + DateTime.Now.ToString("dd-MMM-yyyy"));

            const int headLines = 7;
            var perPage = TxtPageLines - headLines;
            var pages = (body.Count + perPage - 1) / perPage;
            var sb = new System.Text.StringBuilder();
            for (int p = 0; p < pages; p++)
            {
                if (p > 0) sb.Append('\f');
                sb.AppendLine(r.Company);
                sb.AppendLine(r.Address);
                sb.AppendLine(("FULLS STOCK (Actual) CLOSING REPORT AS ON " + r.AsOn + "   Unit: " + r.Unit).PadRight(122) + "Page:" + (p + 1).ToString().PadLeft(3));
                sb.AppendLine(new string('-', 132));
                sb.AppendLine(TxtRow("ITEM", "", "", "", "", "QUANTITY", "GROSS", "NET", "DATE OF", "BEST"));
                sb.AppendLine(TxtRow("CODE", "Item Name", "MRP", "MFG DT", "BATCH NO", "PKTS/BAG", "WEIGHT", "WEIGHT", "DESPATCH", "BEFORE", "Age"));
                sb.AppendLine(new string('-', 132));
                foreach (var line in body.Skip(p * perPage).Take(perPage)) sb.AppendLine(line);
            }
            return sb.ToString();
        }

        public ActionResult DownloadExcel(string token)
        {
            var bytes = ClassicExcel.Take(token, out var name);
            if (bytes == null) return HttpNotFound("The report has expired - please generate it again.");
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", name);
        }

        // VB6 JAGREPORT1_EXPORTTOEXCEL: title rows, a two-row header (fill colour
        // index 34), frozen at A6, one row per batch with the item on every row,
        // status spelled out and BBD-over rows in bold red. VB6's always-zero
        // "Purchase Rate" and always-blank "Stock Type" columns are left out.
        private static XLWorkbook BuildWorkbook(FullsStockReport r)
        {
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Fulls Stock");
            ws.Style.Font.FontName = "Calibri";

            ws.Cell(1, 1).Value = r.Company;
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 18;
            var asOn = DateTime.TryParseExact(r.AsOn, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out var d)
                ? d.ToString("dd-MMM-yyyy") : r.AsOn;
            ws.Cell(2, 1).Value = "Fulls Stock (Actual) Report As On " + asOn;
            ws.Cell(2, 1).Style.Font.Bold = true;
            ws.Cell(2, 1).Style.Font.FontSize = 14;
            ws.Cell(3, 1).Value = r.Unit + " Units";
            ws.Cell(3, 1).Style.Font.Bold = true;
            ws.Cell(3, 1).Style.Font.FontSize = 14;

            string[] heads = { "Unit", "Code", "Description", "MRP", "TM", "MFG Date", "Batch No.", "Quantity",
                               "Gross Weight", "Net Weight", "DOD", "BBD", "Age", "Status" };
            double[] widths = { 7, 8, 32, 7, 5, 11, 12, 10, 12, 11, 11, 11, 7, 15 };
            int cols = heads.Length;
            ws.Cell(4, 2).Value = "Item";
            for (int c = 0; c < cols; c++)
            {
                ws.Cell(5, c + 1).Value = heads[c];
                ws.Column(c + 1).Width = widths[c];
            }
            var head = ws.Range(4, 1, 5, cols);
            head.Style.Font.Bold = true;
            head.Style.Font.FontSize = 10;
            head.Style.Fill.BackgroundColor = XLColor.FromArgb(0xCC, 0xFF, 0xFF);
            head.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            head.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.SheetView.FreezeRows(5);

            int row = 6;
            foreach (var it in r.Items)
            {
                foreach (var l in it.Lines)
                {
                    ws.Cell(row, 1).Value = l.UNIT;
                    ws.Cell(row, 2).SetValue(it.ITCD);
                    ws.Cell(row, 3).Value = it.NAME;
                    ws.Cell(row, 4).SetValue(l.MRP);
                    ws.Cell(row, 5).Value = l.TM;
                    ws.Cell(row, 6).SetValue(l.MFGDT);
                    ws.Cell(row, 7).SetValue(l.BATCHNO);
                    ws.Cell(row, 8).Value = l.QTY;
                    ws.Cell(row, 9).Value = l.GROSS_WEIGHT;
                    ws.Cell(row, 10).Value = l.NET_WEIGHT;
                    ws.Cell(row, 11).SetValue(l.DOD);
                    ws.Cell(row, 12).SetValue(l.BBD);
                    if (!string.IsNullOrEmpty(l.MFGDT)) ws.Cell(row, 13).Value = l.AGE;
                    ws.Cell(row, 14).Value = StatusText(l.STATUS);
                    if (l.STATUS == "***")
                    {
                        ws.Range(row, 1, row, cols).Style.Font.FontColor = XLColor.Red;
                        ws.Range(row, 1, row, cols).Style.Font.Bold = true;
                    }
                    row++;
                }
            }
            if (row > 6)
            {
                var body = ws.Range(6, 1, row - 1, cols);
                body.Style.Font.FontSize = 9;
                ws.Range(6, 8, row - 1, 10).Style.NumberFormat.Format = "0.00";
                ws.Range(6, 11, row - 1, 12).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Range(6, 6, row - 1, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }
            return wb;
        }

        private static string StatusText(string status)
        {
            switch (status)
            {
                case "***": return "BBD Date Over";
                case "**": return "DOD Date Over";
                case "*": return "IDOD Date Over";
                default: return "";
            }
        }
    }
}
