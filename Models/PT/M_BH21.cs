using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PacketTea.Models.PT
{
    

    public class M_BH21
    {
        public virtual string LOCA { get; set; }
        public virtual string UNIT { get; set; }
        public virtual decimal? ID_ENT { get; set; }
        public virtual string RT { get; set; }
        public virtual string LAB1 { get; set; }
        public virtual string LAB2 { get; set; }
        public virtual string LAB3 { get; set; }
        public virtual string LAB4 { get; set; }
        public virtual string LAB5 { get; set; }
        public virtual string CCAC { get; set; }
        public virtual string ACODE { get; set; }
        public virtual string SUBCODE { get; set; }
        public virtual decimal? LYCLBAL { get; set; }
        public virtual decimal? MTHOPBAL1 { get; set; }
        public virtual decimal? MTHOPBAL { get; set; }
        public virtual decimal? OPNQTY { get; set; }
        public virtual decimal? MTHCLBAL { get; set; }
        public virtual decimal? DEB { get; set; }
        public virtual decimal? CRED { get; set; }
        public virtual decimal? QTYDEB { get; set; }
        public virtual decimal? QTYCRED { get; set; }
        public virtual string ACNAME { get; set; }
        public virtual decimal? INTRT { get; set; }
        public virtual string INTDRCR { get; set; }
        public virtual decimal? TDSRATE { get; set; }
        public virtual string REF_ORA { get; set; }
        public virtual string LOGIC_ORA { get; set; }
        public virtual string LOGIC1 { get; set; }
        public virtual string RFG { get; set; }
        public virtual decimal? INTOP { get; set; }
        public virtual DateTime? ENTDT { get; set; }
        public virtual string USER_ORA { get; set; }
        public virtual string TIME_ORA { get; set; }
        public virtual string DUMMY { get; set; }
        public virtual DateTime? NONOP { get; set; }
        public virtual string DEPCODE { get; set; }
        public virtual string PRODCODE { get; set; }
        public virtual string ROSSCODE { get; set; }
        public virtual string UNIDAGCODE { get; set; }
        public virtual string CODE104 { get; set; }
        public virtual string MMRCODE { get; set; }
        public virtual string ROSSACCD { get; set; }
        
        public virtual string USER_NEW { get; set; }
        public virtual DateTime? ENTDT_NEW { get; set; }
        public virtual string TIME_NEW { get; set; }
        public virtual string DTAG { get; set; }
        public virtual string MACHINE_NO { get; set; }
        public virtual string ACCODE { get; set; }
        public virtual decimal? TDS_CODE { get; set; }
        public virtual string GCODE { get; set; }
        public virtual string YTD_FIGURE { get; set; }
        public virtual string CATE_CODE { get; set; }
        public virtual string FFCODE { get; set; }
        public virtual string COMP_NAME { get; set; }
        public virtual string ADD1 { get; set; }
        public virtual string ADD2 { get; set; }
        public virtual string CITY { get; set; }
        public virtual string PIN { get; set; }
        public virtual string AC_NO { get; set; }
        public virtual string PRINT_TAG { get; set; }
        public virtual string BANK_NAME { get; set; }
        public virtual string PAN_NO { get; set; }
        public virtual string SERV_TAX_NO { get; set; }
        public virtual string RTGS_CODE { get; set; }
        public virtual string MICR_CODE { get; set; }
        public virtual string BRANCH { get; set; }
        public virtual string COMPANY { get; set; }
        public virtual string GACODE { get; set; }
        public virtual string GSUBCODE { get; set; }
        public virtual string NONOP_REASON { get; set; }
        public virtual string SERV_TAX_APP { get; set; }
        public virtual decimal? SERV_TAX_ABETMENT { get; set; }
        public virtual decimal? SERV_TAX_EXEMPTION { get; set; }
        public virtual string GST_NO { get; set; }
        public virtual DateTime? GST_REGDT { get; set; }
        public virtual string ADHAAR_NO { get; set; }
        public virtual string STATE_CODE { get; set; }
        public virtual string EMAIL_ID { get; set; }
        public virtual string TCS_TAG { get; set; }
        public virtual string SWIFT_CODE { get; set; }
        public virtual string TDS_TAG { get; set; }
        public virtual string SPECIFIED_PERSON { get; set; }
        public virtual string SUPP_TYPE_CD { get; set; }
        public virtual string MSME_REG_NO { get; set; }
        public virtual DateTime? MSME_REG_DT { get; set; }
        public virtual string ESIC_NO { get; set; }
        public virtual DateTime? ESIC_REG_DT { get; set; }
        public virtual int ID { get; set; }
        public virtual string USER_NAME_NEW { get; set; }
        public virtual DateTime? USER_ENTDT_NEW { get; set; }
        public virtual string USER_NAME { get; set; }
        public virtual DateTime? USER_ENTDT { get; set; }
        public virtual string OS_USER { get; set; } = Environment.UserName;
        public virtual string TERMINAL_ID { get; set; } = System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString();

        public string _acodeName { get; set; } 
    }
    public class M_BH21dtl
    {
        public M_BH21 m_bh21 { get; set; }
    }
}