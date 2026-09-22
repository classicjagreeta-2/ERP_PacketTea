using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PacketTea.Models.PT
{
    public class M_UNIT
    {
        public virtual string LOCA { get; set; }
        public virtual string CODE { get; set; }
        public virtual string U_NAME { get; set; }
        public virtual string O_USER { get; set; }
        public virtual string T_ID { get; set; }

        // NULLABLE
        public virtual decimal? ID { get; set; }

        public virtual string GLOCA { get; set; }
        public virtual string UNIT { get; set; }
        public virtual string SCODE { get; set; }
        public virtual string DIRE { get; set; }
        public virtual string NAME { get; set; }
        public virtual string CONAME { get; set; }
        public virtual string ADD1 { get; set; }
        public virtual string ADD2 { get; set; }

        public virtual string CSTNO { get; set; }
        public virtual DateTime? CSTDT { get; set; }

        public virtual string LSTNO { get; set; }
        public virtual DateTime? LSTDT { get; set; }

        public virtual string ECCNO { get; set; }
        public virtual string RANGE { get; set; }
        public virtual string DIV { get; set; }
        public virtual string CEREGDNO { get; set; }
        public virtual string PLANO { get; set; }

        public virtual string UNSRTEMPT { get; set; }
        public virtual string STAXCD { get; set; }
        public virtual string SIMB { get; set; }
        public virtual string MFG_TAG { get; set; }

        public virtual string TIN_NO { get; set; }

        public virtual string EMAILID_HO { get; set; }
        public virtual string EMAILID { get; set; }

        public virtual string GST_NO { get; set; }

        public virtual DateTime? U_ENTDT { get; set; }
        public virtual string U_NAMENEW { get; set; }
        public virtual DateTime? U_ENTDTNEW { get; set; }

        public virtual string DTAG { get; set; }
        public virtual string O_USERNEW { get; set; }
        public virtual string T_IDNEW { get; set; }

        public virtual string USER_NAME { get; set; }
        public virtual DateTime? USER_ENTDT { get; set; }

        public virtual string USER_NAME_NEW { get; set; }
        public virtual DateTime? USER_ENTDT_NEW { get; set; }

        public virtual string OS_USER { get; set; }
        public virtual string TERMINAL_ID { get; set; }

        public virtual string LOCKEDBYUSERID { get; set; }
        public virtual DateTime? LOCKEDUNTIL { get; set; }

    }
}