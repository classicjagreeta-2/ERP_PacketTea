using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PacketTea.Models.Master
{
    public class M_GSTMAS
    {
        public virtual string LOCA { get; set; } =Utility.SessionHelper.GetUser().Loca;
        public virtual int AUTOID { get; set; }
        public virtual string GST_CODE { get; set; }
        public virtual string GST_DESCN { get; set; }
        public virtual DateTime? EFF_DATE { get; set; }
        public virtual decimal? CGST_RATE { get; set; }
        public virtual decimal? SGST_RATE { get; set; }
        public virtual decimal? IGST_RATE { get; set; }
        public virtual decimal? GCESS_RATE { get; set; }
        public virtual string GST_TYPE { get; set; }
        public virtual string SALE_ACODE { get; set; }
        public virtual string SALE_SUBCODE { get; set; }
        public virtual string SALE_REFCD { get; set; }
        public virtual string SALE_COST_CENTRE { get; set; }
        public virtual string SALE_CGST_ACODE { get; set; }
        public virtual string SALE_CGST_SUBCODE { get; set; }
        public virtual string SALE_CGST_REFCD { get; set; }
        public virtual string SALE_CGST_COST_CENTRE { get; set; }
        public virtual string SALE_SGST_ACODE { get; set; }
        public virtual string SALE_SGST_SUBCODE { get; set; }
        public virtual string SALE_SGST_REFCD { get; set; }
        public virtual string SALE_SGST_COST_CENTRE { get; set; }
        public virtual string SALE_IGST_ACODE { get; set; }
        public virtual string SALE_IGST_SUBCODE { get; set; }
        public virtual string SALE_IGST_REFCD { get; set; }
        public virtual string SALE_IGST_COST_CENTRE { get; set; }
        public virtual string SALE_CESS_ACODE { get; set; }
        public virtual string SALE_CESS_SUBCODE { get; set; }
        public virtual string SALE_CESS_REFCD { get; set; }
        public virtual string SALE_CESS_COST_CENTRE { get; set; }
        public virtual string PUR_ACODE { get; set; }
        public virtual string PUR_SUBCODE { get; set; }
        public virtual string PUR_REFCD { get; set; }
        public virtual string PUR_COST_CENTRE { get; set; }
        public virtual string PUR_CGST_ACODE { get; set; }
        public virtual string PUR_CGST_SUBCODE { get; set; }
        public virtual string PUR_CGST_REFCD { get; set; }
        public virtual string PUR_CGST_COST_CENTRE { get; set; }
        public virtual string PUR_SGST_ACODE { get; set; }
        public virtual string PUR_SGST_SUBCODE { get; set; }
        public virtual string PUR_SGST_REFCD { get; set; }
        public virtual string PUR_SGST_COST_CENTRE { get; set; }
        public virtual string PUR_IGST_ACODE { get; set; }
        public virtual string PUR_IGST_SUBCODE { get; set; }
        public virtual string PUR_IGST_REFCD { get; set; }
        public virtual string PUR_IGST_COST_CENTRE { get; set; }
        public virtual string PUR_CESS_ACODE { get; set; }
        public virtual string PUR_CESS_SUBCODE { get; set; }
        public virtual string PUR_CESS_REFCD { get; set; }
        public virtual string PUR_CESS_COST_CENTRE { get; set; }
        public virtual string USER_ORA { get; set; }
        public virtual DateTime? ENTDT { get; set; }
        public virtual string TIME_ORA { get; set; }
        public virtual string DTAG { get; set; }
        public virtual string USER_NEW { get; set; }
        public virtual DateTime? ENTDT_NEW { get; set; }
        public virtual string TIME_NEW { get; set; }
        public virtual string MACHINE_NO { get; set; }
        public virtual string SALE_CGST_ACODE_RCM { get; set; }
        public virtual string SALE_SGST_ACODE_RCM { get; set; }
        public virtual string SALE_IGST_ACODE_RCM { get; set; }
        public virtual string SALE_CESS_ACODE_RCM { get; set; }
        public virtual string PUR_CGST_ACODE_RCM { get; set; }
        public virtual string PUR_SGST_ACODE_RCM { get; set; }
        public virtual string PUR_IGST_ACODE_RCM { get; set; }
        public virtual string PUR_CESS_ACODE_RCM { get; set; }
        public virtual decimal? TCS_RATE { get; set; }
        public virtual string TCS_ACODE { get; set; }
        public virtual string MRET_ACODE_P { get; set; }
        public virtual string MRET_ACODE_B { get; set; }
        public virtual string ADV_CGST_ACODE_P { get; set; }
        public virtual string ADV_SGST_ACODE_P { get; set; }
        public virtual string ADV_IGST_ACODE_P { get; set; }
        public virtual string ADV_CESS_ACODE_P { get; set; }
        public virtual string ADV_CGST_ACODE_R { get; set; }
        public virtual string ADV_SGST_ACODE_R { get; set; }
        public virtual string ADV_IGST_ACODE_R { get; set; }
        public virtual string ADV_CESS_ACODE_R { get; set; }
        public virtual decimal TCS_RATE_PAN { get; set; }
        public virtual string TCS_ACODE_PAN { get; set; }
        public virtual string PUR_TCS_ACODE { get; set; }
        public virtual string PUR_TCS_ACODE_PAN { get; set; }
        public virtual decimal? SPECIFIED_RATE { get; set; }
        public virtual string SPECIFIED_ACODE { get; set; }
        public virtual string PUR_SPECIFIED_ACODE { get; set; }
        public virtual string DISCOUNT_ACODE { get; set; }
        public virtual string USER_NAME { get; set; } =Utility.SessionHelper.GetUser().getUserName;
        public virtual DateTime? USER_ENTDT { get; set; }
        public virtual string USER_NAME_NEW { get; set; }
        public virtual DateTime? USER_ENTDT_NEW { get; set; }
        public virtual string OS_USER { get; set; } = Environment.UserName;
        public virtual string TERMINAL_ID { get; set; } = System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString();
    }
}