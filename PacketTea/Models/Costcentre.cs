using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PacketTea.Models.PacketTea
{
    public class Costcentre
    {
        public int id { get; set; }
        public string loca { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public string adds { get; set; }
        public string addss { get; set; }
        public string tag { get; set; }
        public string dtag { get; set; }
        public string useR_ORA { get; set; }
        public DateTime? entdt { get; set; }
        public string timE_ORA { get; set; }
        public string useR_NEW { get; set; }
        public object entdT_NEW { get; set; }
        public string timE_NEW { get; set; }
        public string machinE_NO { get; set; }
        public string ffcode { get; set; }
        public string grP1_CODE { get; set; }
        public string grP2_CODE { get; set; }
        public string reporT_ORDER { get; set; }
        public string useR_NAME { get; set; }
        public DateTime? useR_ENTDT { get; set; }
        public string useR_NAME_NEW { get; set; }
        public object useR_ENTDT_NEW { get; set; }
        public string oS_USER { get; set; }
        public string terminaL_ID { get; set; }

        public List<CostcentreUnit> Unitlist { get; set; }


        public class CostcentreUnit
        {
            public virtual string UNIT_CODE { get; set; }

            public virtual string DESC3 { get; set; }
        }
    }
}