using ClosedXML.Excel;
using PacketTea.Models;
using PacketTea.Models.PT;
using PacketTea.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Finance.Controllers.TEA
{
    // "Sample Gain Loss Register" -- ported from Classic ERP's Con_HO_04012100F. Parameters: From / To
    // date, Unit, Type. The old program's output was an Excel sheet (its Crystal layout was commented
    // out), so the outputs are Excel (the old sheet) and HTML (a printable page). The API
    // (SampleGainLoss/GetReport, company schema) builds the rows.
    public class SampleGainLossController : Controller
    {
        private const string ReportTitle = "Sample Gain Loss Register";

        private static List<string> Split(string s) =>
            (s ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(u => u.Trim())
                     .Where(u => u.Length > 0 && !u.Equals("All", StringComparison.OrdinalIgnoreCase)).ToList();

        public ActionResult Index()
        {
            return View();
        }

        // The Type list.
        public async Task<ActionResult> Types()
        {
            try
            {
                var response = await Services.GetAsync<List<SampleGainLossPick>>("/api/SampleGainLoss/GetTypes");
                if (!response.IsSuccessStatusCode || response.Data == null)
                    return Json(new { error = !string.IsNullOrEmpty(response.Message) ? response.Message : "Unable to load the types." }, JsonRequestBehavior.AllowGet);
                return Json(response.Data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = FullsStockActualController.ApiCallError(ex) }, JsonRequestBehavior.AllowGet);
            }
        }

        private async Task<(SampleGainLossResult Report, string Error)> LoadAsync(string fromDate, string toDate, string units, string tranCode)
        {
            ResponseApiModel<SampleGainLossResult> response;
            try
            {
                var u = Split(units);
                response = await Services.PostAsync<SampleGainLossResult>("/api/SampleGainLoss/GetReport",
                    new { FromDate = fromDate, ToDate = toDate, Units = u.Count == 0 ? null : u, TranCode = tranCode });
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

        private static string CompanyLine(SampleGainLossResult r) =>
            !string.IsNullOrWhiteSpace(r.CompanyV) && r.CompanyV.Trim().Length > 1 ? r.CompanyV.Trim() : (r.Company ?? "");

        private static string Title(SampleGainLossResult r)
        {
            string F(string s) => DateTime.TryParseExact(s ?? "", "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture,
                                                          System.Globalization.DateTimeStyles.None, out var d) ? d.ToString("dd-MMM-yyyy") : s;
            return ReportTitle + " From " + F(r.FromDate) + " To " + F(r.ToDate);
        }

        public async Task<ActionResult> Html(string fromDate, string toDate, string units, string tranCode)
        {
            var (report, error) = await LoadAsync(fromDate, toDate, units, tranCode);
            if (error != null)
            {
                ViewBag.Error = error;
                return View("Html", new SampleGainLossResult());
            }
            ViewBag.Title = Title(report);
            ViewBag.RunBy = SessionHelper.GetUser()?.getUserName ?? "";
            ViewBag.RunAt = DateTime.Now.ToString("dd-MMM-yyyy HH:mm");
            return View("Html", report);
        }

        public async Task<ActionResult> Excel(string fromDate, string toDate, string units, string tranCode)
        {
            var (report, error) = await LoadAsync(fromDate, toDate, units, tranCode);
            if (error != null)
                return Json(new { success = false, message = error }, JsonRequestBehavior.AllowGet);
            try
            {
                using (var wb = BuildWorkbook(report))
                {
                    var name = "SampleGainLossRegister-" + (report.FromDate ?? "").Replace("/", "") + "-" + (report.ToDate ?? "").Replace("/", "") + ".xlsx";
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

        // The old sheet: company / address / title, a 15-column header and the lines with a Total line.
        private static XLWorkbook BuildWorkbook(SampleGainLossResult r)
        {
            var wb = new XLWorkbook();
            wb.Style.Font.FontName = "Calibri";
            var ws = wb.Worksheets.Add("SampleGainLossRegister");
            ws.Cell(1, 1).Value = CompanyLine(r);
            ws.Cell(2, 1).Value = r.Address ?? "";
            ws.Cell(3, 1).Value = Title(r);
            ws.Range(1, 1, 3, 15).Style.Font.Bold = true;
            string[] head = { "SAMPLE LETTER NO.", "SAMPLE LETTER DATE ", "PACKING NO.", "PACKING DATE", "BLEND NO.", "SALE BILL NO.", "GARDEN", "INV NO.",
                              "SAMPLE(KG)", "GAIN(KG)", "LOSS(KG)", "AWR NO.", "AWR DATE", "SHORT QTY", "MFG DATE" };
            for (int c = 0; c < head.Length; c++) ws.Cell(4, c + 1).Value = head[c];
            var hr = ws.Range(4, 1, 4, head.Length);
            hr.Style.Font.Bold = true;
            hr.Style.Fill.BackgroundColor = XLColor.FromHtml("#404E67");
            hr.Style.Font.FontColor = XLColor.White;
            hr.Style.Alignment.WrapText = true;
            ws.SheetView.FreezeRows(4);

            int row = 5;
            foreach (var x in r.Rows)
            {
                var v = new object[] { x.SAMPLE_LETTER_NO, x.SAMPLE_LETTER_DATE, x.PACKING_NO, x.PACKING_DATE, x.BLEND_NO, x.SALE_BILL_NO, x.GARDEN, x.INVNO,
                                       x.SAMPLE, x.GAIN, x.LOSS, x.AWR_NO, x.AWR_DATE, x.SHORT_QTY, x.MFG_DATE };
                for (int c = 0; c < v.Length; c++)
                {
                    if (v[c] is decimal d) ws.Cell(row, c + 1).Value = d;
                    else ws.Cell(row, c + 1).SetValue(Convert.ToString(v[c]) ?? "");
                }
                row++;
            }
            ws.Cell(row, 2).Value = "Total";
            ws.Cell(row, 9).Value = r.Rows.Sum(x => x.SAMPLE);
            ws.Cell(row, 10).Value = r.Rows.Sum(x => x.GAIN);
            ws.Cell(row, 11).Value = r.Rows.Sum(x => x.LOSS);
            ws.Cell(row, 14).Value = r.Rows.Sum(x => x.SHORT_QTY);
            var tr = ws.Range(row, 1, row, head.Length);
            tr.Style.Font.Bold = true;
            tr.Style.Fill.BackgroundColor = XLColor.FromHtml("#DFE3EC");
            tr.Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Range(5, 9, row, 11).Style.NumberFormat.Format = "#####0.00;-#####0.00; ";
            ws.Range(5, 14, row, 14).Style.NumberFormat.Format = "#####0.00;-#####0.00; ";
            ws.Columns(1, head.Length).AdjustToContents(4, row);
            ws.Row(4).Height = 32;
            return wb;
        }
    }
}
