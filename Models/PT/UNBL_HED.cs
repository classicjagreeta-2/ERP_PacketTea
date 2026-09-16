//using Finance.Models.DTO;
using PagedList;
using System;
using System.Collections.Generic;

namespace PacketTea.Models.PT
{

    public class UNBL_HED
    {
        public string loca { get; set; } = Utility.SessionHelper.GetUser().Loca;
        public string rt { get; set; } = " ";
        public string fre { get; set; }
        public string blno { get; set; }
        public DateTime? bldt { get; set; }
        public string pcd { get; set; }
        public string tpt { get; set; }
        public decimal? maxload { get; set; }
        public string tm { get; set; } = " ";
        public string unit { get; set; }
        public string tim { get; set; }
        public string timrem { get; set; }
        public DateTime? remdt { get; set; }
        public string veh { get; set; }
        public string remarks { get; set; }
        public string waY12 { get; set; }
        public string routeno { get; set; }
        public string sman { get; set; }
        public string waybilL_NO { get; set; }
        public string lR_NO { get; set; }
        public string vtype { get; set; }
        public string ordeR_NO { get; set; }
        public string tpT_PAID { get; set; }
        public decimal? amt { get; set; }
        public decimal? cess { get; set; }
        public decimal? spl { get; set; }
        public decimal? cst { get; set; }
        public decimal? surchg { get; set; }
        public decimal? rental { get; set; }
        public decimal? deliv { get; set; }
        public decimal? kodeliv { get; set; }
        public decimal? packing { get; set; }
        public decimal? deposit { get; set; }
        public decimal? discount { get; set; }
        public decimal? totax { get; set; }
        public decimal? stax { get; set; }
        public decimal? distcom { get; set; }
        public decimal? asS_VALUE { get; set; }
        public decimal? etax { get; set; }
        public string delivery { get; set; }
        public string adD1 { get; set; }
        public string adD2 { get; set; }
        public string adD3 { get; set; }
        public string city { get; set; }
        public string pin { get; set; }
        public string useR_ORA { get; set; } = Utility.SessionHelper.GetUser().getUserName;
        public DateTime? entdt { get; set; }
        public string timE_ORA { get; set; } = " ";
        public string useR_NEW { get; set; }
        public DateTime? entdT_NEW { get; set; }
        public string timE_NEW { get; set; }
        public string dtag { get; set; }
        public decimal? iD_ENT { get; set; }
        public decimal? iD_EDIT { get; set; }
        public string asm { get; set; }
        public string rse { get; set; }
        public string acd { get; set; }
        public string machinE_NO { get; set; }
        public string stno { get; set; }
        public DateTime? stdate { get; set; }
        public string stmtno { get; set; }
        public DateTime? stmtdt { get; set; }
        public string prinT_NO { get; set; }
        public DateTime? prinT_DATE { get; set; }
        public string seC_SALE { get; set; }
        public string triP_NO { get; set; }
        public string vaT_TYPE { get; set; }
        public string vaT_NO { get; set; }
        public decimal? frtratE_1 { get; set; }
        public decimal? frtratE_2 { get; set; }
        public string triP_UNIT { get; set; }
        public string bilL_LOCA { get; set; }
        public string tranS_PCD { get; set; }
        public string doC_YEAR { get; set; } 
        public string smcode { get; set; }
        public decimal? hfrtratE_1 { get; set; }
        public decimal? hfrtratE_2 { get; set; }
        public string cstcd { get; set; } = " ";
        public string c_FORM { get; set; }
        public decimal? toT_QTY { get; set; }
        public decimal? conS_RATE { get; set; }
        public decimal? conS_AMT { get; set; }
        public string loader { get; set; }
        public string loadeR2 { get; set; }
        public string inV_TYPE { get; set; }
        public string partY_ORDER_NO { get; set; }
        public DateTime? partY_ORDER_DT { get; set; }
        public string ewaybilL_NO { get; set; }
        public DateTime? ewaybilL_DT { get; set; }
        public string ewaybilL_TM { get; set; }
        public DateTime? ewaybilL_VALID_DT { get; set; }
        public string ewaybilL_VALID_TM { get; set; }
        public string loading { get; set; }
        public string approved { get; set; }
        public string prinT_TIME { get; set; }
        public DateTime? loadinG_DATE { get; set; }
        public string loadinG_TIME { get; set; }
        public DateTime? approveD_DATE { get; set; }
        public string approveD_TIME { get; set; }
        public string cnotE_NO { get; set; }
        public DateTime? cnotE_DT { get; set; }
        public string challaN_NO { get; set; }
        public DateTime? challaN_DT { get; set; }
        public string approveD_BY { get; set; }
        public string irn { get; set; }
        public string einV_ACKNO { get; set; }
        public DateTime? einV_ACKDT { get; set; }
        public string einV_ACKTM { get; set; }
        public string signedqrcode { get; set; }
        public string driveR_MOBILENO { get; set; }
        public string vxceeD_TAG { get; set; }
        public int id { get; set; }
        public string banK_CODE { get; set; }
        public string narration { get; set; }
        public string lockedbyuserid { get; set; }
        public DateTime? lockeduntil { get; set; }
        public string BillTo { get; set; }
        public string BillToPartyName { get; set; }

        // For Unit of Calculation radio buttons
        public string flag { get; set; }
    }

    public class UNBLDATA
    {
        public UNBL_HED unbL_HED { get; set; }
        public List<UNBL> unbl { get; set; }
        public List<UNBLPLT> unblplt { get; set; }
    }
}

