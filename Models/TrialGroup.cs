using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Finance.Models.Finance
{
    public class TrialGroup
    {
        public int id { get; set; }
        public string loca { get; set; }
        public string unit { get; set; }
        public string code { get; set; }
        public string descn { get; set; }
        public string useR_NAME { get; set; }
        public DateTime? useR_ENTDT { get; set; }
        public string dtag { get; set; }
        public string u_NAME_NEW { get; set; }
        public object u_ENTDT_NEW { get; set; }
        public string oS_USER { get; set; }
        public string t_ID { get; set; }
        public string useR_NAME_NEW { get; set; }
        public object useR_ENTDT_NEW { get; set; }
        public string terminaL_ID { get; set; }
    }
}