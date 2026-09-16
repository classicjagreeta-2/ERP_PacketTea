using DocumentFormat.OpenXml.Office2010.ExcelAc;
using System;
using System.Collections.Generic;

namespace PacketTea.Models.PT
{
    public class UNBL
    {
        public string itemName { get; set; }
        public string loca { get; set; } = Utility.SessionHelper.GetUser().Loca;
        public string rt { get; set; } = " ";
        public string fre { get; set; }
        public string scmcode { get; set; }
        public string blno { get; set; }
        public DateTime? bldt { get; set; }
        public DateTime? mfgdt { get; set; }
        public string batchno { get; set; }
        public string pcd { get; set; }
        public string tpt { get; set; }
        public string itcd { get; set; }
        public int? srlno { get; set; }
        public decimal? maxload { get; set; }
        public string mrp { get; set; }
        public string tm { get; set; } = " ";
        public string unit { get; set; }
        public decimal? qty { get; set; }
        public decimal? plrg { get; set; }
        public decimal? splrg { get; set; }
        public string tim { get; set; }
        public decimal? disc { get; set; }
        public string timrem { get; set; }
        public DateTime? remdt { get; set; }
        public string veh { get; set; }
        public decimal? rate { get; set; }
        public decimal? amt { get; set; }
        public string plcrcd { get; set; }
        public decimal? plcrqty { get; set; }
        public decimal? plcrate { get; set; }
        public decimal? plcrval { get; set; }
        public decimal? hdc { get; set; }
        public string kopvc { get; set; }
        public decimal? cess { get; set; }
        public decimal? spl { get; set; }
        public decimal? rental { get; set; }
        public decimal? kodeliv { get; set; }
        public decimal? deliv { get; set; }
        public decimal? packing { get; set; }
        public decimal? deposit { get; set; }
        public string cstcd { get; set; } = " ";
        public decimal? cst { get; set; }
        public decimal? surchg { get; set; }
        public decimal? discount { get; set; }
        public decimal? totax { get; set; }
        public decimal? stax { get; set; }
        public string cd { get; set; }
        public DateTime? datE1 { get; set; }
        public string remarks { get; set; }
        public decimal? distcom { get; set; }
        public decimal? asS_VALUE { get; set; }
        public decimal? etax { get; set; }
        public string waY12 { get; set; }
        public string routeno { get; set; }
        public string sman { get; set; }
        public DateTime? cancdt { get; set; }
        public string useR_ORA { get; set; } = Utility.SessionHelper.GetUser().getUserName;
        public DateTime? entdt { get; set; }
        public string timE_ORA { get; set; } = " ";
        public decimal? qtY_BTTLS { get; set; } = 0m;
        public string dtag { get; set; }
        public string lR_NO { get; set; }
        public string vtype { get; set; }
        public string waybilL_NO { get; set; }
        public string ordeR_NO { get; set; } = " ";
        public string tpT_PAID { get; set; }
        public string fdano { get; set; }
        public decimal? iD_ENT { get; set; }
        public decimal? iD_EDIT { get; set; }
        public decimal? asurchg { get; set; }
        public string useR_NEW { get; set; }
        public DateTime? entdT_NEW { get; set; }
        public string timE_NEW { get; set; }
        public string machinE_NO { get; set; }
        public string stno { get; set; }
        public DateTime? stdate { get; set; }
        public decimal? ecesS_BAS { get; set; }
        public decimal? ecesS_SPL { get; set; }
        public decimal? disC_RENT { get; set; }
        public decimal? puR_RATE { get; set; }
        public string vaT_TYPE { get; set; }
        public string vaT_NO { get; set; }
        public decimal? staX_OTH { get; set; }
        public decimal? staX_R_OTH { get; set; }
        public decimal? discprov { get; set; }
        public decimal? discrprov { get; set; }
        public decimal? discsprov { get; set; }
        public decimal? discrsprov { get; set; }
        public string cnno { get; set; }
        public DateTime? cndt { get; set; } 
        public decimal? discdis { get; set; }
        public decimal? discrdis { get; set; }
        public decimal? discsdis { get; set; }
        public decimal? discrsdis { get; set; }
        public string disC_STAT { get; set; }
        public string crdnotE_ST { get; set; }
        public string bilL_LOCA { get; set; }
        public string tranS_PCD { get; set; }
        public string doC_YEAR { get; set; } 
        public string mrP_SLORD { get; set; }
        public decimal? edU_CESS { get; set; }
        public decimal? sheC_SPL { get; set; }
        public decimal? ecplrg { get; set; }
        public decimal? heplrg { get; set; }
        public decimal? tcS_PER { get; set; }
        public decimal? tcS_AMT { get; set; }
        public decimal? edU_PER { get; set; }
        public decimal? edU_AMT { get; set; }
        public decimal? shedU_PER { get; set; }
        public decimal? shedU_AMT { get; set; }
        public string fQ_ITCD { get; set; }
        public string fG_ITCD { get; set; }
        public decimal? fQ_QTY { get; set; }
        public decimal? fG_QTY { get; set; }
        public decimal? fQ_AMT { get; set; }
        public decimal? fG_AMT { get; set; }
        public string fQ_MRP { get; set; }
        public string fG_MRP { get; set; }
        public string fQ1_ITCD { get; set; }
        public string fQ1_MRP { get; set; }
        public decimal? fQ1_QTY { get; set; }
        public decimal? fQ1_AMT { get; set; }
        public string fG1_ITCD { get; set; }
        public string fG1_MRP { get; set; }
        public decimal? fG1_QTY { get; set; }
        public decimal? fG1_AMT { get; set; }
        public string ratE_CODE { get; set; }
        public string disC_CODE { get; set; }
        public string bilL_REMARK { get; set; }
        public string schM_CODE { get; set; }
        public string pono { get; set; }
        public DateTime? podate { get; set; }
        public string sO_ITCD { get; set; }
        public string sO_MRP { get; set; }
        public decimal? sO_QTY { get; set; }
        public string reaS_CODE { get; set; }
        public DateTime? sO_DESP_DATE { get; set; }
        public decimal? sO_AMT { get; set; }
        public decimal? noR_DISC { get; set; }
        public decimal? spL_DISC { get; set; }
        public string bparT_DCOM { get; set; }
        public string bparT_RMARG { get; set; }
        public decimal? bP_DCOM_VAL { get; set; }
        public decimal? bP_RMARG_VAL { get; set; }
        public string parA_SCH_TO_DISC { get; set; }
        public decimal? schemE_VALUE { get; set; }
        public decimal? totaL_BILL_VALUE { get; set; }
        public decimal? prE_ORDER_QTY { get; set; }
        public string prE_DOCNO { get; set; }
        public DateTime? prE_DOCDT { get; set; }
        public string prE_ORDER_NO { get; set; }
        public DateTime? prE_ORDER_DT { get; set; }
        public decimal? schM_DISC { get; set; }
        public decimal? reB_AMT { get; set; }
        public decimal? aeD_ECESS { get; set; }
        public decimal? aeD_SHEC { get; set; }
        public string bpdS_DOCNO { get; set; }
        public decimal? bpdS_AMT { get; set; }
        public string fqD_ITCD { get; set; }
        public string fqD_MRP { get; set; }
        public decimal? fqD_QTY { get; set; }
        public decimal? disT_SCHM_DISC { get; set; }
        public decimal? cgsT_AMT { get; set; }
        public decimal? sgsT_AMT { get; set; } = 0;
        public decimal? igsT_AMT { get; set; }
        public decimal? gcesS_AMT { get; set; }
        public decimal? cgsT_RATE { get; set; }
        public decimal? sgsT_RATE { get; set; }
        public decimal? igsT_RATE { get; set; }
        public decimal? gcesS_RATE { get; set; }
        public string gsT_CODE { get; set; }
        public decimal? gsT_DISCOUNT { get; set; }
        public string inV_TYPE { get; set; }
        public string inV_SOURCE { get; set; }
        public decimal? freE_AMT { get; set; }
        public decimal? reT_MARGIN { get; set; }
        public string gsT_PCD { get; set; }
        public decimal? claiM_DISC { get; set; }
        public decimal? coupoN_QTY { get; set; }
        public decimal? coupoN_AMT { get; set; }
        public string flag { get; set; }
        public decimal? weight { get; set; }
        public string cN_DOCNO { get; set; }
        public DateTime? cN_DOCDT { get; set; }
        public int id { get; set; }
        public decimal? grosS_WEIGHT { get; set; }
        public string itdesC1 { get; set; }
        public string casE_PACK { get; set; }
    }
    public class UNBL_HD_LIST
    {
        public List<UNBL> UNBL_LIST { get; set; } = new List<UNBL>();
    }
}
