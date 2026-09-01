using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PacketTea.Models.Report
{
    public class VOU_PRINT
    {
        public decimal SRLNO { get; set; }   
        public string UNIT { get; set; }   
        public string VOU_NO { get; set; }
        public string VOU_NO1 { get; set; }
        public DateTime? VOU_DT { get; set; }
        public string REC_TYPE { get; set; }
        public string DR_ACODE { get; set; }
        public string DR_SUBCODE { get; set; }
        public object REF_ORA { get; set; }
        public string REF_ORA1 { get; set; }
        public string NARRATION { get; set; }
        public Decimal AMOUNT { get; set; }
        public string CHQNO { get; set; }
        public DateTime? CHQDT { get; set; }
        public object BANK { get; set; }
        public string PADITO { get; set; }
        public string DRCR { get; set; }
        public string DR_REF { get; set; }
        public object ACNAME { get; set; }
        public string ACODE_ACNAME { get; set; }
        public string GNAME { get; set; }
        public object NAM { get; set; }
        public string VOUDESCN { get; set; }
        public int DRCR1 { get; set; }
        public string CON { get; set; }
        public string CR_ACODE { get; set; }
        public object CR_SUBCODE { get; set; }
        public string DRREFACNAME { get; set; }
        public string USER_ORA { get; set; }
        public object COST_CENTRE { get; set; }
        public object COSTNAME { get; set; }
        public object SHNAME { get; set; }
        public object MEMO_VOUNO { get; set; }
        public DateTime? MEMO_VOUDT { get; set; }
        public string NARR { get; set; }
        public string CHQNOTYPE { get; set; }
        public string BANKNAME { get; set; }
        public string ACNO { get; set; }
        public string RTGSCODE { get; set; }
        public string BRANCH { get; set; }
        public Decimal? QTY { get; set; }
        public Decimal? sum { get; set; }
        
    }





    //public class VOU_PRINT_V2
    //{
    //    public string PADITO { get; set; } = string.Empty;
    //    public decimal QTY { get; set; } = 0;
    //    public int DRCR_1 { get; set; } = 0;
    //    public string NARR { get; set; } = string.Empty;
    //    public string COST_CENTRE { get; set; } = string.Empty;
    //    public string REF_NAME { get; set; } = string.Empty;
    //    public DateTime? VOU_DT { get; set; } = DateTime.MinValue;
    //    public string RTGS_CODE { get; set; } = string.Empty;
    //    public string DR_REF { get; set; } = string.Empty;
    //    public string BANK_NAME { get; set; } = string.Empty;
    //    public string UNIT { get; set; } = string.Empty;
    //    public string GNAME { get; set; } = string.Empty;
    //    public string COST_NAME { get; set; } = string.Empty;
    //    public string REF_ORA { get; set; } = string.Empty;
    //    public string SHNAME { get; set; } = string.Empty;
    //    public string VOU_NO { get; set; } = string.Empty;
    //    public string NARRATION { get; set; } = string.Empty;
    //    public DateTime? MVOU_DT { get; set; } = DateTime.MinValue;
    //    public string DR_CR { get; set; } = string.Empty;
    //    public string CR_ACNAME { get; set; } = string.Empty;
    //    public string USER_ORA { get; set; } = string.Empty;
    //    public string VOUCHER_DESCN { get; set; } = string.Empty;
    //    public string BANK { get; set; } = string.Empty;
    //    public string MVOU_NO { get; set; } = string.Empty;
    //    public string AC_NO { get; set; } = string.Empty;
    //    public string SUBCODE { get; set; } = string.Empty;
    //    public decimal AMOUNT { get; set; } = 0;
    //    public string ACODE { get; set; } = string.Empty;
    //    public string CON { get; set; } = string.Empty;
    //    public string CHQNO_TYPE { get; set; } = string.Empty;
    //    public DateTime? CHQDT { get; set; } = DateTime.MinValue;
    //    public string REC_TYPE { get; set; } = string.Empty;
    //    public string DR_REF_ACNAME { get; set; } = string.Empty;
    //    public string BRANCH { get; set; } = string.Empty;
    //    public string CR_ACODE { get; set; } = string.Empty;
    //    public string CR_SUBCODE { get; set; } = string.Empty;
    //    public string ACNAME { get; set; } = string.Empty;
    //    public string CHQNO { get; set; } = string.Empty;
    //}


    public class VOU_PRINT_V2
    {
        //========================
        // Crystal Report Fields
        //========================

        public string VOU_NO { get; set; } = string.Empty;
        public string VOU_DT { get; set; } = string.Empty;
        public string VOUCHER_DESCN { get; set; } = string.Empty;

        public string DR_ACCOUNT { get; set; } = string.Empty;     // Crystal Only
        public string DR_ACHEAD { get; set; } = string.Empty;      // Crystal Only

        public decimal SRLNO { get; set; } = 0;                    // Crystal Only

        public string ACODE { get; set; } = string.Empty;
        public string DR_ACODE { get; set; } = string.Empty;
        public string SUBCODE { get; set; } = string.Empty;
        public string REF_ORA { get; set; } = string.Empty;
        public string ACNAME { get; set; } = string.Empty;
        public string REF_NAME { get; set; } = string.Empty;

        public string MAIN_HEAD { get; set; } = string.Empty;      // Crystal Only

        public string NARRATION { get; set; } = string.Empty;

        public string CHQNO { get; set; } = string.Empty;
        public DateTime? CHQDT { get; set; } = DateTime.MinValue;

        public string BANK { get; set; } = string.Empty;

        public decimal AMOUNT { get; set; } = 0;

        public string DR_CR { get; set; } = string.Empty;

        public string PAIDTO { get; set; } = string.Empty;         // Crystal Field

        public decimal TOTAMT { get; set; } = 0;                   // Crystal Only
        public string TOTWORD { get; set; } = string.Empty;        // Crystal Only

        public string GNAME { get; set; } = string.Empty;

        public string USER_ORA { get; set; } = string.Empty;

        public string COST_CENTRE { get; set; } = string.Empty;
        public string COST_NAME { get; set; } = string.Empty;

        public string BILLDET { get; set; } = string.Empty;        // Crystal Only

        public string MVOU_NO { get; set; } = string.Empty;
        public DateTime? MVOU_DT { get; set; } = DateTime.MinValue;

        public string BANK_NAME { get; set; } = string.Empty;
        public string AC_NO { get; set; } = string.Empty;
        public string RTGS_CODE { get; set; } = string.Empty;
        public string BRANCH { get; set; } = string.Empty;


        //========================
        // JSON Fields (Not in Crystal)
        //========================

        // Mismatch:
        // Crystal : PAIDTO
        // JSON    : PADITO
        // Keep both.

        public string PADITO { get; set; } = string.Empty;
        public string DRCR_2 { get; set; } = string.Empty;

        public decimal? QTY { get; set; } = 0;

        public int DRCR_1 { get; set; } = 0;

        public string NARR { get; set; } = string.Empty;

        public string LOCA { get; set; } = string.Empty;
        public string UNIT { get; set; } = string.Empty;

        public string SHNAME { get; set; } = string.Empty;

        public string CR_ACNAME { get; set; } = string.Empty;

        public string DR_REF { get; set; } = string.Empty;

        public string DR_REF_ACNAME { get; set; } = string.Empty;

        public string CON { get; set; } = string.Empty;

        public string CHQNO_TYPE { get; set; } = string.Empty;

        public string REC_TYPE { get; set; } = string.Empty;

        public string CR_ACODE { get; set; } = string.Empty;

        public string CR_SUBCODE { get; set; } = string.Empty;

        public string BILLDET1 { get; set; } = string.Empty;

        public string BILLDETTOT { get; set; } = string.Empty;

        public decimal ACT_DA_PAY { get; set; } = 0;

        public decimal OTHER_DED_AMT { get; set; } = 0;

        public decimal TOTAL_ALLOW_AMT { get; set; } = 0;

        public decimal EARNED_WAGES { get; set; } = 0;

        public string ACODE_ACNAME { get; set; } = string.Empty;

        public string NAM { get; set; } = string.Empty;
    }
    public class USERDATA
    {
        public string USER_ORA { get; set; } = string.Empty;
        public string USER_NAME { get; set; } = string.Empty;
    }
    }