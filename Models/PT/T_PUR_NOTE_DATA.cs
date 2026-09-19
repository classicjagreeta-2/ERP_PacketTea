using System;
using System.Collections.Generic;

namespace PacketTea.Models.PT
{
    // Save payload of the Tea Purchase Debit / Credit Note screen. Mirrors the API's
    // TEA_PUR_NOTE_DATA exactly: Services.PostAsync serializes with System.Text.Json
    // (names kept as written), so every property here must match the API name, or the
    // field silently never arrives.
    public class T_PUR_NOTE_DATA
    {
        public string TRAN_CODE { get; set; }
        public string UNIT { get; set; }
        public string SALE_TYPE { get; set; }
        public string DOCNO { get; set; }
        public DateTime? DOCDT { get; set; }
        public string PCD { get; set; }
        public string BLNO { get; set; }
        public DateTime? BLDT { get; set; }
        public string SALE_CENTRE { get; set; }
        public string QTY_RETURN { get; set; }
        public string REM1 { get; set; }
        public string REM2 { get; set; }
        public string REM3 { get; set; }
        public string REM4 { get; set; }
        public bool LAST_YR { get; set; }
        public List<T_PUR_NOTE_LINE> LINES { get; set; } = new List<T_PUR_NOTE_LINE>();
    }

    public class T_PUR_NOTE_LINE
    {
        public string INVNO { get; set; }
        public DateTime? PACK_DATE { get; set; }
        public string GRADE { get; set; }
        public string MARK { get; set; }
        public decimal BAG_CHEST { get; set; }
        public decimal NET_WT { get; set; }
        public decimal TNET_WT { get; set; }
        public decimal TGROSS_WT { get; set; }
        public decimal BILL_QTY { get; set; }
        public decimal SAMPLE_QTY { get; set; }
        public decimal OTHER_QTY { get; set; }
        public decimal RATE { get; set; }
        public decimal AMT { get; set; }
    }
}
