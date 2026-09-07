using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Finance.Models.Finance
{
    public class voucharTypeMaster
    {
        public int id { get; set; }
        public string loca { get; set; }
        public string desC_ORA { get; set; }
        public string type { get; set; }
        public int lasT_NO { get; set; }
        public object lasT_DT { get; set; }
        public int lasT_AMT { get; set; }
        public int drcr { get; set; }
        public string con { get; set; }
        public string dr { get; set; }
        public string useR_ORA { get; set; }
        public object entdt { get; set; }
        public string timE_ORA { get; set; }
        public string useR_NEW { get; set; }
        public object entdT_NEW { get; set; }
        public string timE_NEW { get; set; }
        public string dtag { get; set; }
        public string machinE_NO { get; set; }
        public string conS_LEDG { get; set; }
        public string ftext { get; set; }
        public string vnuM_TYPE { get; set; }
        public string posT_DATED { get; set; }
        public DateTime? froM_DATE { get; set; }
        public DateTime? tO_DATE { get; set; }
        public string provisional { get; set; }
        public object useR_NAME { get; set; }
        public object useR_ENTDT { get; set; }
        public object useR_NAME_NEW { get; set; }
        public object useR_ENTDT_NEW { get; set; }
        public object oS_USER { get; set; }
        public object terminaL_ID { get; set; }
    }
}