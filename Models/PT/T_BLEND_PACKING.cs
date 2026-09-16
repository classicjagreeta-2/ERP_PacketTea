using System;

namespace PacketTea.Models.PT
{
    // Mirrors ClassicERPCoreAPI.Models.PacketTea.PacketTea_OtherModel.T_BLEND_PACKING
    // (a subset -- only the columns the Packing Entry grid actually edits/posts;
    // property NAMES must match the API's exactly since the JSON posted through
    // Save() is bound server-side by System.Text.Json, which is case-sensitive
    // by default unlike Newtonsoft).
    public class T_BLEND_PACKING
    {
        public decimal? ID { get; set; }
        public string SL_NO { get; set; }

        public string MARK { get; set; }
        public string INVNO { get; set; }
        public string GRADE { get; set; }
        public string CAT { get; set; }
        public string BATCHNO { get; set; }
        public decimal? BAG_CHEST { get; set; }
        public decimal? NOB { get; set; }
        public decimal? QTY { get; set; }
        public decimal? GROSS { get; set; }
        public decimal? TARE { get; set; }
        public decimal? CHESTSLF { get; set; }
        public decimal? CHESTSLT { get; set; }
        public string SZ_CODE { get; set; }
        public DateTime? MFG_FROM { get; set; }
        public DateTime? MFG_TO { get; set; }
        public string SEASON { get; set; }
        public DateTime? PACK_DATE { get; set; }
        public decimal? RATE { get; set; }
        public decimal? AMOUNT { get; set; }
        public string ALLOCATION { get; set; }
        public string ITCD { get; set; }
        public string MRP { get; set; }

        // A row can source from a different Final Blend than the doc's header
        // one (see BlendPackingController's class comment on the per-row picker
        // fix) -- left blank, it inherits the header's BlendDocNo/BlendDocDt.
        public string BLEND_DOCNO { get; set; }
        public DateTime? BLEND_DOCDT { get; set; }

        // Display-only fields returned by the API alongside the raw row -- the grid
        // shows each coded column next to its description (the VB6 grid's Mark Code/
        // Mark, Grade Code/Grade, Cat/Category, Size Code/Size, Alloc Code/Allocation
        // pairs). These must be declared here even though nothing posts them back: the
        // MVC controller round-trips the API's JSON through this typed shape, so a
        // property missing from this class is dropped and reaches the grid as undefined.
        public string MarkName { get; set; }
        public string GradeName { get; set; }
        public string CategoryName { get; set; }
        public string SzDesc { get; set; }
        public string AllocationName { get; set; }
        public decimal? NET { get; set; }
    }
}
