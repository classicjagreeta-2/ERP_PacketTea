namespace PacketTea.Models.PT
{
    // Mirrors ClassicERPCoreAPI.Models.PacketTea.PacketTea_OtherModel.TRN_INV_DETAIL.
    // RATE/AMT are what the grid shows: for a tax-inclusive document the rate the
    // user typed, not the stored tax-exclusive base (the API derives that on save).
    public class T_INV_DETAIL
    {
        public decimal? SRLNO { get; set; }
        public string ITCD { get; set; }
        public string ITDESC1 { get; set; }
        public string ITDESC2 { get; set; }
        public string ITDESC3 { get; set; }
        public string ITDESC4 { get; set; }
        public string UNIT_DESC { get; set; }       // UOM
        public string HSN_CODE { get; set; }
        public decimal? QTY { get; set; }
        public decimal? RATE { get; set; }
        public decimal? AMT { get; set; }

        public string GST_CODE { get; set; }
        public decimal? CGST_RATE { get; set; }
        public decimal? CGST_AMT { get; set; }
        public decimal? SGST_RATE { get; set; }
        public decimal? SGST_AMT { get; set; }
        public decimal? IGST_RATE { get; set; }
        public decimal? IGST_AMT { get; set; }
        public decimal? GCESS_RATE { get; set; }
        public decimal? GCESS_AMT { get; set; }
        public decimal? TCS_RATE { get; set; }
        public decimal? TCS_AMT { get; set; }
        public decimal? TOT_AMT { get; set; }

        // Display-only field returned by the API alongside the row.
        public string ITEM_NAME { get; set; }
    }
}
