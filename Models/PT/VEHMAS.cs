using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacketTea.Models.PT
{
    using System;

    namespace PacketTea.Models.PT
    {
        public class VEHMAS
        {
            public int id { get; set; }
            public string loca { get; set; }
            public string code { get; set; }
            public string name { get; set; }
            public int? capacity { get; set; }
            public string vtype { get; set; }

            public string useR_ORA { get; set; }
            public DateTime? entdt { get; set; }
            public string timE_ORA { get; set; }

            public string useR_NEW { get; set; }
            public DateTime? entdT_NEW { get; set; }
            public string timE_NEW { get; set; }

            public string dtag { get; set; }
            public string machinE_NO { get; set; }

            public string unit { get; set; }
            public string grouP_CODE { get; set; }
            public string mfG_NAME { get; set; }
            public string model { get; set; }

            public int? puR_YEAR { get; set; }
            public int? fleeT_PRIORITY { get; set; }

            public string active { get; set; }

            public DateTime? reG_DT { get; set; }
            public decimal? oP_KM { get; set; }

            public string fleeT_DESCN { get; set; }
            public string fleeT_DOC_PATH { get; set; }
            public string fleeT_SOP_PATH { get; set; }

            public string batterY_TYPE { get; set; }
            public string geaR_BOXNO { get; set; }
            public string reaR_AXLENO { get; set; }

            public int? nO_TYRES { get; set; }

            public decimal? fueL_CAP { get; set; }
            public decimal? enG_OIL_CAP { get; set; }

            public string curR_USED_FOR { get; set; }

            public decimal? capacitY_WT { get; set; }
            public decimal? capacitY_CASE { get; set; }

            public string bodY_TYPE { get; set; }
            public string curR_COND { get; set; }
            public string fueL_TYPE { get; set; }

            public decimal? pricE_CHASSIS { get; set; }
            public decimal? pricE_BODY { get; set; }
            public decimal? booK_VALE { get; set; }

            public string enG_NO { get; set; }
            public string chassiS_NO { get; set; }
            public string batterY_NO { get; set; }

            public string tyrE_TYPE { get; set; }
            public string rute { get; set; }

            public decimal? geaR_OIL_CAP { get; set; }
            public decimal? difF_OIL_CAP { get; set; }
            public decimal? colL_OIL_CAP { get; set; }

            public string tyre { get; set; }
            public string wheeL_RIMS { get; set; }

            public int? nO_WHEELS { get; set; }

            public string imagE_PATH { get; set; }
            public string owN_VEHICLE { get; set; }

            public decimal? avG_KM { get; set; }

            public string batterY_MAKE { get; set; }
            public DateTime? batterY_PURDT { get; set; }
            public DateTime? batterY_WARRENTY { get; set; }

            public string reF_CODE { get; set; }

            public decimal? lY_CL_KM { get; set; }
        }
    }
}
