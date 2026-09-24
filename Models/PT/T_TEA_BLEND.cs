using System;

namespace PacketTea.Models.PT
{
    // Mirrors ClassicERPCoreAPI.Models.PacketTea.PacketTea_OtherModel.T_TEA_BLEND
    public class T_TEA_BLEND
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

        public string ALLOC_CODE { get; set; }
        public string BLEND_NO { get; set; }
        public string BLEND_GRADE { get; set; }
        public DateTime? BLEND_DATE { get; set; }
        public string BLEND_WH { get; set; }
        public string BLEND_DESCN { get; set; }
        public string BLEND_MARK { get; set; }
        public decimal? BLEND_CHEST { get; set; }
        public decimal? BLEND_CHEST_WT { get; set; }
        public decimal? BLEND_CHEST_GROSSWT { get; set; }
        public decimal? BLEND_QTY { get; set; }
        public decimal? TOTQTY { get; set; }
        public decimal? TOTAMT { get; set; }
        public string PCODE { get; set; }
        public string TPT { get; set; }
        public string EXPREF_NO { get; set; }
        public string SHIP_MARK { get; set; }
        public string NOTES { get; set; }
        public string SP_NOTES { get; set; }
        public string CLOSED { get; set; }
        public string R_DOCNO { get; set; }
        public DateTime? R_DOCDT { get; set; }

        // Display-only fields returned by the API alongside the raw row
        public string PartyName { get; set; }
        public string WarehouseName { get; set; }
        public string AllocationName { get; set; }
        public string BlendMarkName { get; set; }
        public string BlendGradeName { get; set; }
        public string TransporterName { get; set; }

        // Final Blend Entry only: the Master Blend (R_DOCNO/R_DOCDT) this row was
        // raised against, resolved to its own business-friendly Blend No/Date/Qty
        // (FinalBlendController.GetByPage/GetByDocNo). Always blank for a plain
        // Master Blend Entry row.
        public string MasterBlendNo { get; set; }
        public string MasterBlendDate { get; set; }
        public decimal? MasterBlendQty { get; set; }
        public decimal? RemainingQty { get; set; }
        // Edit-load only (FinalBlend/GetByDocNo): the Master's own qty and what every Final Blend
        // raised against it has issued (this one included). Display-only -- unlike
        // MasterBlendQty they never feed the Blend Qty rescale baseline.
        public decimal? MasterQty { get; set; }
        public decimal? IssuedQty { get; set; }
    }
}
