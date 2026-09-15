using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Finance.Models.PT
{
    public class M_PMAST
    {
        // ==================== NOT NULL ====================

        public virtual string LOCA { get; set; }
        public virtual string CODE { get; set; }
        public virtual string TYPE { get; set; }
        public virtual string U_NAME { get; set; }
        public virtual string O_USER { get; set; }
        public virtual string T_ID { get; set; }


        // ==================== NULLABLE ====================

        public virtual decimal? ID { get; set; }
        public virtual string GLOCA { get; set; }
        public virtual string UNIT { get; set; }
        public virtual string NAME { get; set; }
        public virtual string DESTI { get; set; }
        public virtual string ADD1 { get; set; }
        public virtual string ADD2 { get; set; }
        public virtual string ADD3 { get; set; }
        public virtual string CITY { get; set; }
        public virtual string CONTACT_NO { get; set; }
        public virtual string E_MAIL_ID { get; set; }
        public virtual string TIN_ID { get; set; }
        public virtual string CST_NO { get; set; }
        public virtual string DC { get; set; }

        public virtual int? CREDIT_D { get; set; }
        public virtual decimal? CREDIT_L { get; set; }

        public virtual string G_CODE { get; set; }
        public virtual string PIN { get; set; }
        public virtual string TMC_NO { get; set; }
        public virtual string PAN_NO { get; set; }
        public virtual string STATE { get; set; }
        public virtual string COUNTRY { get; set; }
        public virtual string ACD { get; set; }
        public virtual string PAYTERMS { get; set; }
        public virtual string MG_CODE { get; set; }
        public virtual string RCODE { get; set; }
        public virtual string ACODE { get; set; }
        public virtual string SUBCODE { get; set; }

        public virtual DateTime? INACTIVE_DATE { get; set; }

        public virtual string GST_NO { get; set; }
        public virtual DateTime? GST_REGDT { get; set; }
        public virtual DateTime? EDT_UNREGD { get; set; }
        public virtual DateTime? EDT_COMPOSITE { get; set; }
        public virtual DateTime? EDT_REGD { get; set; }

        public virtual string ADHAAR_NO { get; set; }
        public virtual string STATE_CODE { get; set; }
        public virtual string CNAME { get; set; }
        public virtual string GPCD { get; set; }
        public virtual string SHT_NAME { get; set; }
        public virtual string COUNTRY_CD { get; set; }
        public virtual string CURRENCY_CD { get; set; }
        public virtual string CATE_CODE { get; set; }
        public virtual string ECGC_REFNO { get; set; }
        public virtual DateTime? ECGC_REFDT { get; set; }
        public virtual DateTime? ECGC_VALID_DT { get; set; }
        public virtual DateTime? VALID_FROM { get; set; }
        public virtual DateTime? VALID_TO { get; set; }

        public virtual decimal? FOB_COST { get; set; }
        public virtual decimal? COMMI_PER { get; set; }

        public virtual string BANK_NAME { get; set; }
        public virtual string BANK_ADD { get; set; }
        public virtual string BANK_ACNO { get; set; }
        public virtual string IFC_SWIFT_CODE { get; set; }
        public virtual string RATE_TYPE { get; set; }
        public virtual string BUYER_REGNO { get; set; }
        public virtual string CONT_NAME1 { get; set; }
        public virtual string CONT_NAME2 { get; set; }
        public virtual string CONT_PH2 { get; set; }
        public virtual string EMAIL_ID2 { get; set; }
        public virtual string SAM_APPROVED_TYPE { get; set; }
        public virtual string BUILDING_NAME { get; set; }
        public virtual string BUILDING_NO { get; set; }
        public virtual string FLAT_NO { get; set; }
        public virtual string DISTRICT { get; set; }
        public virtual string MOBILE_NO { get; set; }
        public virtual string TCS_TAG { get; set; }

        public virtual decimal? DISTANCE { get; set; }
        public virtual decimal? ECGC_LIMIT { get; set; }

        public virtual string TDS_TAG { get; set; }
        public virtual string SPECIFIED_PERSON { get; set; }

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