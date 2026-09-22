using System.Collections.Generic;
using System.Data;

namespace PacketTea.Models.PT
{
    // Crystal data source for ~/CrystalReport/FullsStockActual.rpt: one row per
    // batch line (table "FullsStockRpt", schema in CrystalReport/FullsStockActual.xsd).
    // Item and grand totals ride on every row because bag.packet quantities cannot
    // be summed in Crystal -- group on ITCD and show ITEM_* in its group footer and
    // GRAND_* in the report footer. SEQ keeps the API's order (Item Code / Name / Custom).
    // Formula fields the controller fills when present: HEADER1 (company), HEADER2
    // (address), TITLE, UNIT_TEXT, RUN_INFO.
    public static class FullsStockCrystal
    {
        public const string TableName = "FullsStockRpt";

        public static DataSet Build(FullsStockReport r)
        {
            var dt = new DataTable(TableName);
            dt.Columns.Add("SEQ", typeof(int));
            dt.Columns.Add("ITEM_SEQ", typeof(int));
            dt.Columns.Add("FIRST_IN_ITEM", typeof(bool));
            dt.Columns.Add("ITCD", typeof(string));
            dt.Columns.Add("DESCRIPTION", typeof(string));
            dt.Columns.Add("UNIT", typeof(string));
            dt.Columns.Add("TM", typeof(string));
            dt.Columns.Add("MRP", typeof(string));
            dt.Columns.Add("MFGDT", typeof(string));
            dt.Columns.Add("BATCHNO", typeof(string));
            dt.Columns.Add("QTY", typeof(decimal));
            dt.Columns.Add("GROSS_WEIGHT", typeof(decimal));
            dt.Columns.Add("NET_WEIGHT", typeof(decimal));
            dt.Columns.Add("IDOD", typeof(string));
            dt.Columns.Add("DOD", typeof(string));
            dt.Columns.Add("BBD", typeof(string));
            dt.Columns.Add("AGE", typeof(int));
            dt.Columns.Add("STATUS", typeof(string));
            dt.Columns.Add("ITEM_QTY", typeof(decimal));
            dt.Columns.Add("ITEM_GROSS_WEIGHT", typeof(decimal));
            dt.Columns.Add("ITEM_NET_WEIGHT", typeof(decimal));
            dt.Columns.Add("GRAND_QTY", typeof(decimal));
            dt.Columns.Add("GRAND_GROSS_WEIGHT", typeof(decimal));
            dt.Columns.Add("GRAND_NET_WEIGHT", typeof(decimal));

            int seq = 0, itemSeq = 0;
            foreach (var it in r.Items ?? new List<FullsStockItem>())
            {
                itemSeq++;
                bool first = true;
                foreach (var l in it.Lines)
                {
                    dt.Rows.Add(++seq, itemSeq, first, it.ITCD, it.NAME, l.UNIT, l.TM, l.MRP, l.MFGDT, l.BATCHNO,
                                l.QTY, l.GROSS_WEIGHT, l.NET_WEIGHT, l.IDOD, l.DOD, l.BBD,
                                string.IsNullOrEmpty(l.MFGDT) ? 0 : l.AGE, l.STATUS ?? "",
                                it.QTY, it.GROSS_WEIGHT, it.NET_WEIGHT,
                                r.GRAND_QTY, r.GRAND_GROSS_WEIGHT, r.GRAND_NET_WEIGHT);
                    first = false;
                }
            }
            var ds = new DataSet("ReportDataSet");
            ds.Tables.Add(dt);
            return ds;
        }
    }

    // Mirrors of the API's FullsStockActualController DTOs (VB6 rep_dank_tpm.frm).
    // Keep every property here: a field missing on this side is silently dropped.
    public class FullsStockUnit
    {
        public string CODE { get; set; }
        public string NAME { get; set; }
    }

    public class FullsStockReport
    {
        public string Company { get; set; }
        public string Address { get; set; }
        public string AsOn { get; set; }
        public string Unit { get; set; }
        public List<FullsStockItem> Items { get; set; } = new List<FullsStockItem>();
        public decimal GRAND_QTY { get; set; }
        public decimal GRAND_GROSS_WEIGHT { get; set; }
        public decimal GRAND_NET_WEIGHT { get; set; }

        // Set by the MVC controller for the Graphics footer (VB6 LASTCAPTION).
        public string RunBy { get; set; }
        public string RunAt { get; set; }
    }

    public class FullsStockItem
    {
        public string ITCD { get; set; }
        public string NAME { get; set; }
        public decimal NOB { get; set; }
        public List<FullsStockLine> Lines { get; set; } = new List<FullsStockLine>();
        public decimal QTY { get; set; }
        public decimal GROSS_WEIGHT { get; set; }
        public decimal NET_WEIGHT { get; set; }
    }

    public class FullsStockLine
    {
        public string UNIT { get; set; }
        public string TM { get; set; }
        public string MRP { get; set; }
        public string MFGDT { get; set; }
        public string BATCHNO { get; set; }
        public decimal QTY { get; set; }
        public decimal GROSS_WEIGHT { get; set; }
        public decimal NET_WEIGHT { get; set; }
        public string IDOD { get; set; }
        public string DOD { get; set; }
        public string BBD { get; set; }
        public int AGE { get; set; }
        public string STATUS { get; set; }
    }
}
