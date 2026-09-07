using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PacketTea.Models.PacketTea
{
    public class Product
    {
        public int id { get; set; }
        public string loca { get; set; }
        public string unit { get; set; }
        public string itcd { get; set; }
        public string descn { get; set; }
        public string shT_CODE { get; set; }
        public string pricE_GROUP { get; set; }
        public string sizE_GROUP { get; set; }
        public string uom { get; set; }
        public string fgi { get; set; }
        public string ret { get; set; }
        public string fielD_PER { get; set; }
        public string fielD_GRAV { get; set; }
        public string fielD_DIP { get; set; }
        public string fielD_100_PER { get; set; }
        public string suB_HEAD { get; set; }
        public string grade { get; set; }
        public string aC_SALE { get; set; }
        public string aC_EXCISE { get; set; }
        public string aC_EDUCESS { get; set; }
        public string aC_FRT { get; set; }
        public string aC_CENTRAL_TAX { get; set; }
        public string aC_LOCAL_TAX { get; set; }
        public string aC_E_TAX { get; set; }
        public string aC_ROFF { get; set; }
        public string aC_FRT_PD { get; set; }
        public string seriaL_NO { get; set; }
        public string aC_SHEC { get; set; }
        public string aC_CONS { get; set; }
        public string aC_LIAB { get; set; }
        public string hsN_CODE { get; set; }
        public string reF_ORA { get; set; }
        public string cosT_CENTRE { get; set; }
        public string aC_SALE_SUBCODE { get; set; }
        public string useR_NAME { get; set; }
        public DateTime? useR_ENTDT { get; set; }
        public string useR_NAME_NEW { get; set; }
        public object useR_ENTDT_NEW { get; set; }
        public string oS_USER { get; set; }
        public string terminaL_ID { get; set; }
        public string dtag { get; set; }
    }
}