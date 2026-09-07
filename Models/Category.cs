using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PacketTea.Models.PacketTea
{
    public class Category
    {
        public int id { get; set; }
        public string loca { get; set; }
        public string unit { get; set; }
        public string code { get; set; }
        public string descn { get; set; }
        public string subunit { get; set; }
        public string shT_DESCN { get; set; }
        public string useR_NAME { get; set; }
        public DateTime useR_ENTDT { get; set; }
        public string useR_NAME_NEW { get; set; }
        public object useR_ENTDT_NEW { get; set; }
        public string oS_USER { get; set; }
        public string terminaL_ID { get; set; }
        public object dtag { get; set; }
    }
}