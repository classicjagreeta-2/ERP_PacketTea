
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
        public async Task<JsonResult> GetVendor(string q = "", int limit = 0, string fieldValue = "", string fieldText = "", string value = "", int p = 1)
        {
            if (string.IsNullOrEmpty(q))
            {
                q = value;
            }

            var resp = await Services.GetAsync<PageValue<M_PMAST>>(
                $"/api/TeaPurchase/GetVendor?search={q}&page={p}&pageSize={limit}"
            );

            var data = resp?.Data.results;

            return Json(new
            {
                data = data ?? new List<M_PMAST>(),
                count = resp?.Data.rowCount ?? 0
            }, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> GetMark(string q = "", int limit = 0, string fieldValue = "", string fieldText = "", string value = "", int p = 1)
        {
            if (string.IsNullOrEmpty(q))
            {
                q = value;
            }
            var resp = await Services.GetAsync<PageValue<M_MARK>>($"/api/TeaPurchase/GetMarkCode?search={q}&page={p}&pageSize={limit}");
            var data = resp?.Data.results;

            return Json(new
            {
                data = data ?? new List<M_MARK>(),
                count = resp?.Data.rowCount ?? 0
            }, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> GetGrade(string q = "", int limit = 0, string fieldValue = "", string fieldText = "", string value = "", int p = 1)
        {
            if (string.IsNullOrEmpty(q))
            {
                q = value;
            }
            var resp = await Services.GetAsync<PageValue<M_GRADE>>($"/api/TeaPurchase/GetMarkGrade?search={q}&page={p}&pageSize={limit}");
            var data = resp?.Data.results;

            return Json(new
            {
                data = data ?? new List<M_GRADE>(),
                count = resp?.Data.rowCount ?? 0
            }, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> GetLocation(string q = "", int limit = 0, string fieldValue = "", string fieldText = "", string value = "", int p = 1)
        {
            if (string.IsNullOrEmpty(q))
            {
                q = value;
            }
            var resp = await Services.GetAsync<PageValue<M_SALESCENTRE>>($"/api/TeaPurchase/GetLocation?search={q}&page={p}&pageSize={limit}");
            var data = resp?.Data.results;

            return Json(new
            {
                data = data ?? new List<M_SALESCENTRE>(),
                count = resp?.Data.rowCount ?? 0
            }, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetBroker( string q = "", int limit = 0, string fieldValue = "", string fieldText = "", string value = "", int p = 1)
        {
            if (string.IsNullOrEmpty(q))
            {
                q = value;
            }

            var resp = await Services.GetAsync<PageValue<M_Broker>>(
                $"/api/TeaPurchase/GetBroker?search={q}&page={p}&pageSize={limit}"
            );

            var data = resp?.Data.results;

            return Json(new
            {
                data = data ?? new List<M_Broker>(),
                count = resp?.Data.rowCount ?? 0
            }, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> GetTransp(string q = "", int limit = 0, string fieldValue = "", string fieldText = "", string value = "", int p = 1)
        {
            if (string.IsNullOrEmpty(q))
            {
                q = value;
            }
            var resp = await Services.GetAsync<PageValue<M_Transp>>($"/api/TeaPurchase/GetTransporter?search={q}&page={p}&pageSize={limit}");
            var data = resp?.Data.results;

            return Json(new
            {
                data = data ?? new List<M_Transp>(),
                count = resp?.Data.rowCount ?? 0
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddProductRow(int rowIndex)
        {
            ViewBag.RowIndex = rowIndex;

            var model = new List<PT_ALL_LIST>
    {
        new PT_ALL_LIST
        {
            T_TEA_DETAIL = new List<T_TEA_PURCHASE>
            {
                new T_TEA_PURCHASE()
            }
        }
    };

            return PartialView("_Product_Details", model);
        }
        public async Task<JsonResult> GetDetailJson(string unit, string docdt, string docno)
        {
            try
            {
                string formattedDate = docdt;

                if (!string.IsNullOrEmpty(docdt))
                {
                    DateTime dt = Convert.ToDateTime(docdt);
                    formattedDate = dt.ToString("yyyy-MM-dd");
                }

                var detResponse = await Services.GetAsync<List<T_TEA_PURCHASE>>(
                    $"/api/TeaPurchase/GetByDocNo?DOCNO={docno}&unit={unit}&docdt={formattedDate}"
                );

                var list = detResponse?.Data ?? new List<T_TEA_PURCHASE>();

                return Json(list, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetDetailJson Error: {ex}");

                return Json(new List<T_TEA_PURCHASE>(), JsonRequestBehavior.AllowGet);
            }
        }
        public async Task<ActionResult> InsertOrUpdate(string unit = "", string docdt = "", string docno = "")
        {
            // ===============================
            // FINANCIAL YEAR SETUP
            // ===============================
            string fy = Session["SelectedfinancialYear"]?.ToString();

            if (!string.IsNullOrEmpty(fy))
            {
                var parts = fy.Split('-');

                string startDigits = new string(
                    parts[0].Where(char.IsDigit).ToArray());

                string endDigits = new string(
                    parts[1].Where(char.IsDigit).ToArray());

                int startYear = int.Parse(
                    startDigits.Substring(startDigits.Length - 4));

                int endYear = int.Parse(
                    endDigits.Substring(endDigits.Length - 4));

                DateTime tempStart, tempEnd;

                int startDay = 1;
                int startMonth = 4;

                int endDay = 31;
                int endMonth = 3;

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

                DateTime startdate = new DateTime(
                    startYear,
                    startMonth,
                    startDay);

                DateTime enddate = new DateTime(
                    endYear,
                    endMonth,
                    endDay);

                ViewBag.StartDate = startdate.ToString("dd-MM-yyyy");
                ViewBag.EndDate = enddate.ToString("dd-MM-yyyy");
            }


            // ===============================
            // FORMAT DOCUMENT DATE
            // ===============================
            string formattedDate = docdt;

            if (!string.IsNullOrEmpty(docdt))
            {
                DateTime dt = Convert.ToDateTime(docdt);
                formattedDate = dt.ToString("yyyy-MM-dd");
            }


            // ===============================
            // NEW ENTRY
            // ===============================
            if (string.IsNullOrEmpty(docno))
            {
                return View(new PT_ALL_LIST
                {
                    T_TEA_HEAD = new T_TEA_PURCHASE
                    {
                        UNIT = unit
                    },

                    T_TEA_DETAIL = new List<T_TEA_PURCHASE>()
                });
            }


            // ===============================
            // VALIDATE PARAMETERS
            // ===============================
            if (string.IsNullOrEmpty(unit) ||
                string.IsNullOrEmpty(formattedDate))
            {
                TempData["toastrError"] =
                    "Document No, Unit and Document Date are required.";

                return RedirectToAction("Index");
            }


            // ===============================
            // GET HEADER + DETAIL
            // ===============================
            var response =
                await Services.GetAsync<List<T_TEA_PURCHASE>>(
                    $"/api/TeaPurchase/GetByDocNo" +
                    $"?DOCNO={HttpUtility.UrlEncode(docno)}" +
                    $"&unit={HttpUtility.UrlEncode(unit)}" +
                    $"&docdt={HttpUtility.UrlEncode(formattedDate)}"
                );


            // ===============================
            // CHECK RESPONSE
            // ===============================
            if (response == null || response.Data == null)
            {
                TempData["toastrError"] =
                    "Unable to load Tea Purchase data.";

                return RedirectToAction("Index");
            }


            // ===============================
            // SEPARATE HEAD + DETAIL
            // ===============================
            var data = response.Data;

            T_TEA_PURCHASE head =
                data.FirstOrDefault() ?? new T_TEA_PURCHASE();

            List<T_TEA_PURCHASE> details =
                data;


            // ===============================
            // CREATE VIEW MODEL
            // ===============================
            var editModel = new PT_ALL_LIST
            {
                T_TEA_HEAD = head,
                T_TEA_DETAIL = details
            };


            return View(editModel);
        }
        public async Task<ActionResult> LoadPacketTeaPurchaseDetails(string unit, string docdt, string docno, int rowIndex = 0)
        {
            string formattedDate = docdt;

            if (!string.IsNullOrEmpty(docdt))
            {
                DateTime dt = Convert.ToDateTime(docdt);
                formattedDate = dt.ToString("yyyy-MM-dd");
            }
     
            // 1️⃣ Fetch PO details
            var detResponse = await Services.GetAsync<List<T_TEA_PURCHASE>>(
               $"/api/TeaPurchase/GetByDocNo?DOCNO={docno}&unit={unit}&docdt={formattedDate}"
            );

            var details = detResponse?.Data ?? new List<T_TEA_PURCHASE>();

            ViewBag.RowIndex = rowIndex;

            var model = new List<PT_ALL_LIST>
                {
                    new PT_ALL_LIST
                    {
                        T_TEA_DETAIL = details.Any()
                            ? details
                            : new List<T_TEA_PURCHASE> { new T_TEA_PURCHASE() }
                    }
                };

            return PartialView("_Product_Details", model);
        }


    }
}