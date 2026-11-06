using System.Collections.Generic;

namespace PacketTea.Models.PT
{
    // Mirrors of the API's SampleGainLossController DTOs (Classic ERP's Sample Gain Loss Register).
    // Keep every property here: a field missing on this side is silently dropped.
    public class SampleGainLossPick
    {
        public string CODE { get; set; }
        public string DESCN { get; set; }
    }

    public class SampleGainLossResult
    {
        public string Company { get; set; }
        public string CompanyV { get; set; }
        public string Address { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string Units { get; set; }
        public List<SampleGainLossRow> Rows { get; set; } = new List<SampleGainLossRow>();
    }

    public class SampleGainLossRow
    {
        public int TYP { get; set; }
        public string SAMPLE_LETTER_NO { get; set; }
        public string SAMPLE_LETTER_DATE { get; set; }
        public string PACKING_NO { get; set; }
        public string PACKING_DATE { get; set; }
        public string BLEND_NO { get; set; }
        public string SALE_BILL_NO { get; set; }
        public string GARDEN { get; set; }
        public string INVNO { get; set; }
        public decimal SAMPLE { get; set; }
        public decimal GAIN { get; set; }
        public decimal LOSS { get; set; }
        public string AWR_NO { get; set; }
        public string AWR_DATE { get; set; }
        public decimal SHORT_QTY { get; set; }
        public string MFG_DATE { get; set; }
    }
}
