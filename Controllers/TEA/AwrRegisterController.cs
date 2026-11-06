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
    // "AWR Register" -- ported from Classic ERP's Con_HO_03030206T. Parameters: From / To date, Unit,
    // AWR From / To, Warehouse, Garden, Report option Garden Wise / Seller Wise (+ Seller). Graphics =
    // Crystal PDF from the old layouts (~/CrystalReport/AWRRegister.rpt, AWRRegisterSellerWise.rpt),
    // HTML = printable page, Excel = the old 16-column sheet. "CTTA Excel" is not ported.
    // The API (AwrRegister/GetReport, company schema: T_AWR) builds the rows.
    public class AwrRegisterController : Controller
    {
        private static List<string> Split(string s) =>
            (s ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(u => u.Trim())
                     .Where(u => u.Length > 0 && !u.Equals("All", StringComparison.OrdinalIgnoreCase)).ToList();

        private static object OrNull(List<string> l) => l.Count == 0 ? null : l;

        public ActionResult Index()
        {
            return View();
        }

        // Garden / Warehouse / Seller lists (a seller narrows the gardens, as the old GetSellerList).
        public async Task<ActionResult> Lookups(string seller)
        {
            try
            {
                var url = "/api/AwrRegister/GetLookups" + (string.IsNullOrWhiteSpace(seller) ? "" : "?seller=" + Uri.EscapeDataString(seller));
                var response = await Services.GetAsync<AwrRegisterLookups>(url);
                if (!response.IsSuccessStatusCode || response.Data == null)
                    return Json(new { error = !string.IsNullOrEmpty(response.Message) ? response.Message : "Unable to load the lists." }, JsonRequestBehavior.AllowGet);
                return Json(response.Data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = FullsStockActualController.ApiCallError(ex) }, JsonRequestBehavior.AllowGet);
            }
        }

        private async Task<(AwrRegisterResult Report, string Error)> LoadAsync(string fromDate, string toDate, string units, string awrFrom, string awrTo,
                                                                                string gardens, string warehouses, string sellers)
        {
            ResponseApiModel<AwrRegisterResult> response;
            try
            {
                response = await Services.PostAsync<AwrRegisterResult>("/api/AwrRegister/GetReport", new
                {
                    FromDate = fromDate, ToDate = toDate, Units = OrNull(Split(units)), AwrFrom = awrFrom, AwrTo = awrTo,
                    Gardens = OrNull(Split(gardens)), Warehouses = OrNull(Split(warehouses)), Sellers = OrNull(Split(sellers))
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

        private static string CompanyLine(AwrRegisterResult r) =>
            !string.IsNullOrWhiteSpace(r.CompanyV) && r.CompanyV.Trim().Length > 1 ? r.CompanyV.Trim() : (r.Company ?? "");

        private static string Title(AwrRegisterResult r, bool sellerWise) =>
            "AWR Register" + (sellerWise ? " Seller Wise" : "") + " From " + FormatDate(r.FromDate) + " To " + FormatDate(r.ToDate);

        private static string FormatDate(string s) =>
            DateTime.TryParseExact(s ?? "", "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var d)
                ? d.ToString("dd-MMM-yyyy") : s;

        public async Task<ActionResult> Html(string fromDate, string toDate, string units, string awrFrom, string awrTo,
                                             string gardens, string warehouses, string sellers, string reportOpt)
        {
            var sellerWise = reportOpt == "sellerwise";
            var (report, error) = await LoadAsync(fromDate, toDate, units, awrFrom, awrTo, gardens, warehouses, sellers);
            if (error != null)
            {
                ViewBag.Error = error;
                return View("Html", new AwrRegisterResult());
            }
            ViewBag.Title = Title(report, sellerWise);
            ViewBag.SellerWise = sellerWise;
            ViewBag.RunBy = SessionHelper.GetUser()?.getUserName ?? "";
            ViewBag.RunAt = DateTime.Now.ToString("dd-MMM-yyyy HH:mm");
            return View("Html", report);
        }

        public async Task<ActionResult> Graphics(string fromDate, string toDate, string units, string awrFrom, string awrTo,
                                                 string gardens, string warehouses, string sellers, string reportOpt)
        {
            var sellerWise = reportOpt == "sellerwise";
            var file = sellerWise ? "AWRRegisterSellerWise.rpt" : "AWRRegister.rpt";
            var rpt = Server.MapPath("~/CrystalReport/" + file);
            if (!System.IO.File.Exists(rpt))
                return FullsStockActualController.CrystalMissing(file, Url.Action("Html", new { fromDate, toDate, units, awrFrom, awrTo, gardens, warehouses, sellers, reportOpt }));

            var (report, error) = await LoadAsync(fromDate, toDate, units, awrFrom, awrTo, gardens, warehouses, sellers);
            if (error != null)
                return Content("<div style='font-family:Calibri,Arial;margin:40px'><b>AWR Register</b><br/><br/>" +
                               HttpUtility.HtmlEncode(error) + "</div>", "text/html");
            try
            {
                var line1 = CompanyLine(report);
                var pdf = CrystalPdf.Render(rpt, AwrRegisterCrystal.Build(report), new Dictionary<string, string>
                {
                    ["Header1"] = line1,
                    ["Header2"] = line1 == report.Company ? "" : report.Company,
                    ["Header3"] = Title(report, sellerWise),
                    ["Header4"] = report.Address ?? "",
                    ["ReportFooter"] = "Report: AWR Register   user: " + (SessionHelper.GetUser()?.getUserName ?? "") +
                                       "   Time: " + DateTime.Now.ToString("HH:mm") + "   Date: " + DateTime.Now.ToString("dd-MMM-yyyy"),
                });
                Response.AppendHeader("Content-Disposition", "inline; filename=AWRRegister.pdf");
                return File(pdf, "application/pdf");
            }
            catch (Exception ex)
            {
                return Content("Crystal report error:\n\n" + ex, "text/plain");
            }
        }

        public async Task<ActionResult> Excel(string fromDate, string toDate, string units, string awrFrom, string awrTo,
                                              string gardens, string warehouses, string sellers, string reportOpt)
        {
            var (report, error) = await LoadAsync(fromDate, toDate, units, awrFrom, awrTo, gardens, warehouses, sellers);
            if (error != null)
                return Json(new { success = false, message = error }, JsonRequestBehavior.AllowGet);
            try
            {
                using (var wb = BuildWorkbook(report, reportOpt == "sellerwise"))
                {
                    var name = "AWRRegister-" + (report.FromDate ?? "").Replace("/", "") + "-" + (report.ToDate ?? "").Replace("/", "") + ".xlsx";
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

        // The old sheet: AWR No, AWR Date, Lorry No, Carrier, CN No, CN Date, Garden, Invoice No, Grade,
        // Rec Bag, Rec Kgs, Short Bag, Short Kgs, Sample Kgs, Remarks, Seller (rows ordered as the report).
        private static XLWorkbook BuildWorkbook(AwrRegisterResult r, bool sellerWise)
        {
            var wb = new XLWorkbook();
            wb.Style.Font.FontName = "Calibri";
            var ws = wb.Worksheets.Add("Sales");
            ws.Cell(1, 1).Value = CompanyLine(r);
            ws.Cell(2, 1).Value = Title(r, sellerWise);
            ws.Range(1, 1, 2, 12).Style.Font.Bold = true;

            string[] head = { "AWR NO", "AWR Date", "Lorry NO ", "Carrier", "CN NO", "CN Date", "Garden", "Inovice No", "Grade",
                              "REC Bag", "REC Kgs", "Short Bag", "Short Kgs", "Sample Kgs", "Remarks", "Seller" };
            for (int c = 0; c < head.Length; c++) ws.Cell(3, c + 1).Value = head[c];
            ws.Range(3, 1, 3, head.Length).Style.Font.Bold = true;
            ws.Range(3, 1, 3, head.Length).Style.Fill.BackgroundColor = XLColor.FromHtml("#404E67");
            ws.Range(3, 1, 3, head.Length).Style.Font.FontColor = XLColor.White;
            ws.SheetView.FreezeRows(3);

            int row = 4;
            foreach (var x in r.Rows)
            {
                var v = new object[] { x.AWR_NO, x.AWR_DATE, x.TNO, x.TransName, x.CONS_NO1, x.CONS_DT1, x.REC_NAME, x.INVNO, x.GRADE,
                                       x.BAG_RCVD, x.TNET_WT, x.SR_QTY, x.SR_KGS, x.SAMPLE_QTY, x.REMARK, x.SELLER };
                for (int c = 0; c < v.Length; c++)
                {
                    if (v[c] is decimal d) ws.Cell(row, c + 1).Value = d;
                    else ws.Cell(row, c + 1).SetValue(Convert.ToString(v[c]) ?? "");
                }
                row++;
            }
            ws.Columns(1, head.Length).AdjustToContents(3, row);
            return wb;
        }
    }
}
