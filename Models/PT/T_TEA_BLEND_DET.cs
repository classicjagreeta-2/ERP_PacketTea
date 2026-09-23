using System;

namespace PacketTea.Models.PT
{
    // Mirrors ClassicERPCoreAPI.Models.PacketTea.PacketTea_OtherModel.T_TEA_BLEND_DET
    public class T_TEA_BLEND_DET
    {
        public decimal? ID { get; set; }

        public string LOCA { get; set; }
        public string GLOCA { get; set; }
        public string UNIT { get; set; }
        public string DOCNO { get; set; }
        public DateTime DOCDT { get; set; }
        public string DOC_YEAR { get; set; }
        public string BLEND_TYPE { get; set; }
        public string APPROVED { get; set; }
        public decimal? SLNO { get; set; }

        public string MARK { get; set; }
        public string INVNO { get; set; }
        public DateTime? PACK_DATE { get; set; }
        public string GRADE { get; set; }
        public decimal? BAG_CHEST { get; set; }
        public decimal? QTY { get; set; }
        public decimal? SAMPLE_QTY { get; set; }
        public decimal? RATE { get; set; }
        public decimal? AMOUNT { get; set; }
        public string WH_LOCA { get; set; }
        public string WAREHOUSE { get; set; }
        public string AWR_NO { get; set; }
        public DateTime? AWR_DATE { get; set; }
        public string DONO { get; set; }
        public DateTime? DODT { get; set; }
        public string TRANS_CODE { get; set; }
        public string R_DOCNO { get; set; }
        public DateTime? R_DOCDT { get; set; }

        public string MarkName { get; set; }
        public string GradeName { get; set; }
        public string TransporterName { get; set; }
    }
}
