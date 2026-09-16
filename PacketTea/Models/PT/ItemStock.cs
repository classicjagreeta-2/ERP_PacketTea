using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacketTea.Models.Master
{
    public class ItemStock
    {
        public string loca { get; set; }
        public string unit { get; set; }
        public string itc { get; set; }
        public string mrp { get; set; }
        public string tm { get; set; }

        public decimal? opbal { get; set; }
        public decimal? purch { get; set; }
        public decimal? prod { get; set; }
        public decimal? mretu { get; set; }
        public decimal? chkin { get; set; }
        public decimal? sales { get; set; }
        public decimal? stqty { get; set; }
        public decimal? brkg { get; set; }
        public decimal? adj { get; set; }
        public decimal? clstock { get; set; }

        public string useR_ORA { get; set; }
        public DateTime? entdt { get; set; }
        public string timE_ORA { get; set; }

        public string useR_NEW { get; set; }
        public string timE_NEW { get; set; }
        public DateTime? entdT_NEW { get; set; }

        public string itM_SEL { get; set; }
        public decimal? slorderpnd { get; set; }

        public string tag { get; set; }
        public string machinE_NO { get; set; }

        public DateTime? mfgdt { get; set; }
        public string batchno { get; set; }

        public decimal? puR_RATE { get; set; }

        public string pcd { get; set; }

        public int id { get; set; }

        public decimal? grosS_WEIGHT { get; set; }
        public decimal? neT_WEIGHT { get; set; }
    }
}
