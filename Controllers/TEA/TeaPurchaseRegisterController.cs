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
    // "Tea Purchase Register" -- ported from Classic ERP's Con_HO_04011600F. Parameters: From / To date,
    // Unit, Purchase Type, Document No, Purchase Location, Own / Other / Both Garden and the report
    // Details / Party Wise / Summary. Graphics = Crystal PDF from the old layouts
    // (~/CrystalReport/TeaPurchase.rpt, TeaPurchasePartyWise.rpt, TeaPurchasePartyWiseSummary.rpt),
    // HTML = printable page, Excel = the lines as a sheet. The API
    // (TeaPurchaseRegister/GetReport, company schema) builds the rows.
    public class TeaPurchaseRegisterController : Controller
    {
        private static List<string> Split(string s) =>
            (s ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(u => u.Trim())
                     .Where(u => u.Length > 0 && !u.Equals("All", StringComparison.OrdinalIgnoreCase)).ToList();

        private static object OrNull(List<string> l) => l.Count == 0 ? null : l;

        public ActionResult Index()
        {
            return View();
        }

        // Purchase types, locations and the documents of the period / units / purchase type.
        public async Task<ActionResult> Lookups(string fromDate, string toDate, string units, string purType)
        {
            try
            {
                var response = await Services.PostAsync<TeaPurchaseLookups>("/api/TeaPurchaseRegister/GetLookups",
                    new { FromDate = fromDate, ToDate = toDate, Units = OrNull(Split(units)), PurType = purType });
                if (!response.IsSuccessStatusCode || response.Data == null)
                    return Json(new { error = !string.IsNullOrEmpty(response.Message) ? response.Message : "Unable to load the lists." }, JsonRequestBehavior.AllowGet);
                return Json(response.Data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = FullsStockActualController.ApiCallError(ex) }, JsonRequestBehavior.AllowGet);
            }
        }

        private async Task<(TeaPurchaseRegisterResult Report, string Error)> LoadAsync(string fromDate, string toDate, string units, string purType,
                                                                                        string docNos, string locations, string gardenOpt, string reportOpt)
        {
            ResponseApiModel<TeaPurchaseRegisterResult> response;
            try
            {
                response = await Services.PostAsync<TeaPurchaseRegisterResult>("/api/TeaPurchaseRegister/GetReport", new
                {
                    FromDate = fromDate, ToDate = toDate, Units = OrNull(Split(units)), PurType = purType,
                    DocNos = OrNull(Split(docNos)), Locations = OrNull(Split(locations)),
                    GardenOpt = string.IsNullOrEmpty(gardenOpt) ? "both" : gardenOpt, ReportOpt = string.IsNullOrEmpty(reportOpt) ? "Opt1" : reportOpt
                });
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

        private static string CompanyLine(TeaPurchaseRegisterResult r) =>
            !string.IsNullOrWhiteSpace(r.CompanyV) && r.CompanyV.Trim().Length > 1 ? r.CompanyV.Trim() : (r.Company ?? "");

        private static string FormatDate(string s) =>
            DateTime.TryParseExact(s ?? "", "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var d)
                ? d.ToString("dd/MMM/yyyy") : s;

        // The old titles: "<purchase type> - Tea Purchase Register [Party wise Details|Party wise Summary] (Own Garden) For the Period ..".
        private static string Title(TeaPurchaseRegisterResult r, string reportOpt, string gardenOpt)
        {
            var kind = reportOpt == "Opt2" ? " Party wise Details" : reportOpt == "Opt3" ? " Party wise Summary" : "";
            var garden = gardenOpt == "own" ? " (Own Garden)" : gardenOpt == "other" ? " (Other Garden)" : " (Both Garden)";
            var type = string.IsNullOrEmpty(r.PurTypeText) ? "" : r.PurTypeText + " - ";
            return type + "Tea Purchase Register" + kind + garden + " For the Period " + FormatDate(r.FromDate) + " To " + FormatDate(r.ToDate);
        }

        private static string AcYear(TeaPurchaseRegisterResult r)
        {
            if (!DateTime.TryParseExact(r.FromDate ?? "", "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var d))
                return "";
            var y = d.Month >= 4 ? d.Year : d.Year - 1;
            return "A/C YEAR : " + y + "-" + (y + 1);
        }

        public async Task<ActionResult> Html(string fromDate, string toDate, string units, string purType, string docNos, string locations, string gardenOpt, string reportOpt)
        {
            var (report, error) = await LoadAsync(fromDate, toDate, units, purType, docNos, locations, gardenOpt, reportOpt);
            if (error != null)
            {
                ViewBag.Error = error;
                return View("Html", new TeaPurchaseRegisterResult());
            }
            ViewBag.Title = Title(report, reportOpt, gardenOpt);
            ViewBag.ReportOpt = string.IsNullOrEmpty(reportOpt) ? "Opt1" : reportOpt;
            ViewBag.AcYear = AcYear(report);
            ViewBag.RunBy = SessionHelper.GetUser()?.getUserName ?? "";
            ViewBag.RunAt = DateTime.Now.ToString("dd-MMM-yyyy HH:mm");
            return View("Html", report);
        }

        public async Task<ActionResult> Graphics(string fromDate, string toDate, string units, string purType, string docNos, string locations, string gardenOpt, string reportOpt)
        {
            var file = reportOpt == "Opt2" ? "TeaPurchasePartyWise.rpt" : reportOpt == "Opt3" ? "TeaPurchasePartyWiseSummary.rpt" : "TeaPurchase.rpt";
            var rpt = Server.MapPath("~/CrystalReport/" + file);
            if (!System.IO.File.Exists(rpt))
                return FullsStockActualController.CrystalMissing(file, Url.Action("Html", new { fromDate, toDate, units, purType, docNos, locations, gardenOpt, reportOpt }));

            var (report, error) = await LoadAsync(fromDate, toDate, units, purType, docNos, locations, gardenOpt, reportOpt);
            if (error != null)
                return Content("<div style='font-family:Calibri,Arial;margin:40px'><b>Tea Purchase Register</b><br/><br/>" +
                               HttpUtility.HtmlEncode(error) + "</div>", "text/html");
            try
            {
                var pdf = CrystalPdf.Render(rpt, TeaPurchaseCrystal.Build(report), new Dictionary<string, string>
                {
                    ["Header0"] = CompanyLine(report),
                    ["Header1"] = Title(report, reportOpt, gardenOpt),
                    ["Header2"] = AcYear(report),
                    ["Header4"] = report.TotalRound.ToString("0.00"),
                    ["ReportFooter"] = "Report: Tea Purchase Register   user: " + (SessionHelper.GetUser()?.getUserName ?? "") +
                                       "   Time: " + DateTime.Now.ToString("HH:mm") + "   Date: " + DateTime.Now.ToString("dd-MMM-yyyy"),
                });
                Response.AppendHeader("Content-Disposition", "inline; filename=TeaPurchaseRegister.pdf");
                return File(pdf, "application/pdf");
            }
            catch (Exception ex)
            {
                return Content("Crystal report error:\n\n" + ex, "text/plain");
            }
        }

        public async Task<ActionResult> Excel(string fromDate, string toDate, string units, string purType, string docNos, string locations, string gardenOpt, string reportOpt)
        {
            var (report, error) = await LoadAsync(fromDate, toDate, units, purType, docNos, locations, gardenOpt, reportOpt);
            if (error != null)
                return Json(new { success = false, message = error }, JsonRequestBehavior.AllowGet);
            try
            {
                using (var wb = BuildWorkbook(report, reportOpt, gardenOpt))
                {
                    var name = "TeaPurchaseRegister-" + (report.FromDate ?? "").Replace("/", "") + "-" + (report.ToDate ?? "").Replace("/", "") + ".xlsx";
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

        private static XLWorkbook BuildWorkbook(TeaPurchaseRegisterResult r, string reportOpt, string gardenOpt)
        {
            var wb = new XLWorkbook();
            wb.Style.Font.FontName = "Calibri";
            var ws = wb.Worksheets.Add("Tea Purchase Register");
            ws.Cell(1, 1).Value = CompanyLine(r);
            ws.Cell(2, 1).Value = Title(r, reportOpt, gardenOpt);
            ws.Range(1, 1, 2, 10).Style.Font.Bold = true;

            bool detail = reportOpt != "Opt2" && reportOpt != "Opt3";
            string[] head = detail
                ? new[] { "Doc No", "Doc Date", "Bill No", "Bill Date", "Party", "Garden", "Invoice No", "Grade", "Bags", "Qty", "Rate", "Amount",
                          "CGST", "SGST", "IGST", "Misc", "Round Off", "Total", "Sale Centre", "Allocation" }
                : new[] { "Party", "Doc No", "Bill No", "Bill Date", "Qty", "Amount", "CGST", "SGST", "IGST", "TCS", "Round Off", "Total", "Allocation" };
            for (int c = 0; c < head.Length; c++) ws.Cell(4, c + 1).Value = head[c];
            ws.Range(4, 1, 4, head.Length).Style.Font.Bold = true;
            ws.Range(4, 1, 4, head.Length).Style.Fill.BackgroundColor = XLColor.FromHtml("#404E67");
            ws.Range(4, 1, 4, head.Length).Style.Font.FontColor = XLColor.White;
            ws.SheetView.FreezeRows(4);

            int row = 5;
            foreach (var x in r.Rows)
            {
                var v = detail
                    ? new object[] { x.DOCNO, x.DOCDT, x.BILLNO, x.BILLDT, x.PARTY, x.GARDEN, x.INVNO, x.GRADE_NAME, x.BAG_CHEST, x.QTY, x.RATE, x.AMOUNT,
                                     x.CGST_AMT, x.SGST_AMT, x.IGST_AMT, x.MISC_CHGS, x.ROUNDOFF, x.TOT_AMT, x.SALECENTRE, x.ALLOCATION }
                    : new object[] { x.PARTY, x.DOCNO, x.BILLNO, x.BILLDT, x.QTY, x.AMOUNT, x.CGST_AMT, x.SGST_AMT, x.IGST_AMT, x.TCS_AMT, x.ROUNDOFF, x.TOT_AMT, x.ALLOCATION };
                for (int c = 0; c < v.Length; c++)
                {
                    if (v[c] is decimal d) ws.Cell(row, c + 1).Value = d;
                    else ws.Cell(row, c + 1).SetValue(Convert.ToString(v[c]) ?? "");
                }
                row++;
            }
            ws.Cell(row, 1).Value = "Total";
            ws.Range(row, 1, row, head.Length).Style.Font.Bold = true;
            int qtyCol = detail ? 10 : 5;
            ws.Cell(row, qtyCol).Value = r.Rows.Sum(x => x.QTY);
            ws.Cell(row, detail ? 12 : 6).Value = r.Rows.Sum(x => x.AMOUNT);
            ws.Cell(row, detail ? 18 : 12).Value = r.Rows.Sum(x => x.TOT_AMT);
            ws.Columns(1, head.Length).AdjustToContents(4, row);
            return wb;
        }
    }
}
