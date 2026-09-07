using System;

namespace PacketTea.Models.DTO
{
    public class TRN_PAYMENT_ADVICE_DTO
    {
        public virtual string LOCA { get; set; }
        public virtual string UNIT { get; set; }
        public virtual string DOC_YEAR { get; set; }
        public virtual string PAYADV_NO { get; set; }
        public virtual DateTime? PAYADV_DT { get; set; }

        public virtual string ACODE { get; set; }
        public virtual string SUBCODE { get; set; }
        public virtual string ACCODE { get; set; }

        public virtual int SRLNO { get; set; }

        public virtual string BLNO { get; set; }
        public virtual DateTime? BLDT { get; set; }

        public virtual decimal? BILL_AMT { get; set; }
        public virtual decimal? DEDUCT_AMT { get; set; }
        public virtual decimal? TDS_AMT { get; set; }
        public virtual decimal? PAYABLE_AMT { get; set; }

        public virtual string REMARKS { get; set; }

        public virtual string I_DOCTYPE { get; set; }
        public virtual string I_DOCNO { get; set; }
        public virtual DateTime? I_DOCDT { get; set; }
        public virtual int? I_SRLNO { get; set; }

        public virtual string P_DOCTYPE { get; set; }
        public virtual string P_DOCNO { get; set; }
        public virtual DateTime? P_DOCDT { get; set; }
        public virtual int? P_SRLNO { get; set; }

        public virtual DateTime? DUEDT { get; set; }

        public virtual string OTHER_REF { get; set; }

        public virtual string USER_NAME { get; set; }
        public virtual DateTime? USER_ENTDT { get; set; }

        public virtual string DTAG { get; set; }

        public virtual string U_NAME_NEW { get; set; }
        public virtual DateTime? U_ENTDT_NEW { get; set; }

        public virtual string OS_USER { get; set; }
        public virtual string T_ID { get; set; }

        public virtual int ID { get; set; }
        public virtual string ACNAME { get; set; }
        public virtual string LOCKEDBYUSERID { get; set; }
        public virtual DateTime? LOCKEDUNTIL { get; set; }
    }
}
