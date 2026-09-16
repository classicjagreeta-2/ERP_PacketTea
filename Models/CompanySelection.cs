using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Web;

namespace PacketTea.Models
{
    public class CompanySelection
    {
        public class LocationSelection
        {
            [JsonProperty("loca")]
            public string Loca { get; set; }

            [JsonProperty("location")]
            public string Location { get; set; }

            [JsonProperty("database")]
            public string Database { get; set; }

            [JsonProperty("financialYear")]
            public string FinancialYear { get; set; }
            [JsonProperty("address")]
            public string Address { get; set; }
            [JsonProperty("DocYear")]
            public string DocYear { get; set; }
        }

        [JsonProperty("company")]
        public string Company { get; set; }
        [JsonProperty("isdefault")]
        public string IsDefault { get; set; }

        [JsonProperty("locationSelection")]
        public List<LocationSelection> locationSelection { get; set; }


    }
}