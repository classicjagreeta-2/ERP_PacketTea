using System.Collections.Generic;
using System.Data;

namespace PacketTea.Models.PT
{
    // Crystal data source for ~/CrystalReport/StockReport.rpt: table "StockRpt"
    // (schema in CrystalReport/StockReport.xsd). Item rows come first (ROW_KIND "D"),
    // then the grand total row(s) (ROW_KIND "T", one per Packet/Bag and Net Wt.) --
    // sort on SEQ and format the detail bold when ROW_KIND = "T". Grand totals are
    // rows rather than Crystal summaries because bag.packet quantities cannot be
    // summed. Formula fields the controller fills when present: HEADER1 (company),
    // HEADER2 (address), TITLE, UNIT_TEXT, RUN_INFO, SKIPPED.
    public static class StockReportCrystal
    {
        public const string TableName = "StockRpt";

        public static DataSet Build(StockReportResult r)
        {
            var dt = new DataTable(TableName);
            dt.Columns.Add("SEQ", typeof(int));
            dt.Columns.Add("ROW_KIND", typeof(string));
            dt.Columns.Add("ITCD", typeof(string));
            dt.Columns.Add("DESCN", typeof(string));
            dt.Columns.Add("TYP", typeof(string));
            dt.Columns.Add("TYPE_TEXT", typeof(string));
            string[] num = { "OP", "PROD", "PURCH", "MRETU", "TOTAL_RECT", "LOADOUT", "CHECKIN", "DIRECT_SALE",
                             "INDIRECT_SALE", "TRANS", "PURCH_RETU", "TOTAL_DESP", "ADJ", "SAMPLE", "BKG_LKG",
                             "SHT", "TOTAL_OUT", "CL_STOCK", "GAIN_LOSS" };
            foreach (var n in num) dt.Columns.Add(n, typeof(decimal));

            int seq = 0;
            System.Action<StockReportRow, string> add = (x, kind) => dt.Rows.Add(
                ++seq, kind, x.ITCD ?? "", x.DESCN ?? "", x.TYP ?? "", x.TypeText,
                x.OP, x.PROD, x.PURCH, x.MRETU, x.TOTAL_RECT, x.LOADOUT, x.CHECKIN, x.DIRECT_SALE,
                x.INDIRECT_SALE, x.TRANS, x.PURCH_RETU, x.TOTAL_DESP, x.ADJ, x.SAMPLE, x.BKG_LKG,
                x.SHT, x.TOTAL_OUT, x.CL_STOCK, x.GAIN_LOSS);
            foreach (var x in r.Rows ?? new List<StockReportRow>()) add(x, "D");
            foreach (var x in r.Totals ?? new List<StockReportRow>()) add(x, "T");

            var ds = new DataSet("ReportDataSet");
            ds.Tables.Add(dt);
            return ds;
        }
    }

    // Mirrors of the API's StockReportController DTOs (VB6 fullmove_depo.frm).
    // Keep every property here: a field missing on this side is silently dropped.
    public class StockReportResult
    {
        public string Company { get; set; }
        public string Address { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string Units { get; set; }
        public string QtyWt { get; set; }
        public List<string> Skipped { get; set; } = new List<string>();
        public List<StockReportRow> Rows { get; set; } = new List<StockReportRow>();
        public List<StockReportRow> Totals { get; set; } = new List<StockReportRow>();

        // Set by the MVC controller for the Graphics footer (VB6 LASTCAPTION).
        public string RunBy { get; set; }
        public string RunAt { get; set; }
    }

    public class StockReportRow
    {
        public string ITCD { get; set; }
        public string DESCN { get; set; }
        public string TYP { get; set; }
        public decimal OP { get; set; }
        public decimal PROD { get; set; }
        public decimal PURCH { get; set; }
        public decimal MRETU { get; set; }
        public decimal TOTAL_RECT { get; set; }
        public decimal LOADOUT { get; set; }
        public decimal CHECKIN { get; set; }
        public decimal DIRECT_SALE { get; set; }
        public decimal INDIRECT_SALE { get; set; }
        public decimal TRANS { get; set; }
        public decimal PURCH_RETU { get; set; }
        public decimal TOTAL_DESP { get; set; }
        public decimal ADJ { get; set; }
        public decimal SAMPLE { get; set; }
        public decimal BKG_LKG { get; set; }
        public decimal SHT { get; set; }
        public decimal TOTAL_OUT { get; set; }
        public decimal CL_STOCK { get; set; }
        public decimal GAIN_LOSS { get; set; }

        public string TypeText => TYP == "A" ? "Packet/Bag" : TYP == "B" ? "Net Wt." : "";
    }
}
