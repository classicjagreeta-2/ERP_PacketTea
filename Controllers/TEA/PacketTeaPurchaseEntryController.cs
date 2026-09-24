
using Finance.Models.PT;
using PacketTea.Models;
using PacketTea.Models.PT;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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
        public async Task<JsonResult> GetVendor( string q = "",   int limit = 25, string fieldValue = "",  string fieldText = "",  string value = "",  int p = 1)
        {
            if (string.IsNullOrEmpty(q))
            {
                q = value;
            }

            string acode = "72000200";

            var resp = await Services.FinanceGetAsync<PageModel<GetByPageSubCodeDDL>>( $"/api/BH21/GetByPageSubCodeDDLWithGST" + $"?Acode={acode}" +  $"&search={q}" +
                $"&page={p}" +
                $"&pageSize={limit}"
            );

            var data = resp?.Data?.value;

            var firstVendor = data?.results?.FirstOrDefault();

            if (firstVendor != null)
            {
                System.Diagnostics.Debug.WriteLine(
                    "========== VENDOR =========="
                );

                System.Diagnostics.Debug.WriteLine(
                    "ACODE    = " + firstVendor.acode
                );

                System.Diagnostics.Debug.WriteLine(
                    "ACNAME   = " + firstVendor.acname
                );

                System.Diagnostics.Debug.WriteLine(
                    "SUBCODE  = " + firstVendor.subcode
                );

                System.Diagnostics.Debug.WriteLine(
                    "GST      = " + firstVendor.gsT_NO
                );

                System.Diagnostics.Debug.WriteLine(
                    "STATE    = " + firstVendor.statE_CODE
                );

                System.Diagnostics.Debug.WriteLine(
                    "UNIT     = " + firstVendor.uniT_CODE
                );

                System.Diagnostics.Debug.WriteLine(
                    "============================"
                );
            }

            return Json(
                new
                {
                    data = data?.results ??
                           new List<GetByPageSubCodeDDL>(),

                    count = data?.rowCount ?? 0
                },
                JsonRequestBehavior.AllowGet
            );
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
        public async Task<JsonResult> GetSize(string q = "", int limit = 0, string fieldValue = "", string fieldText = "", string value = "", int p = 1)
        {
            if (string.IsNullOrEmpty(q))
            {
                q = value;
            }
            var resp = await Services.GetAsync<PageValue<M_CHESTSZ>>($"/api/TeaPurchase/GetSize?search={q}&page={p}&pageSize={limit}");
            var data = resp?.Data.results;

            return Json(new
            {
                data = data ?? new List<M_CHESTSZ>(),
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

        public async Task<JsonResult> GetUnit(string q = "", int limit = 10, string fieldValue = "", string fieldText = "", string value = "", int p = 1)
        {
            var resp = await Services.GetAsync<List<M_UNIT>>("/api/TeaPurchase/GetUnit");

            var all = resp?.Data ?? new List<M_UNIT>();

            // 🔍 Search filter
            if (!string.IsNullOrWhiteSpace(q))
            {
                all = all.Where(x =>
                    (x.CODE != null && x.CODE.Contains(q)) ||
                    (x.NAME != null && x.NAME.Contains(q)) ||
                    (x.GLOCA != null && x.GLOCA.Contains(q))
                ).ToList();
            }

            var count = all.Count;

            // 📄 Paging
            var data = all
                .Skip((p - 1) * limit)
                .Take(limit)
                .Select(x => new  // ✅ Make sure GLOCA is included
                {
                    CODE = x.CODE,
                    NAME = x.NAME,
                    GLOCA = x.GLOCA,
                    
                })
                .ToList();

            return Json(new
            {
                data = data,
                count = count
            }, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> GetDocType(string q = "", int limit = 10,  string fieldValue = "", string fieldText = "", string value = "", int p = 1, string unit = "")
        {
            if (string.IsNullOrEmpty(q))
            {
                q = value;
            }

            var resp = await Services.GetAsync<List<M_SALETYPE>>(
                "/api/TeaPurchase/GetType?"
            );

            var all = resp?.Data ?? new List<M_SALETYPE>();

            if (!string.IsNullOrEmpty(q))
            {
                q = q.ToLower();

                all = all.Where(x =>
                    (!string.IsNullOrEmpty(x.CODE) &&
                     x.CODE.ToLower().Contains(q))
                    ||
                    (!string.IsNullOrEmpty(x.DESCN) &&
                     x.DESCN.ToLower().Contains(q))
                    ||
                    (!string.IsNullOrEmpty(x.TYPE) &&
                     x.TYPE.ToLower().Contains(q))
                ).ToList();
            }

            var count = all.Count;

            var data = all
                .Skip((p - 1) * limit)
                .Take(limit)
                .Select(x => new
                {
                    CODE = x.CODE,
                    DESCN = x.DESCN,
                    TYPE = x.TYPE
                })
                .ToList();

            return Json(new
            {
                data = data,
                count = count
            }, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetLocation( string q = "", int limit = 0,   string fieldValue = "", string fieldText = "", string value = "",  int p = 1)
        {
            if (string.IsNullOrEmpty(q))
            {
                q = value;
            }

            var resp = await Services.GetAsync<PageValue<M_SALESCENTRE>>(
                $"/api/TeaPurchase/GetLocation?search={q}&page={p}&pageSize={limit}"
            );

            var data = resp?.Data.results;

            return Json(new
            {
                data = data ?? new List<M_SALESCENTRE>(),
                count = resp?.Data.rowCount ?? 0
            }, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetWarehouse(string q = "", int limit = 0, string fieldValue = "", string fieldText = "", string value = "", int p = 1)
        {
            if (string.IsNullOrEmpty(q))
            {
                q = value; 
            }

            var resp = await Services.GetAsync<PageValue<M_PMAST>>(
                $"/api/TeaPurchase/GetByWareHouse?search={q}&page={p}&pageSize={limit}"
            );

            var data = resp?.Data.results;

            return Json(new
            {
                data = data ?? new List<M_PMAST>(),
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
        public async Task<ActionResult> InsertOrUpdate(string unit = "", string docdt = "", string docno = "", string gloca = "", string doctype = "", string doctypeType = "")
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
                        UNIT = unit,
                        GLOCA = gloca,
                        PUR_TYPE = doctypeType
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


        [HttpPost]
        public async Task<ActionResult> SaveForm(PT_ALL_LIST model)
        {
            if (model == null || model.T_TEA_HEAD == null)
            {
                TempData["toastrError"] = "Invalid header data.";
                return RedirectToAction("Index");
            }

            if (model.T_TEA_DETAIL == null || !model.T_TEA_DETAIL.Any())
            {
                TempData["toastrError"] = "Please add at least one detail row.";
                return RedirectToAction("Index");
            }

            var head = model.T_TEA_HEAD;
            var details = model.T_TEA_DETAIL;

            var user = Utility.SessionHelper.GetUser();

            int slNo = 1;

            foreach (var detail in details)
            {
                //head.DOCNO = "000117";
                detail.GLOCA = head.GLOCA;
                detail.LOCA = user.Loca;
                detail.PUR_TYPE = head.PUR_TYPE;
                detail.GLOCA = head.GLOCA;
                detail.UNIT = head.UNIT;
              
                detail.DOCDT = head.DOCDT;
                detail.DOC_YEAR = head.DOC_YEAR;
                head.PACK_DATE = DateTime.Now;

                detail.PACK_DATE = head.PACK_DATE;
                detail.SALE_CENTRE = head.SALE_CENTRE;
                detail.BILLNO = head.BILLNO;
                detail.BILLDT = head.BILLDT;

                detail.EWAYBILLNO = head.EWAYBILLNO;
                detail.EWAYBILLDT = head.EWAYBILLDT;

                detail.PCODE = head.PCODE;
                detail.VENDORNAME = head.VENDORNAME;
                detail.TPT = head.TPT;
                detail.VEH_NO = head.VEH_NO;
                detail.PCODE_TYPE = detail.SZ_CODE;
                detail.GRADE_TYPE = head.GRADE_TYPE;

                detail.SEASON = head.SEASON;

                detail.CONS_NO = head.CONS_NO;
                detail.CONS_DT = head.CONS_DT;
                detail.PROMPT_DATE = head.PROMPT_DATE;

                detail.BROK_CODE = head.BROK_CODE;

                //detail.DO_NO = head.DO_NO;
                //detail.DO_DT = head.DO_DT;

                //detail.ALLOCATION = head.ALLOCATION;

                detail.REMARKS = head.REMARKS;

                detail.SALE_CENTRE = head.SALE_CENTRE;
                detail.SALE_TYPE = head.SALE_TYPE;

                detail.FLAVOUR = head.FLAVOUR;
                detail.ORGANIC_TEA_TYPE = head.ORGANIC_TEA_TYPE;

                // DETAIL
                detail.SL_NO = slNo.ToString();

                //detail.AMOUNT =
                //    (detail.QTY ?? 0) * (detail.RATE ?? 0);

                //detail.TOT_AMT = detail.AMOUNT;

                // AUDIT
                detail.U_NAME = user.getUserName;
                detail.O_USER = Environment.UserName;
                detail.T_ID = Environment.MachineName;

                detail.USER_NAME = user.getUserName;
                detail.USER_ENTDT = DateTime.Now;

                detail.U_ENTDT = DateTime.Now;

                detail.U_NAMENEW = user.getUserName;
                detail.U_ENTDTNEW = DateTime.Now;

                detail.O_USERNEW = Environment.UserName;
                detail.T_IDNEW = Environment.MachineName;

                //detail.USER_NAME_NEW = user.getUserName;
                //detail.USER_ENTDT_NEW = DateTime.Now;

                detail.OS_USER = Environment.UserName;
                detail.TERMINAL_ID = Environment.MachineName;

                detail.LOCKEDBYUSERID = user.getUserName;
                detail.LOCKEDUNTIL = DateTime.Now;

                slNo++;
            }

            //try
            //{
            // Send ONLY details because API expects:
            // IEnumerable<T_TEA_PURCHASE>
            //var json = JsonSerializer.Serialize(details);
            //var purchases = details;
            //            var response = await Services.PostAsync<List<T_TEA_PURCHASE>>(
            //    "/api/TeaPurchase/SaveOrUpdateAll",
            //    details
            //);

            //            if (response.IsSuccessStatusCode)
            //            {
            //                TempData["toastrSuccess"] =
            //                    $"{head.DOCNO} Packet Tea Purchase Entry Saved Successfully";
            //            }
            //            else
            //            {
            //                TempData["toastrError"] =
            //                    "Packet Tea Purchase Entry was not saved successfully.";
            //            }
            //        }
            //        catch (Exception ex)
            //        {
            //            TempData["toastrError"] =
            //                "Error while saving Packet Tea Purchase Entry: " + ex.Message;
            //        }

            //        return RedirectToAction("Index");
            try
            {
                var response = await Services.PostAsync<List<T_TEA_PURCHASE>>(
                    "/api/TeaPurchase/SaveOrUpdateAll",
                    details
                );

                // =========================
                // NEW ENTRY
                // =========================
                if (head.ID == 0)
                {
                    if (response != null && response.IsSuccessStatusCode)
                    {
                        TempData["toastrSuccess"] =
                            $"{head.DOCNO} Packet Tea Purchase Entry Saved Successfully";

                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ViewBag.toastrError =
                            response?.Message ??
                              $"{head.DOCNO} Packet Tea Purchase Entry Save Failed";

                        return View("InsertOrUpdate", model);
                    }
                }

                // =========================
                // UPDATE ENTRY
                // =========================
                else
                {
                    head.USER_ENTDT_NEW = DateTime.Now;
                    head.USER_NAME_NEW = user.getUserName;
                    if (response != null && response.IsSuccessStatusCode)
                    {
                        TempData["toastrSuccess"] =
                            $"{head.DOCNO} Packet Tea Purchase Entry Updated Successfully";

                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ViewBag.toastrError =
                            response?.Message ??
                              $"{head.DOCNO} Packet Tea Purchase Entry Update Failed";

                        return View("InsertOrUpdate", model);
                    }
                }
            }
            catch (Exception ex)
            {
                // =========================
                // NEW ERROR
                // =========================
                if (head.ID == 0)
                {
                    ViewBag.toastrError =
                        "Error while saving Packet Tea Purchase Entry: " + ex.Message;
                }

                // =========================
                // UPDATE ERROR
                // =========================
                else
                {
                    ViewBag.toastrError =
                        "Error while updating Packet Tea Purchase Entry: " + ex.Message;
                }

                return View("InsertOrUpdate", model);
            }
        }

    }
}