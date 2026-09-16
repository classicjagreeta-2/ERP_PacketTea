using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PacketTea.Models.PT
{
    public class M_SALESCENTRE
    {


        public virtual decimal? ID { get; set; }

        public virtual string LOCA { get; set; }

        public virtual string GLOCA { get; set; }

        public virtual string UNIT { get; set; }

        public virtual string CODE { get; set; }

        public virtual string NAME { get; set; }

        public virtual string ADD1 { get; set; }

        public virtual string ADD2 { get; set; }

        public virtual string ADD3 { get; set; }

        public virtual string CITY { get; set; }

        public virtual string PIN { get; set; }

        public virtual string CONTACT_NO { get; set; }

        public virtual string E_MAIL_ID { get; set; }

        public virtual string PAN_NO { get; set; }

        public virtual string GST_NO { get; set; }

        public virtual DateTime? GST_REGDT { get; set; }

        public virtual string STATE_CODE { get; set; }

        public virtual string REF_CODE { get; set; }

        public virtual string ACODE { get; set; }

        public virtual string PH_NO { get; set; }

        public virtual string FSSAI_NO { get; set; }

        public virtual DateTime? FSSAI_DT { get; set; }

        public virtual string CIN_NO { get; set; }

        public virtual string GST_REF_CODE { get; set; }

        public virtual string DESTI_CODE { get; set; }

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