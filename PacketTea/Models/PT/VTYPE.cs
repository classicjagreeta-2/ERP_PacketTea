using System;

namespace PacketTea.Models.PT
{
    public class VTYPE
    {
        public string loca { get; set; }
        public string code { get; set; }
        public string desC_ORA { get; set; }

        public int? capacitY_M { get; set; }
        public int? capacitY_Q { get; set; }

        public string useR_ORA { get; set; }
        public string timE_ORA { get; set; }

        public DateTime? entdt { get; set; }

        public string dtag { get; set; }
        public string useR_NEW { get; set; }
        public string timE_NEW { get; set; }

        public DateTime? entdT_NEW { get; set; }

        public object unit { get; set; }
        public object grouP_CATEGORY { get; set; }
        public object grouP_DESCN { get; set; }
        public object miN_AMT { get; set; }
        public object abatment { get; set; }
        public object sT_RATE { get; set; }
        public object volumE_CFT { get; set; }

        public int? id { get; set; }
    }
}