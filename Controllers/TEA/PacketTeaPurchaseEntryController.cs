
using Finance.Models.PT;
using PacketTea.Models;
using PacketTea.Models.PT;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static PacketTea.Helpers;

namespace PacketTea.Controllers.TEA
{
    public class PacketTeaPurchaseEntryController : Controller
    {
        // GET: PacketTeaPurchaseEntry
        public async Task<ActionResult> Index(string sortOrder, string currentFilter, string searchString, int? page = 1, int pageSize = 15)
        {
            {
                var sdsd = (List<AEDV>)Session["User_AEDV"];
                ViewBag.Permission = sdsd?.FirstOrDefault(l => l.Controller == "PacketTeaPurchaseEntry");
                ViewBag.CurrentSort = sortOrder;
                ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
                ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";
                ViewBag.CurrentFilter = searchString;
                ViewBag.PageSize = pageSize;
                int pageNumber = (page ?? 1);

                ViewBag.Page = pageNumber;

                // Fetch PO Head data
                string SelectedfinancialYear = Session["SelectedfinancialYear"].ToString();


                // Split at '-'
                string fy = Session["SelectedfinancialYear"].ToString();

                // Split at hyphen
                var parts = fy.Split('-');

                // Remove all non-digits and get the year
                string startDigits = new string(parts[0].Where(char.IsDigit).ToArray());
                string endDigits = new string(parts[1].Where(char.IsDigit).ToArray());

                // Take the last 4 digits as year0
                int startYear = int.Parse(startDigits.Substring(startDigits.Length - 4));
                int endYear = int.Parse(endDigits.Substring(endDigits.Length - 4));


                string startdate = startYear.ToString();
                string enddate = endYear.ToString();

                // ✅ Add these - FY runs April 1 to March 31



                var hoResponse = await Services.GetAsync<PageModel<TeaPurchase_LISTING>>($"/api/TeaPurchase/GetByPage?page={page}&pageSize={pageSize}&search={searchString}");


                var hoList = hoResponse.Data?.value?.results?.ToList()
                             ?? new List<TeaPurchase_LISTING>();

                var pagedList = new StaticPagedList<TeaPurchase_LISTING>(
                    hoList, pageNumber, pageSize, hoResponse.Data.value.rowCount
                );


                if (Request.IsAjaxRequest())
                {
                    return PartialView("_TeaPurchase_LIST", pagedList);
                }
                return View(pagedList);
            }
        }

        public async Task<JsonResult> GetDetailJson(string unit, string desp_date, string docno)
        {
            try
            {
                string formattedDate = desp_date;
                if (!string.IsNullOrEmpty(desp_date))
                {
                    DateTime dt = Convert.ToDateTime(desp_date);
                    formattedDate = dt.ToString("dd-MM-yyyy");
                }

                var detResponse = await Services.GetAsync<List<T_TEA_PURCHASE>>(
                    $"/api/TeaPurchase/GetByDocno?unit={unit}&desp_date={formattedDate}&docno={docno}"
                );

                var list = detResponse?.Data ?? new List<T_TEA_PURCHASE>();

                return Json(list, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetDetailJson Error: {ex.Message}");
                return Json(new List<T_TEA_PURCHASE>(), JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<ActionResult> InsertOrUpdate(string id = "", string unit = "", string doctype = "")
        {
            // ===============================
            // 🔥 FINANCIAL YEAR SETUP (FIX)
            // ===============================
            string fy = Session["SelectedfinancialYear"]?.ToString();

            if (!string.IsNullOrEmpty(fy))
            {
                var parts = fy.Split('-');

                string startDigits = new string(parts[0].Where(char.IsDigit).ToArray());
                string endDigits = new string(parts[1].Where(char.IsDigit).ToArray());

                int startYear = int.Parse(startDigits.Substring(startDigits.Length - 4));
                int endYear = int.Parse(endDigits.Substring(endDigits.Length - 4));

                DateTime tempStart, tempEnd;

                int startDay = 1, startMonth = 4;
                int endDay = 31, endMonth = 3;

                if (DateTime.TryParse(parts[0], out tempStart))
                {
                    startDay = tempStart.Day;
                    startMonth = tempStart.Month;
                }

                if (DateTime.TryParse(parts[1], out tempEnd))
                {
                    endDay = tempEnd.Day;
                    endMonth = tempEnd.Month;
                }

                DateTime startdate = new DateTime(startYear, startMonth, startDay);
                DateTime enddate = new DateTime(endYear, endMonth, endDay);

                ViewBag.StartDate = startdate.ToString("dd-MM-yyyy");
                ViewBag.EndDate = enddate.ToString("dd-MM-yyyy");
            }


            //if (string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(unit) && !string.IsNullOrEmpty(doctype))
            //{
            //    return View(new Trn_Po_Table
            //    {
            //        TRN_PO_HEAD = new TRN_PO_HEAD
            //        {
            //            UNIT = unit,
            //            DOCTYPE = doctype
            //        },
            //        TRN_PO_DETAIL = new List<TRN_PO_DETAIL>()
            //    });
            //}

            //// ===============================
            //// ⭐ PURE NEW ENTRY
            //// ===============================
            //if (string.IsNullOrEmpty(id))
            //{
            //    return View(new Trn_Po_Table
            //    {
            //        TRN_PO_HEAD = new TRN_PO_HEAD(),
            //        TRN_PO_DETAIL = new List<TRN_PO_DETAIL>()
            //    });
            //}

            //// ===============================
            //// ⭐ EDIT MODE
            //// ===============================
            //var sss = Utility.Cryptography.Decrypt(id);
            //id = sss;

            //// LOAD HEAD
            //var headResponse = await Services.GetAsync<TRN_PO_HEAD>(
            //    $"/api/Inv_TrnPo/GetByID2?id={id}"
            //);

            ////if (!headResponse.IsSuccessStatusCode)
            ////{
            ////    TempData["toastrError"] =
            ////        headResponse.Message ?? "Unable to load PO data.";

            ////    return RedirectToAction("Login", "Auth");
            ////}
            //if (!headResponse.IsSuccessStatusCode)
            //{
            //    string message = headResponse.Message;

            //    if (!string.IsNullOrEmpty(message))
            //    {
            //        message = message.Replace("### Message :-", "").Trim();

            //        int index = message.IndexOf("### InnerException :-");
            //        if (index >= 0)
            //        {
            //            message = message.Substring(0, index).Trim();
            //        }
            //    }

            //    TempData["ErrorMessage"] = message;

            //    if (!headResponse.IsLocked)
            //    {
            //        return RedirectToAction("Index", "PurchaseOrder");
            //    }
            //}

            //var docNo = headResponse.Data?.DOCNO;

            //var pr = GetAEDVPermission();
            //if (!pr.Edit)
            //{
            //    TempData["toastrWarning"] = $"{docNo} Edit Not Allowed.";
            //    return RedirectToAction("Index", "PurchaseOrder");
            //}

            //TRN_PO_HEAD head = headResponse?.Data ?? new TRN_PO_HEAD();


            //// LOAD DETAILS
            //List<TRN_PO_DETAIL> details = new List<TRN_PO_DETAIL>();

            //if (!string.IsNullOrEmpty(head.DOCNO))
            //{
            //    var detailResponse = await Services.GetAsync<List<TRN_PO_DETAIL>>(
            //        $"/api/Inv_TrnPo/GetByDocno?docno={head.DOCNO}&doctype={head.DOCTYPE}&unit={head.UNIT}&docdt={head.DOCDT}&docyear={head.DOC_YEAR}"
            //    );

            //    details = detailResponse?.Data ?? new List<TRN_PO_DETAIL>();
            //}

            // VIEW MODEL
            //var editModel = new Trn_Po_Table
            //{
            //    TRN_PO_HEAD = head,
            //    TRN_PO_DETAIL = details
            //};

            return View();
        }



    }
}