using PacketTea.Controllers;
using PacketTea.Utility;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.UI.WebControls;
using System.Xml.Linq;

namespace PacketTea.Models
{

    public static class Services
    {

        private static HttpClient _client { get; set; }
        private static string _userName = null;
        private static string _token = null;
        private static string _dbName = null;
        private static string _baseDomain = null;
        private static string _dbType = null;
        static Services()
        {
            _baseDomain = ConfigurationManager.AppSettings["SubDomain"];
            _dbType = ConfigurationManager.AppSettings["DatabaseType"];
            _client = new HttpClient();
            //{
            //    Timeout = TimeSpan.FromSeconds(30)
            //};
            _client.BaseAddress = new Uri(ConfigurationManager.AppSettings["BaseApiUrl"]);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public static async Task<ResponseApiModel<TData>> PostAsync<TData>(string url, object model, Type type = null)
        {
            ResponseApiModel<TData> tdata = new ResponseApiModel<TData>();
            JsonContent content = JsonContent.Create(model);
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("x-module", "PacketTea");
            _client.DefaultRequestHeaders.Add("x-docyear", Utility.SessionHelper.GetUser().DocYear);

            if (!string.IsNullOrEmpty(Utility.SessionHelper.GetUser().getToken))// Utility.SessionHelper.GetUser().getToken
            {
                _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + Utility.SessionHelper.GetUser().getToken);// Utility.SessionHelper.GetUser().getToken
            }
            if (!string.IsNullOrEmpty(Utility.SessionHelper.GetUser().getDbName))// Utility.SessionHelper.GetUser().getDbName
            {
                _client.DefaultRequestHeaders.Add("x-database", Utility.SessionHelper.GetUser().getDbName);// Utility.SessionHelper.GetUser().getDbName
            }
            if (!string.IsNullOrEmpty(_dbType))
            {
                _client.DefaultRequestHeaders.Add("x-dbtype", _dbType);
            }
            if (!String.IsNullOrEmpty(_baseDomain))
            {
                url = _baseDomain + url;
            }
            var rest = await _client.PostAsync(url, content);
            tdata = await ReadAsString<TData>(rest);
            return tdata;
        }

        public static async Task<ResponseApiModel<TData>> GetAsync<TData>(string url)
        {
            ResponseApiModel<TData> tdata = new ResponseApiModel<TData>();
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("x-module", "PacketTea");
            _client.DefaultRequestHeaders.Add("x-docyear", Utility.SessionHelper.GetUser().DocYear);
            //_client.DefaultRequestHeaders.Add("x-username", Utility.SessionHelper.GetUser().getUserName ?? "");// Utility.SessionHelper.GetUser().getUserName

            if (!string.IsNullOrEmpty(Utility.SessionHelper.GetUser().getToken))// Utility.SessionHelper.GetUser().getToken
            {
                _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + Utility.SessionHelper.GetUser().getToken);//  Utility.SessionHelper.GetUser().getToken
            }
            if (!string.IsNullOrEmpty(Utility.SessionHelper.GetUser().getDbName))// Utility.SessionHelper.GetUser().getDbName
            {
                _client.DefaultRequestHeaders.Add("x-database", Utility.SessionHelper.GetUser().getDbName);// Utility.SessionHelper.GetUser().getDbName
            }
            if (!string.IsNullOrEmpty(_dbType))
            {
                _client.DefaultRequestHeaders.Add("x-dbtype", _dbType);
            }
            if (!String.IsNullOrEmpty(_baseDomain))
            {
                url = _baseDomain + url;
            }
            var rest = await _client.GetAsync(url);
            var response = await rest.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"Status: {(int)rest.StatusCode} - {rest.StatusCode}");
            System.Diagnostics.Debug.WriteLine(response);
            // Access token expired
            if (rest.StatusCode == HttpStatusCode.Unauthorized)
            {
                bool refresh = await RefreshTokenAsync();
                if (refresh)
                {
                    // Retry with new token
                    rest = await _client.GetAsync(url);
                }
            }
            tdata = await ReadAsString<TData>(rest);
            int count = 0;
            if (tdata.Data is System.Collections.ICollection collection)
            {
                count = collection.Count;
            }
            return tdata;
        }

        private static async Task<bool> RefreshTokenAsync()
        {
            try
            {
                var request = new
                {
                    accessToken = SessionHelper.GetUser().getToken,
                    refreshToken = ""
                };
                var Return = await Services.PostAsync<UserModels>("/Users/Refresh", request);
                if (Return != null)
                {
                    if (Return.Data != null)
                    {
                        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Return.Data.accessToken);
                        Utility.SessionHelper.GetUser().getToken = Return.Data.accessToken;
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        private static async Task<ResponseApiModel<TData>> ReadAsString<TData>(HttpResponseMessage rest)
        {
            Logger logger = LogManager.GetCurrentClassLogger();
            ResponseApiModel<TData> tdata = new ResponseApiModel<TData>();
            string responseBody = await rest.Content.ReadAsStringAsync();
            try
            {
                if (rest.IsSuccessStatusCode)
                {
                    //logger.Warn("rest LogError: - " + responseBody + ". Data : " + rest);
                    tdata.Data = JsonConvert.DeserializeObject<TData>(responseBody);
                    tdata.StatusCode = rest.StatusCode.ToString();
                    tdata.IsSuccessStatusCode = rest.IsSuccessStatusCode;
                    //if (rest.StatusCode == HttpStatusCode.Unauthorized)
                    //{
                    //    tdata.StatusCode = "401";
                    //    tdata.Message = "Unauthorized";
                    //    return tdata;
                    //}
                }
                else
                {
                    logger.Error("rest LogError: - " + responseBody + ". Data : " + rest);
                    var res = JsonConvert.DeserializeObject<dynamic>(responseBody);
                    tdata.IsSuccessStatusCode = rest.IsSuccessStatusCode;
                    if (res != null)
                    {
                        string message = res.message;
                        if (string.IsNullOrEmpty(message))
                        {
                            // ASP.NET Core's [ApiController] automatic model-validation
                            // failures come back as a ValidationProblemDetails body
                            // ({"title": "...", "errors": {"Field": ["msg", ...]}}),
                            // which has no "message" property -- surface that instead
                            // of silently falling back to a generic "Save failed.".
                            string title = res.title;
                            var errorLines = new List<string>();
                            if (res.errors != null)
                            {
                                foreach (var prop in (JObject)res.errors)
                                {
                                    foreach (var msg in prop.Value)
                                        errorLines.Add(msg.ToString());
                                }
                            }
                            message = errorLines.Count > 0
                                ? string.Join(" ", errorLines)
                                : title;
                        }
                        tdata.Message = message;
                    }
                    tdata.StatusCode = rest.StatusCode.ToString();
                }
            }
            catch (Exception ex)
            {
                // The API can answer with a plain-text/HTML body (e.g. a routing
                // error) instead of JSON, even on a 2xx. Newtonsoft throws in that
                // case -- degrade to a failed response instead of a 500/YSOD so the
                // caller's existing "!IsSuccessStatusCode" handling can show it.
                logger.Error("rest exception LogError: - " + ex.Message + ". Data : " + rest + ". Body : " + responseBody);
                tdata.IsSuccessStatusCode = false;
                tdata.StatusCode = rest.StatusCode.ToString();
                tdata.Message = string.IsNullOrWhiteSpace(responseBody) ? ex.Message : responseBody;
            }

            return tdata;
        }
        public static async Task<ResponseApiModel<TData>> PostDBAsync<TData>(string url, object model, string dbname, Type type = null)
        {
            ResponseApiModel<TData> tdata = new ResponseApiModel<TData>();
            JsonContent content = JsonContent.Create(model);
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("x-module", "PacketTea");
            _client.DefaultRequestHeaders.Add("x-docyear", Utility.SessionHelper.GetUser().DocYear);
            if (!string.IsNullOrEmpty(Utility.SessionHelper.GetUser().getToken))
            {
                _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + Utility.SessionHelper.GetUser().getToken);
            }

            _client.DefaultRequestHeaders.Add("x-database", dbname);

            if (!string.IsNullOrEmpty(_dbType))
            {
                _client.DefaultRequestHeaders.Add("x-dbtype", _dbType);
            }
            if (!String.IsNullOrEmpty(_baseDomain))
            {
                url = _baseDomain + url;
            }
            var rest = await _client.PostAsync(url, content);
            tdata = await ReadAsString<TData>(rest);
            return tdata;
        }
        public static async Task<ResponseApiModel<TData>> BoughtleafGetAsync<TData>(string url)
        {
            ResponseApiModel<TData> tdata = new ResponseApiModel<TData>();

            _client.DefaultRequestHeaders.Clear();

            _client.DefaultRequestHeaders.Add("x-module", "Boughtleaf");

            _client.DefaultRequestHeaders.Add(
                "x-docyear",
                Utility.SessionHelper.GetUser().DocYear
            );

            if (!string.IsNullOrEmpty(Utility.SessionHelper.GetUser().getToken))
            {
                _client.DefaultRequestHeaders.Add(
                    "Authorization",
                    "Bearer " + Utility.SessionHelper.GetUser().getToken
                );
            }

            if (!string.IsNullOrEmpty(Utility.SessionHelper.GetUser().getDbName))
            {
                var factoryedb = Utility.SessionHelper.GetUser().Factorydb;
                _client.DefaultRequestHeaders.Add(
                    "x-database", factoryedb);
            }

            if (!string.IsNullOrEmpty(_dbType))
            {
                _client.DefaultRequestHeaders.Add("x-dbtype", _dbType);
            }
            if (!String.IsNullOrEmpty(_baseDomain))
            {
                url = _baseDomain + url;
            }

            var rest = await _client.GetAsync(url);

            tdata = await ReadAsString<TData>(rest);

            return tdata;
        }


    }

    public class UnauthorizedModel
    {
        public string message { get; set; }
    }
}