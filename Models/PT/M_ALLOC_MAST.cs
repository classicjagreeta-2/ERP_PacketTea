using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PacketTea.Models.PT
{
    public class M_ALLOC_MAST
    {
        public virtual decimal ID { get; set; }

        public virtual string LOCA { get; set; }

        public virtual string GLOCA { get; set; }

        public virtual string UNIT { get; set; }

        public virtual string CODE { get; set; }

        public virtual string DESCN { get; set; }

        public virtual string STATE_CODE { get; set; }

        public virtual string ACTIVE_TAG { get; set; }

        public virtual string PUR_TYPE { get; set; }

        public virtual string COUNTRY_CD { get; set; }

        public virtual string U_NAME { get; set; }

        public virtual DateTime? U_ENTDT { get; set; }

        public virtual string U_NAMENEW { get; set; }

        public virtual DateTime? U_ENTDTNEW { get; set; }

        public virtual string O_USER { get; set; }

        public virtual string T_ID { get; set; }

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