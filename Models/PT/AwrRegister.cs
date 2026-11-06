using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;

namespace PacketTea.Models.PT
{
    // Crystal data source for ~/CrystalReport/AWRRegister.rpt (Garden Wise) and AWRRegisterSellerWise.rpt:
    // table "Command", one row per AWR line of the API's AwrRegisterController. One table with the
    // fields of both layouts (the Seller Wise one adds SELLER and SR_QTY_1).
    public static class AwrRegisterCrystal
    {
        public const string TableName = "Command";

        private static DateTime? Date(string s) =>
            DateTime.TryParseExact(s ?? "", "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : (DateTime?)null;

        public static DataSet Build(AwrRegisterResult r)
        {
            var dt = new DataTable(TableName);
            foreach (var n in new[] { "AWR_NO", "INVNO", "MARK", "MDESC", "GRADE", "REC_FROM" }) dt.Columns.Add(n, typeof(string));
            dt.Columns.Add("AWR_DATE", typeof(DateTime));
            foreach (var n in new[] { "BAG_CHEST", "BAG_RCVD", "TGROSS_WT", "NET_WT", "TNET_WT", "SR_KGS" }) dt.Columns.Add(n, typeof(decimal));
            dt.Columns.Add("SR_QTY", typeof(string));
            dt.Columns.Add("SR_QTY_1", typeof(decimal));
            foreach (var n in new[] { "OS_QTY", "TNO", "CONS_NO1" }) dt.Columns.Add(n, typeof(string));
            dt.Columns.Add("CONS_DT1", typeof(DateTime));
            foreach (var n in new[] { "LOC", "PARTY", "TRANSNAME", "REC_NAME" }) dt.Columns.Add(n, typeof(string));
            dt.Columns.Add("BAG_NO", typeof(decimal));
            foreach (var n in new[] { "REMARK", "OWNER_PCD", "NAME", "WAREHOUSE", "DESTI", "ADD1", "ADD2", "SELLER" }) dt.Columns.Add(n, typeof(string));

            foreach (var x in r.Rows ?? new List<AwrRegisterRow>())
            {
                var row = dt.NewRow();
                row["AWR_NO"] = x.AWR_NO ?? ""; row["INVNO"] = x.INVNO ?? ""; row["MARK"] = x.MARK ?? ""; row["MDESC"] = x.MDESC ?? "";
                row["GRADE"] = x.GRADE ?? ""; row["REC_FROM"] = x.REC_FROM ?? "";
                row["AWR_DATE"] = (object)Date(x.AWR_DATE) ?? DBNull.Value;
                row["BAG_CHEST"] = x.BAG_CHEST; row["BAG_RCVD"] = x.BAG_RCVD; row["TGROSS_WT"] = x.GROSS_WT; row["NET_WT"] = x.NET_WT;
                row["TNET_WT"] = x.TNET_WT; row["SR_KGS"] = x.SR_KGS;
                row["SR_QTY"] = x.SR_QTY ?? ""; row["SR_QTY_1"] = x.SR_QTY_1; row["OS_QTY"] = "";
                row["TNO"] = x.TNO ?? ""; row["CONS_NO1"] = x.CONS_NO1 ?? "";
                row["CONS_DT1"] = (object)Date(x.CONS_DT1) ?? DBNull.Value;
                row["LOC"] = x.LOC ?? ""; row["PARTY"] = x.PARTY ?? ""; row["TRANSNAME"] = x.TransName ?? ""; row["REC_NAME"] = x.REC_NAME ?? "";
                row["BAG_NO"] = x.BAG_NO; row["REMARK"] = x.REMARK ?? ""; row["OWNER_PCD"] = x.OWNER_PCD ?? ""; row["NAME"] = x.NAME ?? "";
                row["WAREHOUSE"] = x.WAREHOUSE ?? ""; row["DESTI"] = x.DESTI ?? ""; row["ADD1"] = x.ADD1 ?? ""; row["ADD2"] = x.ADD2 ?? "";
                row["SELLER"] = x.SELLER ?? "";
                dt.Rows.Add(row);
            }
            var ds = new DataSet("ReportDataSet");
            ds.Tables.Add(dt);
            return ds;
        }
    }

    // Mirrors of the API's AwrRegisterController DTOs. Keep every property here: a field missing on
    // this side is silently dropped.
    public class AwrRegisterPick
    {
        public string CODE { get; set; }
        public string DESCN { get; set; }
    }

    public class AwrRegisterLookups
    {
        public List<AwrRegisterPick> Gardens { get; set; } = new List<AwrRegisterPick>();
        public List<AwrRegisterPick> Warehouses { get; set; } = new List<AwrRegisterPick>();
        public List<AwrRegisterPick> Sellers { get; set; } = new List<AwrRegisterPick>();
    }

    public class AwrRegisterResult
    {
        public string Company { get; set; }
        public string CompanyV { get; set; }
        public string Address { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string Units { get; set; }
        public List<AwrRegisterRow> Rows { get; set; } = new List<AwrRegisterRow>();
    }

    public class AwrRegisterRow
    {
        public string AWR_NO { get; set; }
        public string AWR_DATE { get; set; }
        public string INVNO { get; set; }
        public string MARK { get; set; }
        public string MDESC { get; set; }
        public string GRADE { get; set; }
        public string CATEGORY { get; set; }
        public string REC_FROM { get; set; }
        public decimal BAG_CHEST { get; set; }
        public decimal BAG_RCVD { get; set; }
        public decimal TGROSS_WT { get; set; }
        public decimal NET_WT { get; set; }
        public decimal TNET_WT { get; set; }
        public decimal GROSS_WT { get; set; }
        public decimal TARE { get; set; }
        public decimal SR_KGS { get; set; }
        public string SR_QTY { get; set; }
        public decimal SR_QTY_1 { get; set; }
        public string SAMPLE_QTY { get; set; }
        public decimal SHORTAGE_KGS { get; set; }
        public string TNO { get; set; }
        public string CONS_NO1 { get; set; }
        public string CONS_DT1 { get; set; }
        public string LOC { get; set; }
        public string PARTY { get; set; }
        public string TransName { get; set; }
        public string REC_NAME { get; set; }
        public decimal BAG_NO { get; set; }
        public string REMARK { get; set; }
        public string OWNER_PCD { get; set; }
        public string NAME { get; set; }
        public string WAREHOUSE { get; set; }
        public string DESTI { get; set; }
        public string ADD1 { get; set; }
        public string ADD2 { get; set; }
        public string SELLER { get; set; }
        public string BROK_NAME { get; set; }
        public string WAY_BILL { get; set; }
        public string GP_NO { get; set; }
        public string GP_DATE { get; set; }
        public decimal CHESTSLF { get; set; }
        public decimal CHESTSLT { get; set; }
    }
}
