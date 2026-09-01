using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Inventory.Controllers.Test
{
    public class DefaultController : Controller
    {

        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<JsonResult> GetPage(string q = "", int limit = 0, string fieldValue = "", string fieldText = "", string value = "", int p = 0)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, $"http://103.234.116.64:90/INV_Api/api/Bl_T_Gleaf/GetByPage?page={p}&pageSize={limit}");
            request.Headers.Add("accept", "*/*");
            request.Headers.Add("x-database", "FACT_ARYA2026");
            request.Headers.Add("x-module", "BOUGHTLEAF");
            request.Headers.Add("x-dbtype", "ORACLE");
            request.Headers.Add("UserName", "CORETEAM");
            request.Headers.Add("Password", "CORE55%%");
            request.Headers.Add("x-docyear", "2026");
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var dta = (await response.Content.ReadAsStringAsync());
            string json = dta;
            RootObject data = JsonConvert.DeserializeObject<RootObject>(json);
            return Json(new
            {
                data = data.Value.Results,
                totalCount = data.Value.TotalCount,
                currentPage = data.Value.CurrentPage,
                pageSize = data.Value.PageSize,
                count = data.Value.RowCount
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetOrders()
        {

            List<OrderModel> orders = new List<OrderModel>();

            for (int i = 1; i <= 10000; i++)
            {
                orders.Add(new OrderModel
                {
                    OrderID = i,
                    Customer = "Customer " + i,
                    Amount = 100 + i,
                    Date = DateTime.Today.AddDays(-i)
                });
            }

            return Json(orders, JsonRequestBehavior.AllowGet);

        }

    }

    public class RootObject
    {
        [JsonPropertyName("value")]
        public PagedValue Value { get; set; }
    }

    public class PagedValue
    {
        [JsonPropertyName("results")]
        public List<InputPickerRow> Results { get; set; }

        [JsonPropertyName("totalCount")]
        public int TotalCount { get; set; }

        [JsonPropertyName("currentPage")]
        public int CurrentPage { get; set; }

        [JsonPropertyName("pageCount")]
        public int PageCount { get; set; }

        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; }

        [JsonPropertyName("rowCount")]
        public int RowCount { get; set; }

        [JsonPropertyName("linkTemplate")]
        public string LinkTemplate { get; set; }

        [JsonPropertyName("firstRowOnPage")]
        public int FirstRowOnPage { get; set; }

        [JsonPropertyName("lastRowOnPage")]
        public int LastRowOnPage { get; set; }
    }

    public class InputPickerRow
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("unit")]
        public string Unit { get; set; }

        [JsonPropertyName("slno")]
        public string Slno { get; set; }

        [JsonPropertyName("datE_ORA")]
        public DateTime DateOra { get; set; }

        [JsonPropertyName("atime")]
        public string ATime { get; set; }

        [JsonPropertyName("tno")]
        public string Tno { get; set; }

        [JsonPropertyName("pcd")]
        public string Pcd { get; set; }

        [JsonPropertyName("party_Name")]
        public string PartyName { get; set; }

        [JsonPropertyName("perc")]
        public string Perc { get; set; }

        [JsonPropertyName("reason")]
        public string Reason { get; set; }

        [JsonPropertyName("doC_YEAR")]
        public string DocYear { get; set; }

        [JsonPropertyName("bilL_NO")]
        public string BillNo { get; set; }
    }

    public class OrderModel
    {
        public int OrderID { get; set; }

        public string Customer { get; set; }

        public decimal Amount { get; set; }

        public DateTime Date { get; set; }
    }
}