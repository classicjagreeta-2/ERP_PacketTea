using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



    namespace PacketTea.Models.PT
    {
        public class M_Transp
        {
            public string loca { get; set; }
            public string gloca { get; set; }
            public string unit { get; set; }

            public string code { get; set; }
            public string name { get; set; }

            public string adD1 { get; set; }
            public string adD2 { get; set; }
            public string adD3 { get; set; }

            public string city { get; set; }
            public string pin { get; set; }

            public string contacT_NO { get; set; }
            public string e_MAIL_ID { get; set; }

            public string tiN_ID { get; set; }
            public string csT_NO { get; set; }
            public string tmC_NO { get; set; }
            public string paN_NO { get; set; }
            public string gsT_NO { get; set; }

            public string shT_NAME { get; set; }

            public string acode { get; set; }
            public string subcode { get; set; }

            public string statE_CODE { get; set; }

            public DateTime? gsT_REGDT { get; set; }
            public DateTime? edT_UNREGD { get; set; }
            public DateTime? edT_COMPOSITE { get; set; }
            public DateTime? edT_REGD { get; set; }

            public string adhaaR_NO { get; set; }

            public string tcS_TAG { get; set; }
            public string tdS_TAG { get; set; }
            public string specifieD_PERSON { get; set; }

            public string u_NAME { get; set; }
            public DateTime? u_ENTDT { get; set; }

            public string u_NAMENEW { get; set; }
            public DateTime? u_ENTDTNEW { get; set; }

            public string o_USER { get; set; }
            public string t_ID { get; set; }

            public string dtag { get; set; }

            public string o_USERNEW { get; set; }
            public string t_IDNEW { get; set; }

            public string useR_NAME { get; set; }
            public DateTime? useR_ENTDT { get; set; }

            public string useR_NAME_NEW { get; set; }
            public DateTime? useR_ENTDT_NEW { get; set; }

            public string oS_USER { get; set; }
            public string terminaL_ID { get; set; }

            public int? id { get; set; }

            public string lockedbyuserid { get; set; }
            public DateTime? lockeduntil { get; set; }
        }
    }

