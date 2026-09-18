using System;

namespace PacketTea.Models.PT
{
    // Mirrors ClassicERPCoreAPI.Models.PacketTea.PacketTea_OtherModel.T_SAMPLE_DRAW.
    // Also denormalized -- header fields repeat on every line row.
    public class T_SAMPLE_DRAW
    {
        public int ID { get; set; }
        public string LOCA { get; set; }
        public string GLOCA { get; set; }
        public string UNIT { get; set; }
        public string DOCNO { get; set; }
        public DateTime? DOCDT { get; set; }
        public string SALE_CENTRE { get; set; }
        public string BROK_CODE { get; set; }
        public string PCODE { get; set; }
        public string WAREHOUSE { get; set; }
        public string AWR_NO { get; set; }
        public DateTime? AWR_DATE { get; set; }
        public string GARDEN { get; set; }
        public decimal? SRLNO { get; set; }
        public string INVNO { get; set; }
        public DateTime? PACK_DATE { get; set; }
        public string MARK { get; set; }
        public string GRADE { get; set; }
        public string CAT { get; set; }
        public decimal? BAG_CHEST { get; set; }
        public decimal? NET_WT { get; set; }
        public decimal? DRAW_QTY { get; set; }
        public string TRAN_CODE { get; set; }
        public string DOC_YEAR { get; set; }

        // Display-only fields returned by the API alongside the raw row.
        public string MarkName { get; set; }
        public string GradeName { get; set; }
        public string SaleCentreName { get; set; }
        public string BrokerName { get; set; }
        public string PartyName { get; set; }
        public string WarehouseName { get; set; }
    }
}
