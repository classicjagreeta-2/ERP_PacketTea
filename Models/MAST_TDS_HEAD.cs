using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PacketTea.Models
{
    public class MAST_TDS_HEAD
    {
        public virtual int ID { get; set; }
        public virtual string LOCA { get; set; }
        public virtual string CODE { get; set; }
        //public virtual string UNIT { get; set; }
        public virtual string DESCN { get; set; }
        public virtual decimal? TDS_PER { get; set; }
        public virtual decimal? ECESS_PER { get; set; }
        public virtual decimal? SURCH_PER { get; set; }
        public virtual string TDS_ACODE { get; set; }
        public virtual string ECS_ACODE { get; set; }
        public virtual string SUR_ACODE { get; set; }
        public virtual string USER_ORA { get; set; }
        public virtual DateTime? ENTDT { get; set; }
        public virtual string TIME_ORA { get; set; }
        public virtual string DTAG { get; set; }
        public virtual string USER_NEW { get; set; }
        public virtual DateTime? ENTDT_NEW { get; set; }
        public virtual string TIME_NEW { get; set; }
        public virtual string SHEC_ACODE { get; set; }
        public virtual string SUB_LEDGER { get; set; }
        public virtual string SEC_NO { get; set; }
        public virtual string PAN_4TH_CHAR { get; set; }
        public virtual decimal? PER_CONT_AMT { get; set; }
        public virtual decimal? PER_PA_AMT { get; set; }
        public virtual string TDS_TYPE { get; set; }
        public virtual decimal? HIGHER_RATE { get; set; }
        public virtual decimal? SPECIFIED_RATE { get; set; }
        //public virtual string USER_NAME_NEW { get; set; }
        //public virtual DateTime? USER_ENTDT_NEW { get; set; }
        //public virtual string USER_NAME { get; set; } = Config.UserInfo.UserName;
        //public virtual DateTime? USER_ENTDT { get; set; } = DateTime.Now;
        //public virtual string OS_USER { get; set; }
        //public virtual string TERMINAL_ID { get; set; }

    }
}