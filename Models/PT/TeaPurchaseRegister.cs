using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;

namespace PacketTea.Models.PT
{
    // Crystal data source for ~/CrystalReport/TeaPurchase.rpt (Details), TeaPurchasePartyWise.rpt and
    // TeaPurchasePartyWiseSummary.rpt: table "Command", one row per line of the API's
    // TeaPurchaseRegisterController. One table with the fields of the three layouts.
    public static class TeaPurchaseCrystal
    {
        public const string TableName = "Command";

        private static DateTime? Date(string s) =>
            DateTime.TryParseExact(s ?? "", "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : (DateTime?)null;

        public static DataSet Build(TeaPurchaseRegisterResult r)
        {
            var dt = new DataTable(TableName);
            foreach (var n in new[] { "PUR_TYPE", "SALESTYPE", "SALECENTRE", "GARDEN", "BILLNO", "DOCNO", "SHT_NAME", "SEASON", "INVNO", "SL_NO",
                                      "PCODE", "PARTY", "GRADE_NAME", "PARTY_BILLNO" })
                dt.Columns.Add(n, typeof(string));
            dt.Columns.Add("BILLDT", typeof(DateTime));
            dt.Columns.Add("DOCDT", typeof(DateTime));
            foreach (var n in new[] { "BAG_CHEST", "QTY", "GROSS", "TARE", "NETT", "TOT_QTY", "TOT_ROUNDOFF", "TOT_ROUNDED", "ROUNDOFF", "RATE",
                                      "AMOUNT", "IGST_AMT", "CGST_AMT", "SGST_AMT", "MISC_CHGS", "GST_AMT", "TOT_AMT" })
                dt.Columns.Add(n, typeof(decimal));
            dt.Columns.Add("ROWTYPE", typeof(int));

            var rows = r.Rows ?? new List<TeaPurchaseRow>();
            decimal totQty = 0;
            foreach (var x in rows) totQty += x.QTY;
            foreach (var x in rows)
            {
                var row = dt.NewRow();
                row["PUR_TYPE"] = x.PUR_TYPE ?? ""; row["SALESTYPE"] = x.SALESTYPE ?? ""; row["SALECENTRE"] = x.SALECENTRE ?? "";
                row["GARDEN"] = x.GARDEN ?? ""; row["BILLNO"] = x.BILLNO ?? ""; row["DOCNO"] = x.DOCNO ?? ""; row["SHT_NAME"] = "";
                row["SEASON"] = ""; row["INVNO"] = x.INVNO ?? ""; row["SL_NO"] = x.SL_NO ?? ""; row["PCODE"] = x.PCODE ?? "";
                row["PARTY"] = x.PARTY ?? ""; row["GRADE_NAME"] = x.GRADE_NAME ?? ""; row["PARTY_BILLNO"] = x.PARTY_BILLNO ?? "";
                row["BILLDT"] = (object)Date(x.BILLDT) ?? DBNull.Value;
                row["DOCDT"] = (object)Date(x.DOCDT) ?? DBNull.Value;
                row["BAG_CHEST"] = x.BAG_CHEST; row["QTY"] = x.QTY; row["GROSS"] = x.GROSS; row["TARE"] = x.TARE; row["NETT"] = x.NETT;
                row["TOT_QTY"] = totQty; row["TOT_ROUNDOFF"] = r.TotalRound; row["TOT_ROUNDED"] = r.TotalRound; row["ROUNDOFF"] = x.ROUNDOFF;
                row["RATE"] = x.RATE; row["AMOUNT"] = x.AMOUNT; row["IGST_AMT"] = x.IGST_AMT; row["CGST_AMT"] = x.CGST_AMT;
                row["SGST_AMT"] = x.SGST_AMT; row["MISC_CHGS"] = x.MISC_CHGS; row["GST_AMT"] = x.GST_AMT; row["TOT_AMT"] = x.TOT_AMT;
                row["ROWTYPE"] = x.ROWTYPE;
                dt.Rows.Add(row);
            }
            var ds = new DataSet("ReportDataSet");
            ds.Tables.Add(dt);
            return ds;
        }
    }

    // Mirrors of the API's TeaPurchaseRegisterController DTOs. Keep every property here: a field missing
    // on this side is silently dropped.
    public class TeaPurchasePick
    {
        public string CODE { get; set; }
        public string DESCN { get; set; }
    }

    public class TeaPurchaseLookups
    {
        public List<TeaPurchasePick> PurTypes { get; set; } = new List<TeaPurchasePick>();
        public List<TeaPurchasePick> Locations { get; set; } = new List<TeaPurchasePick>();
        public List<TeaPurchasePick> Documents { get; set; } = new List<TeaPurchasePick>();
    }

    public class TeaPurchaseRegisterResult
    {
        public string Company { get; set; }
        public string CompanyV { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string Units { get; set; }
        public string PurTypeText { get; set; }
        public decimal TotalRound { get; set; }
        public List<TeaPurchaseRow> Rows { get; set; } = new List<TeaPurchaseRow>();
    }

    public class TeaPurchaseRow
    {
        public string TYPE { get; set; }
        public string GARDEN { get; set; }
        public string UNIT { get; set; }
        public string DOCNO { get; set; }
        public string DOCDT { get; set; }
        public string BILLNO { get; set; }
        public string BILLDT { get; set; }
        public string PCODE { get; set; }
        public string PARTY { get; set; }
        public string PARTY_BILLNO { get; set; }
        public string SL_NO { get; set; }
        public string GRADE_NAME { get; set; }
        public string INVNO { get; set; }
        public string MARK { get; set; }
        public string SALECENTRE { get; set; }
        public string PUR_TYPE { get; set; }
        public string SALESTYPE { get; set; }
        public string ALLOCATION { get; set; }
        public string QTY_RETURN { get; set; }
        public string ORGANIC_TEA_DESCN { get; set; }
        public decimal BAG_CHEST { get; set; }
        public decimal QTY { get; set; }
        public decimal RATE { get; set; }
        public decimal AMOUNT { get; set; }
        public decimal DISCOUNT { get; set; }
        public decimal CGST_AMT { get; set; }
        public decimal SGST_AMT { get; set; }
        public decimal IGST_AMT { get; set; }
        public decimal GST_AMT { get; set; }
        public decimal TCS_AMT { get; set; }
        public decimal ROUNDOFF { get; set; }
        public decimal TOT_AMT { get; set; }
        public decimal TARE { get; set; }
        public decimal GROSS { get; set; }
        public decimal NETT { get; set; }
        public decimal MISC_CHGS { get; set; }
        public int ROWTYPE { get; set; }
    }
}
