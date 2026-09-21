using System;
using System.Collections.Generic;

namespace PacketTea.Models.PT
{
    // Save payload of the Packet Tea Debit / Credit Note screen. Mirrors the API's
    // PT_NOTE_DATA exactly: Services.SalesPostAsync serializes with System.Text.Json
    // (names kept as written), so every property here must match the API name, or the
    // field silently never arrives.
    public class PT_NOTE_DATA
    {
        public string TRAN_CODE { get; set; }
        public string UNIT { get; set; }
        public string DOCNO { get; set; }
        public DateTime? DOCDT { get; set; }
        public string PCD { get; set; }
        public string BLNO { get; set; }
        public DateTime? BLDT { get; set; }
        public bool LAST_YR { get; set; }
        public DateTime? FROM_DATE { get; set; }
        public DateTime? UPTO_DATE { get; set; }
        public string REM1 { get; set; }
        public string REM2 { get; set; }
        public string REM3 { get; set; }
        public string REM4 { get; set; }
        public List<PT_NOTE_LINE> LINES { get; set; } = new List<PT_NOTE_LINE>();
    }

    public class PT_NOTE_LINE
    {
        public string ITCD { get; set; }
        public string MRP { get; set; }
        public DateTime? MFGDT { get; set; }
        public string BATCHNO { get; set; }
        public decimal QTY { get; set; }
        public decimal GROSS_WEIGHT { get; set; }
        public decimal WEIGHT { get; set; }
        public decimal RATE { get; set; }
        public decimal DISC { get; set; }
        public decimal DISCOUNT { get; set; }
        public string FLAG { get; set; }
        public string CSTCD { get; set; }
        public string GST_CODE { get; set; }
    }
}
