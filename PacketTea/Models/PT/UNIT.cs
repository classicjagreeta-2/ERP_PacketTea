using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PacketTea.Models.Master
{
    public class UNIT
    {
        public virtual int ID { get; set; }
        public virtual string LOCA { get; set; } =Utility.SessionHelper.GetUser().Loca;
        public virtual string CODE { get; set; }
        public virtual string SCODE { get; set; }
        public virtual string DIRE { get; set; }
        public virtual string NAME { get; set; }
        public virtual string CONAME { get; set; }
        public virtual string ADD1 { get; set; }
        public virtual string ADD2 { get; set; }
        public virtual string CSTNO { get; set; }
        public virtual DateTime? CSTDT { get; set; }
        public virtual string LSTNO { get; set; }
        public virtual DateTime? LSTDT { get; set; }
        public virtual string ECCNO { get; set; }
        public virtual string RANGE { get; set; }
        public virtual string DIV { get; set; }
        public virtual string CEREGDNO { get; set; }
        public virtual string PLANO { get; set; }
        public virtual string UNSRTEMPT { get; set; }
        public virtual string STAXCD { get; set; }
        public virtual string SIMB { get; set; }
        public virtual string USER_ORA { get; set; }
        public virtual string TIME_ORA { get; set; }
        public virtual DateTime? ENTDT { get; set; }
        public virtual string USER_NEW { get; set; }
        public virtual DateTime? ENTDT_NEW { get; set; }
        public virtual string TIME_NEW { get; set; }
        public virtual string DTAG { get; set; }
        public virtual string MACHINE_NO { get; set; }
        public virtual string AUTOTRNS { get; set; }
        public virtual string AUTOTRNSF { get; set; }
        public virtual string AUTOBL { get; set; }
        public virtual string FRT { get; set; }
        public virtual string DISC { get; set; }
        public virtual string VAT { get; set; }
        public virtual string MULTI_NO { get; set; }
        public virtual string SLOC { get; set; }
        public virtual string PLANT_CODE { get; set; }
        public virtual string CR_MANG { get; set; }
        public virtual string ROUTE_JUMP { get; set; }
        public virtual string T_UNIT { get; set; }
        public virtual string M_UNIT { get; set; }
        public virtual string EXPS { get; set; }
        public virtual string C_FREE_DS { get; set; }
        public virtual string MR_F_ENT { get; set; }
        public virtual string PU_F_ENT { get; set; }
        public virtual string SL_ORDER { get; set; }
        public virtual string SL_ORDER_APP { get; set; }
        public virtual string DEPO_SCHEMA_NAME { get; set; }
        public virtual string DEPO_UNIT1 { get; set; }
        public virtual string DEPO_UNIT2 { get; set; }
        public virtual string REGION { get; set; }
        public virtual string PROD_EMPTY_CHECK { get; set; }
        public virtual string MFG_UNIT { get; set; }
        public virtual string CIN_NO { get; set; }
        public virtual string REG_ADD1 { get; set; }
        public virtual string REG_ADD2 { get; set; }
        public virtual string REG_ADD3 { get; set; }
        public virtual string REG_CITY { get; set; }
        public virtual string REG_PIN { get; set; }
        public virtual string EMAIL_ID { get; set; }
        public virtual string WEBSITE { get; set; }
        public virtual string STATE { get; set; }
        public virtual string GST_NO { get; set; }
        public virtual string PAN_NO { get; set; }
        public virtual string INV_FMT { get; set; }
        public virtual string FSSAI_NO { get; set; }
        public virtual decimal NO_OF_COPY { get; set; }
        public virtual string ACTIVE_TAG { get; set; }
        public virtual string CITY { get; set; }
        public virtual string PIN { get; set; }
        public virtual string COMMITIONERATE { get; set; }
        public virtual decimal CLAIM_DISC_PER { get; set; }
        public virtual string USER_NAME_NEW { get; set; }
        public virtual DateTime? USER_ENTDT_NEW { get; set; }
        public virtual string USER_NAME { get; set; } =Utility.SessionHelper.GetUser().getUserName;
        public virtual DateTime? USER_ENTDT { get; set; }
        public virtual string OS_USER { get; set; }
        public virtual string TERMINAL_ID { get; set; }
    }
}