using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;

namespace PacketTea.Models.PT
{
    // Crystal data source for ~/CrystalReport/ArrivalNWeightment.rpt (Classic ERP's AWR Printing
    // layout): table "Command", one row per print line of the API's AwrPrintingController.
    public static class AwrPrintingCrystal
    {
        public const string TableName = "Command";

        private static readonly string[] Texts =
        {
            "AWR_NO", "GARDEN_SERIAL", "INVNO", "MARK", "GRADE", "REC_FROM", "REG_ADDR", "AWR_REMARK", "TRANSNAME",
            "WS_QTY", "BR_QTY", "TR_QTY", "OS_QTY", "SR_QTY", "TRAN_TYPE", "TPT", "TNO", "WAY_BILL", "DESP_NO", "CONS_NO1",
            "LOC", "ADD1", "ADD2", "CONTACT_NO", "NAME", "REC_NAME", "BROK_NAME", "PH_NO", "REMARK", "SINGLELINE", "FLAG"
        };

        private static DateTime? Date(string s) =>
            DateTime.TryParseExact(s ?? "", "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : (DateTime?)null;

        public static DataSet Build(AwrPrintingResult r)
        {
            var dt = new DataTable(TableName);
            foreach (var n in new[] { "AWR_NO", "GARDEN_SERIAL", "INVNO", "MARK", "GRADE", "REC_FROM", "REG_ADDR", "AWR_REMARK", "TRANSNAME" })
                dt.Columns.Add(n, typeof(string));
            dt.Columns.Add("AWR_DATE", typeof(DateTime));
            dt.Columns.Add("AMEND_DT", typeof(DateTime));
            foreach (var n in new[] { "CHESTSLF", "CHESTSLT", "BAG_CHEST", "BAG_RCVD", "TGROSS_WT", "NET_WT", "TNET_WT", "SHORT_EXCESS", "TARE" })
                dt.Columns.Add(n, typeof(decimal));
            foreach (var n in new[] { "WS_QTY", "BR_QTY", "TR_QTY", "OS_QTY", "SR_QTY" })
                dt.Columns.Add(n, typeof(string));
            dt.Columns.Add("SR_KGS", typeof(decimal));
            foreach (var n in new[] { "TRAN_TYPE", "TPT", "TNO", "WAY_BILL", "DESP_NO", "CONS_NO1" })
                dt.Columns.Add(n, typeof(string));
            dt.Columns.Add("CONS_DT1", typeof(DateTime));
            foreach (var n in new[] { "LOC", "ADD1", "ADD2", "CONTACT_NO", "NAME", "REC_NAME", "BROK_NAME", "PH_NO" })
                dt.Columns.Add(n, typeof(string));
            dt.Columns.Add("BAG_NO", typeof(decimal));
            foreach (var n in new[] { "REMARK", "SINGLELINE", "FLAG" })
                dt.Columns.Add(n, typeof(string));

            foreach (var x in r.Rows ?? new List<AwrPrintingRow>())
            {
                var row = dt.NewRow();
                row["AWR_NO"] = x.AWR_NO ?? ""; row["GARDEN_SERIAL"] = x.GARDEN_SERIAL ?? ""; row["INVNO"] = x.INVNO ?? "";
                row["MARK"] = x.MARK ?? ""; row["GRADE"] = x.GRADE ?? ""; row["REC_FROM"] = x.REC_FROM ?? "";
                row["REG_ADDR"] = x.REG_ADDR ?? ""; row["AWR_REMARK"] = x.AWR_REMARK ?? ""; row["TRANSNAME"] = x.TransName ?? "";
                row["AWR_DATE"] = (object)Date(x.AWR_DATE) ?? DBNull.Value;
                row["AMEND_DT"] = (object)Date(x.AMEND_DT) ?? DBNull.Value;
                row["CHESTSLF"] = x.CHESTSLF; row["CHESTSLT"] = x.CHESTSLT; row["BAG_CHEST"] = x.BAG_CHEST; row["BAG_RCVD"] = x.BAG_RCVD;
                row["TGROSS_WT"] = x.TGROSS_WT; row["NET_WT"] = x.NET_WT; row["TNET_WT"] = x.TNET_WT;
                row["SHORT_EXCESS"] = x.SHORT_EXCESS; row["TARE"] = x.TARE;
                row["WS_QTY"] = x.WS_QTY ?? ""; row["BR_QTY"] = x.BR_QTY ?? ""; row["TR_QTY"] = x.TR_QTY ?? "";
                row["OS_QTY"] = x.OS_QTY ?? ""; row["SR_QTY"] = x.SR_QTY ?? ""; row["SR_KGS"] = x.SR_KGS;
                row["TRAN_TYPE"] = x.TRAN_TYPE ?? ""; row["TPT"] = ""; row["TNO"] = x.TNO ?? ""; row["WAY_BILL"] = x.WAY_BILL ?? "";
                row["DESP_NO"] = x.DESP_NO ?? ""; row["CONS_NO1"] = x.CONS_NO1 ?? "";
                row["CONS_DT1"] = (object)Date(x.CONS_DT1) ?? DBNull.Value;
                row["LOC"] = x.LOC ?? ""; row["ADD1"] = x.ADD1 ?? ""; row["ADD2"] = x.ADD2 ?? ""; row["CONTACT_NO"] = x.CONTACT_NO ?? "";
                row["NAME"] = x.NAME ?? ""; row["REC_NAME"] = x.REC_NAME ?? ""; row["BROK_NAME"] = x.BROK_NAME ?? ""; row["PH_NO"] = x.PH_NO ?? "";
                row["BAG_NO"] = x.BAG_NO;
                row["REMARK"] = x.REMARK ?? ""; row["SINGLELINE"] = x.SINGLELINE ?? ""; row["FLAG"] = x.FLAG ?? "";
                dt.Rows.Add(row);
            }
            var ds = new DataSet("ReportDataSet");
            ds.Tables.Add(dt);
            return ds;
        }
    }

    // Mirrors of the API's AwrPrintingController DTOs. Keep every property here: a field missing on
    // this side is silently dropped.
    public class AwrUnit
    {
        public string CODE { get; set; }
        public string NAME { get; set; }
    }

    public class AwrNoItem
    {
        public string CODE { get; set; }
        public string DESCN { get; set; }
    }

    public class AwrPrintingResult
    {
        public string Company { get; set; }
        public string CompanyV { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string AwrFrom { get; set; }
        public string AwrTo { get; set; }
        public string Units { get; set; }
        public List<AwrPrintingRow> Rows { get; set; } = new List<AwrPrintingRow>();
    }

    public class AwrPrintingRow
    {
        public string DOCNO { get; set; }
        public string AWR_NO { get; set; }
        public string AWR_DATE { get; set; }
        public string AMEND_DT { get; set; }
        public string GARDEN_SERIAL { get; set; }
        public string INVNO { get; set; }
        public string MARK { get; set; }
        public string GRADE { get; set; }
        public string GRADE_CODE { get; set; }
        public string PACK_DATE { get; set; }
        public string REC_FROM { get; set; }
        public decimal CHESTSLF { get; set; }
        public decimal CHESTSLT { get; set; }
        public decimal BAG_CHEST { get; set; }
        public decimal BAG_RCVD { get; set; }
        public string REG_ADDR { get; set; }
        public string AWR_REMARK { get; set; }
        public string TransName { get; set; }
        public decimal TGROSS_WT { get; set; }
        public decimal NET_WT { get; set; }
        public decimal TNET_WT { get; set; }
        public decimal SHORT_EXCESS { get; set; }
        public decimal TARE { get; set; }
        public string WS_QTY { get; set; }
        public string BR_QTY { get; set; }
        public string TR_QTY { get; set; }
        public string OS_QTY { get; set; }
        public string SR_QTY { get; set; }
        public decimal SR_KGS { get; set; }
        public decimal D_SR_KGS { get; set; }
        public string TRAN_TYPE { get; set; }
        public string TNO { get; set; }
        public string WAY_BILL { get; set; }
        public string DESP_NO { get; set; }
        public string CONS_NO1 { get; set; }
        public string CONS_DT1 { get; set; }
        public string LOC { get; set; }
        public string ADD1 { get; set; }
        public string ADD2 { get; set; }
        public string CONTACT_NO { get; set; }
        public string NAME { get; set; }
        public string REC_NAME { get; set; }
        public string BROK_NAME { get; set; }
        public string EMAILID { get; set; }
        public string PH_NO { get; set; }
        public decimal BAG_NO { get; set; }
        public string REMARK { get; set; }
        public string Damage_Detail { get; set; }
        public string SINGLELINE { get; set; }
        public string FLAG { get; set; }
    }
}
