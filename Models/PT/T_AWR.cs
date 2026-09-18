using System;

namespace PacketTea.Models.PT
{
    // Mirrors ClassicERPCoreAPI.Models.PacketTea.PacketTea_OtherModel.T_AWR.
    // T_AWR is denormalized -- header fields (AWR_NO, AWR_DATE, WAREHOUSE,
    // SALE_CENTRE, ...) repeat on every line row; there is no separate header row.
    public class T_AWR
    {
        public int ID { get; set; }
        public string LOCA { get; set; }
        public string GLOCA { get; set; }
        public string UNIT { get; set; }
        public string AWR_NO { get; set; }
        public DateTime AWR_DATE { get; set; }
        public DateTime? ARRIVAL_DATE { get; set; }
        public string WAREHOUSE { get; set; }
        public string GARDEN { get; set; }
        public string REC_FROM { get; set; }
        public decimal? SRLNO { get; set; }
        public string GP_NO { get; set; }
        public DateTime? GP_DATE { get; set; }
        public string INVNO { get; set; }
        public DateTime? PACK_DATE { get; set; }
        public string GRADE { get; set; }
        public decimal? BAG_CHEST { get; set; }
        public string MARK { get; set; }
        public decimal? CHESTSLF { get; set; }
        public decimal? CHESTSLT { get; set; }
        public decimal? NET_WT { get; set; }
        public decimal? TNET_WT { get; set; }
        public decimal? TGROSS_WT { get; set; }
        public decimal? BAG_RCVD { get; set; }
        public string DOCNO { get; set; }
        public string REMARK { get; set; }
        public string TRAN_TYPE { get; set; }
        public string SALE_CENTRE { get; set; }
        public decimal? NET_RCVD { get; set; }
        public decimal? GROSS_RCVD { get; set; }
        public string DOC_YEAR { get; set; }

        // Display-only fields returned by the API alongside the raw row.
        public string MarkName { get; set; }
        public string GradeName { get; set; }
        public string WarehouseName { get; set; }
        public string SaleCentreName { get; set; }
    }
}
