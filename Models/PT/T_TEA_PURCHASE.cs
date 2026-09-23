using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PacketTea.Models.PT
{
    public class T_TEA_PURCHASE
    {
        // ==================== NOT NULL ====================

        public virtual string LOCA { get; set; }
        public virtual string GLOCA { get; set; }
        public virtual string UNIT { get; set; }
        public virtual string DOCNO { get; set; }
        public virtual DateTime? DOCDT { get; set; }
        public virtual string SL_NO { get; set; }
        public virtual string GRADE { get; set; }
        public virtual string INVNO { get; set; }
        public virtual string MARK { get; set; }
        public virtual DateTime? PACK_DATE { get; set; }
        public virtual string PUR_TYPE { get; set; }
        public virtual string PCODE_TYPE { get; set; }
        public virtual string U_NAME { get; set; }
        public virtual string O_USER { get; set; }
        public virtual string T_ID { get; set; }


        // ==================== NULLABLE ====================

        public virtual int? ID { get; set; } = 0;

        public virtual string BILLNO { get; set; }
        public virtual DateTime? BILLDT { get; set; }
        public virtual string EWAYBILLNO { get; set; }
        public virtual DateTime? EWAYBILLDT { get; set; }

        public virtual string PCODE { get; set; }
        public virtual string TPT { get; set; }
        public virtual string VEH_NO { get; set; }

        public virtual string GRADE_TYPE { get; set; }
        public virtual string CAT { get; set; }
        public virtual string SUBCAT { get; set; }

        public virtual decimal? BAG_CHEST { get; set; }
        public virtual decimal? QTY { get; set; }
        public virtual decimal? RATE { get; set; }
        public virtual decimal? AMOUNT { get; set; }
        public virtual decimal? DISC_PER { get; set; }
        public virtual decimal? DISCOUNT { get; set; }

        public virtual decimal? CGST_RT { get; set; }
        public virtual decimal? CGST_AMT { get; set; }
        public virtual decimal? SGST_RT { get; set; }
        public virtual decimal? SGST_AMT { get; set; }
        public virtual decimal? IGST_RT { get; set; }
        public virtual decimal? IGST_AMT { get; set; }
        public virtual decimal? TCS_RATE { get; set; }
        public virtual decimal? TCS_AMT { get; set; }
        public virtual decimal? TOT_AMT { get; set; }

        public virtual decimal? TARE { get; set; }
        public virtual decimal? GROSS { get; set; }

        public virtual decimal? CHESTSLF { get; set; }
        public virtual decimal? CHESTSLT { get; set; }

        public virtual string SZ_CODE { get; set; }
        public virtual string SEASON { get; set; }

        public virtual DateTime? MFG_FROM { get; set; }
        public virtual DateTime? MFG_TO { get; set; }

        public virtual string CONS_NO { get; set; }
        public virtual DateTime? CONS_DT { get; set; }
        public virtual DateTime? PROMPT_DATE { get; set; }

        public virtual string BROK_CODE { get; set; }
        public virtual string WAREHOUSE { get; set; }
        public virtual string DO_NO { get; set; }
        public virtual DateTime? DO_DT { get; set; }
        public virtual string ALLOCATION { get; set; }

        public virtual decimal? MISC_CHGS { get; set; }
        public virtual decimal? ROUNDOFF { get; set; }
        public virtual string REMARKS { get; set; }

        public virtual string SALE_CENTRE { get; set; }
        public virtual string SALE_TYPE { get; set; }
        public virtual decimal? TOT_GSTAMT_M { get; set; }

        public virtual string SALE_NO { get; set; }
        public virtual decimal? BROKERAGE { get; set; }
        public virtual DateTime? SALE_DATE { get; set; }

        public virtual string TDS_CODE { get; set; }
        public virtual decimal? TDS_AMT { get; set; }
        public virtual decimal? TDS_PER { get; set; }
        public virtual decimal? TDS_DETD { get; set; }

        public virtual string FLAVOUR { get; set; }
        public virtual string ORGANIC_TEA_TYPE { get; set; }
        public virtual string DOC_YEAR { get; set; }

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

        public virtual string CONCT_DOCNO { get; set; }
        public virtual string CONCT_SL_NO { get; set; }
        public virtual DateTime? CONCT_DOCDT { get; set; }
        public virtual string BROKERNAME { get; set; }
        public virtual string TRANSPORTERNAME { get; set; }
        public virtual string VENDORNAME { get; set; }
        public virtual string LOCATIONDESC { get; set; }
        public virtual string MARKDESC { get; set; }
        public virtual string SIZEDESC { get; set; }
        public virtual string CATDESC { get; set; }
        public virtual string WAREHOUSEDESC { get; set; }

        public virtual string GRADEDESC { get; set; }
        

    }
}