using System;

namespace PacketTea.Models.PT
{
    // Mirrors ClassicERPCoreAPI.Models.PacketTea.PacketTea_OtherModel.TRN_INV_DETAIL.
    public class T_INV_DETAIL
    {
        public int ID { get; set; }

        public string DOC_YEAR { get; set; }
        public string LOCA { get; set; }
        public string UNIT { get; set; }
        public string DOCTYPE { get; set; }
        public string DOCNO { get; set; }
        public DateTime? DOCDT { get; set; }
        public decimal? SRLNO { get; set; }

        public string ITCD { get; set; }
        public string DESCN { get; set; }
        public string HSN_CODE { get; set; }
        public decimal? QTY { get; set; }
        public string UOM { get; set; }
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

        // Display-only field returned by the API alongside the raw row.
        public string ItemName { get; set; }
    }
}
