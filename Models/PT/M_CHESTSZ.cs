using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PacketTea.Models.PT
{
    public class M_CHESTSZ
    {
        public virtual string LOCA { get; set; }
        public virtual string GLOCA { get; set; }
        public virtual string UNIT { get; set; }
        public virtual string SZ_CODE { get; set; }
        public virtual string SZ_DESC { get; set; }
        public virtual string PKG_BAG_TYPE { get; set; }
        public virtual string PKG_TAG { get; set; }
        public virtual decimal? TARE_WT { get; set; }
        public virtual decimal? VARIANCE_QTY { get; set; }
        public virtual string APPROVED { get; set; }
        public virtual string MARK { get; set; }
        public virtual string SHORT_DESC { get; set; }
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
        public virtual int ID { get; set; }
        public virtual string LOCKEDBYUSERID { get; set; }
        public virtual DateTime? LOCKEDUNTIL { get; set; }

    }
}