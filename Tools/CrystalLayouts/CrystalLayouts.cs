using System;
using System.Data;
using System.IO;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.ReportAppServer.ClientDoc;
using CrystalDecisions.ReportAppServer.Controllers;
using CrystalDecisions.ReportAppServer.DataDefModel;
using CrystalDecisions.ReportAppServer.DataSetConversion;
using CrystalDecisions.ReportAppServer.ReportDefModel;
using RD = CrystalDecisions.ReportAppServer.ReportDefModel;
using ReportDefinition = CrystalDecisions.ReportAppServer.ReportDefModel.ReportDefinition;
using ObjectFormat = CrystalDecisions.ReportAppServer.ReportDefModel.ObjectFormat;
using Section = CrystalDecisions.ReportAppServer.ReportDefModel.Section;
using ReportDocument = CrystalDecisions.CrystalReports.Engine.ReportDocument;
using DataSet = System.Data.DataSet;

// Generates CrystalReport/FullsStockActual.rpt (A4 portrait) and CrystalReport/StockReport.rpt
// (A4 landscape) from their .xsd schemas through the Crystal RAS report-modification API,
// then test-renders each to PDF with sample rows. Not part of the web project build.
//
// Needs the 64-bit Crystal Reports runtime (13.0.x). From this folder:
//   powershell -File build.ps1 -Source CrystalLayouts.cs -Out CrystalLayouts.exe
//   CrystalLayouts.exe ..\..\CrystalReport <folder for the test PDFs>
// Re-run after changing the .xsd files (regenerated from FullsStockCrystal /
// StockReportCrystal), or fine-tune the generated .rpt in the Visual Studio designer.
class Gen
{
    readonly ISCDReportClientDocument rcd;
    readonly string table;
    const string FontName = "Arial Narrow";
    public decimal FontSize = 8m;

    Gen(ISCDReportClientDocument rcd, string table) { this.rcd = rcd; this.table = table; }

    ReportDefinition Def { get { return rcd.ReportDefController.ReportDefinition; } }
    string F(string col) { return "{" + table + "." + col + "}"; }

    ISCRField Field(string formulaForm)
    {
        var f = rcd.DataDefController.FindFieldByFormulaForm(formulaForm);
        if (f == null) throw new Exception("Field not found: " + formulaForm);
        return f;
    }

    void Formula(string name, string text, CrFieldValueTypeEnum type)
    {
        var ff = new FormulaFieldClass { Name = name, Text = text, Syntax = CrFormulaSyntaxEnum.crFormulaSyntaxCrystal, Type = type };
        rcd.DataDefController.FormulaFieldController.Add(ff);
    }

    RD.FontColor Font(bool bold, decimal? size = null)
    {
        var fc = new FontColorClass();
        fc.Font = new FontClass { Name = FontName, Size = size ?? FontSize, Bold = bold };
        fc.Color = 0; // black
        return fc;
    }

    static ObjectFormat Fmt(CrAlignmentEnum align)
    {
        var of = new ObjectFormatClass();
        of.HorizontalAlignment = align;
        of.EnableCanGrow = false;
        return of;
    }

    void Height(Section s, int twips)
    {
        rcd.ReportDefController.ReportSectionController.SetProperty(s, CrReportSectionPropertyEnum.crReportSectionPropertyHeight, twips);
    }

    void Suppress(Section s, string condition = null)
    {
        var fmt = s.Format.Clone(true);
        if (condition == null) fmt.EnableSuppress = true;
        else
        {
            var cf = new ConditionFormulaClass { Syntax = CrFormulaSyntaxEnum.crFormulaSyntaxCrystal, Text = condition };
            fmt.ConditionFormulas[CrSectionAreaFormatConditionFormulaTypeEnum.crSectionAreaConditionFormulaTypeEnableSuppress] = cf;
        }
        rcd.ReportDefController.ReportSectionController.SetProperty(s, CrReportSectionPropertyEnum.crReportSectionPropertyFormat, fmt);
    }

    // formulaForm: "{Table.COL}" or "{@NAME}"; decimals < 0 = not numeric.
    void AddField(Section s, string formulaForm, int left, int top, int width, CrAlignmentEnum align,
                  bool bold = false, int decimals = -1, bool suppressZero = false, string styleCond = null, bool redNegative = false)
    {
        var src = Field(formulaForm);
        var fo = new FieldObjectClass
        {
            DataSourceName = src.FormulaForm,
            FieldValueType = src.Type,
            Left = left, Top = top, Width = width, Height = (int)(FontSize * 26m) + 10,
        };
        fo.Format = Fmt(align);
        var fc = Font(bold);
        if (styleCond != null)
            fc.ConditionFormulas[CrFontColorConditionFormulaTypeEnum.crFontColorConditionFormulaTypeStyle] =
                new ConditionFormulaClass { Syntax = CrFormulaSyntaxEnum.crFormulaSyntaxCrystal, Text = styleCond };
        if (redNegative)
            fc.ConditionFormulas[CrFontColorConditionFormulaTypeEnum.crFontColorConditionFormulaTypeColor] =
                new ConditionFormulaClass { Syntax = CrFormulaSyntaxEnum.crFormulaSyntaxCrystal, Text = "if CurrentFieldValue < 0 then crRed else crBlack" };
        fo.FontColor = fc;
        if (decimals >= 0)
        {
            var ff = new FieldFormatClass();
            ff.NumericFormat = new NumericFieldFormatClass
            {
                NDecimalPlaces = decimals,
                RoundingFormat = decimals == 0 ? CrRoundingTypeEnum.crRoundingTypeRoundToUnit : CrRoundingTypeEnum.crRoundingTypeRoundToHundredth,
                ThousandsSeparator = false,
                EnableSuppressIfZero = suppressZero,
                NegativeFormat = CrNegativeTypeEnum.crNegativeTypeLeadingMinus,
                DecimalSymbol = ".",
                ThousandSymbol = ",",
                CurrencySymbol = "",
                EnableUseLeadZero = true,
            };
            fo.FieldFormat = ff;
        }
        rcd.ReportDefController.ReportObjectController.Add(fo, s, -1);
    }

    void AddText(Section s, string text, int left, int top, int width, CrAlignmentEnum align, bool bold = false, decimal? size = null)
    {
        var to = new TextObjectClass
        {
            Left = left, Top = top, Width = width, Height = (int)((size ?? FontSize) * 26m) + 10,
        };
        to.Format = Fmt(align);
        to.FontColor = Font(bold, size);
        var p = new ParagraphClass();
        p.Alignment = align;
        var el = new ParagraphTextElementClass { Text = text };
        el.FontColor = Font(bold, size);
        p.ParagraphElements.Add(el);
        to.Paragraphs.Add(p);
        rcd.ReportDefController.ReportObjectController.Add(to, s, -1);
    }

    void AddLine(Section s, int left, int right, int top)
    {
        var lo = new LineObjectClass
        {
            Left = left, Right = right, Top = top, Bottom = top,
            LineStyle = CrLineStyleEnum.crLineStyleSingle, LineThickness = 10,
            SectionName = s.Name, EndSectionName = s.Name, LineColor = 0,
        };
        rcd.ReportDefController.ReportObjectController.Add(lo, s, -1);
    }

    void Paper(bool landscape)
    {
        // A4, fixed in the .rpt rather than taken from whatever printer the server has.
        var po = rcd.PrintOutputController;
        var opts = po.GetPrintOptions();
        opts.DissociatePageSizeAndPrinterPaperSize = true;
        opts.PaperSize = CrPaperSizeEnum.crPaperSizePaperA4;
        opts.PaperOrientation = CrPaperOrientationEnum.crPaperOrientationPortrait;
        po.ModifyPrintOptions(opts);
        // Explicit A4 page in twips so the layout never falls back to a printer's Letter page.
        // ModifyUserPaperSize takes (height, width) in twips.
        if (landscape) po.ModifyUserPaperSize(11906, 16838); else po.ModifyUserPaperSize(16838, 11906);
        po.ModifyPageMargins(360, 360, 360, 360);
        var check = po.GetPrintOptions();
        Console.WriteLine("  page " + check.PaperOrientation + " " + check.PaperSize + " content " + check.PageContentWidth + "x" + check.PageContentHeight);
    }

    static Gen Start(string xsd, string tableName, out ReportDocument holder)
    {
        holder = new ReportDocument();
        var rcd = holder.ReportClientDocument;
        rcd.New();
        var ds = new DataSet();
        ds.ReadXmlSchema(xsd);
        rcd.DatabaseController.AddDataSource(DataSetConverter.Convert(ds));
        return new Gen(rcd, tableName);
    }

    void Save(string path)
    {
        object dir = Path.GetDirectoryName(path);
        if (File.Exists(path)) File.Delete(path);
        rcd.SaveAs(Path.GetFileName(path), ref dir, 0);
    }

    void CommonFormulas(params string[] names)
    {
        foreach (var n in names) Formula(n, "\"\"", CrFieldValueTypeEnum.crFieldValueTypeStringField);
        Formula("PAGE_NO", "\"Page: \" + ToText(PageNumber, 0)", CrFieldValueTypeEnum.crFieldValueTypeStringField);
    }

    // ------------------------------------------------------------------ Fulls Stock Actual
    static void BuildFulls(string dir)
    {
        ReportDocument holder;
        var g = Start(Path.Combine(dir, "FullsStockActual.xsd"), "FullsStockRpt", out holder);
        g.Paper(false);
        g.CommonFormulas("HEADER1", "HEADER2", "TITLE", "UNIT_TEXT", "RUN_INFO");
        g.Formula("ITCD_SHOW", "if {FullsStockRpt.FIRST_IN_ITEM} then {FullsStockRpt.ITCD} else \"\"", CrFieldValueTypeEnum.crFieldValueTypeStringField);
        g.Formula("DESC_SHOW", "if {FullsStockRpt.FIRST_IN_ITEM} then {FullsStockRpt.DESCRIPTION} else \"\"", CrFieldValueTypeEnum.crFieldValueTypeStringField);

        // Group on ITEM_SEQ keeps the API order (Item Code / Name / Custom); rows by SEQ.
        var grp = new GroupClass { ConditionField = g.Field("{FullsStockRpt.ITEM_SEQ}") };
        g.rcd.DataDefController.GroupController.Add(-1, grp);
        g.rcd.DataDefController.SortController.Add(-1, new SortClass { SortField = g.Field("{FullsStockRpt.SEQ}"), Direction = CrSortDirectionEnum.crSortDirectionAscendingOrder });

        var L = CrAlignmentEnum.crAlignmentLeft; var R = CrAlignmentEnum.crAlignmentRight;
        int[] x = { 0, 680, 3160, 3670, 4600, 5630, 6560, 7400, 8420, 9330, 10200, 10640 };
        int[] w = { 650, 2450, 480, 900, 1000, 900, 800, 800, 880, 880, 400, 420 };
        const int right = 11060, rh = 230;

        var def = g.Def;
        g.Suppress(def.ReportHeaderArea.Sections[0]);

        var ph = def.PageHeaderArea.Sections[0];
        g.Height(ph, 1150);
        g.AddField(ph, "{@HEADER1}", 0, 0, 8000, L);
        g.AddField(ph, "{@HEADER2}", 0, rh, 8000, L);
        g.AddField(ph, "{@PAGE_NO}", right - 1500, rh, 1500, R);
        g.AddField(ph, "{@TITLE}", 0, 2 * rh, 8000, L);
        g.AddField(ph, "{@UNIT_TEXT}", right - 2500, 2 * rh, 2500, R);
        g.AddLine(ph, 0, right, 3 * rh + 40);
        int hy = 3 * rh + 110;
        string[] heads = { "ITCD", "DESCRIPTION", "MRP", "MFG. DATE", "BATCH NO", "QUANTITY", "Gross Wt", "Net Wt", "DOD", "BBD", "Age", "" };
        for (int i = 0; i < heads.Length; i++)
            g.AddText(ph, heads[i], x[i], hy, w[i], (i >= 5 && i <= 7) || i == 10 ? R : L);
        g.AddLine(ph, 0, right, hy + rh + 50);

        g.Suppress(def.get_GroupHeaderArea(0).Sections[0]);

        var d = def.DetailArea.Sections[0];
        g.Height(d, rh + 10);
        g.AddField(d, "{@ITCD_SHOW}", x[0], 0, w[0], L);
        g.AddField(d, "{@DESC_SHOW}", x[1], 0, w[1], L);
        g.AddField(d, g.F("MRP"), x[2], 0, w[2], L);
        g.AddField(d, g.F("MFGDT"), x[3], 0, w[3], L);
        g.AddField(d, g.F("BATCHNO"), x[4], 0, w[4], L);
        g.AddField(d, g.F("QTY"), x[5], 0, w[5], R, decimals: 2);
        g.AddField(d, g.F("GROSS_WEIGHT"), x[6], 0, w[6], R, decimals: 2);
        g.AddField(d, g.F("NET_WEIGHT"), x[7], 0, w[7], R, decimals: 2);
        g.AddField(d, g.F("DOD"), x[8], 0, w[8], L);
        g.AddField(d, g.F("BBD"), x[9], 0, w[9], L);
        g.AddField(d, g.F("AGE"), x[10], 0, w[10], R, decimals: 0);
        g.AddField(d, g.F("STATUS"), x[11], 0, w[11], L);

        var gf = def.get_GroupFooterArea(0).Sections[0];
        g.Height(gf, 2 * rh + 150);
        g.AddLine(gf, 0, x[10] + w[10], 40);
        g.AddText(gf, "Total", x[1] + 500, 90, 1500, L);
        g.AddField(gf, g.F("ITEM_QTY"), x[5], 90, w[5], R, decimals: 2);
        g.AddField(gf, g.F("ITEM_GROSS_WEIGHT"), x[6], 90, w[6], R, decimals: 2);
        g.AddField(gf, g.F("ITEM_NET_WEIGHT"), x[7], 90, w[7], R, decimals: 2);
        g.AddLine(gf, 0, x[10] + w[10], rh + 150);

        var rf = def.ReportFooterArea.Sections[0];
        g.Height(rf, 5 * rh + 200);
        g.AddText(rf, "Grand Total", x[1] + 500, 60, 1500, L, bold: true);
        g.AddField(rf, g.F("GRAND_QTY"), x[5], 60, w[5], R, bold: true, decimals: 2);
        g.AddField(rf, g.F("GRAND_GROSS_WEIGHT"), x[6], 60, w[6], R, bold: true, decimals: 2);
        g.AddField(rf, g.F("GRAND_NET_WEIGHT"), x[7], 60, w[7], R, bold: true, decimals: 2);
        g.AddLine(rf, 0, x[10] + w[10], rh + 110);
        g.AddText(rf, " ** -", x[0], 2 * rh, w[0], L);
        g.AddText(rf, "DOD Date over", x[1], 2 * rh, w[1], L);
        g.AddText(rf, "*** -", x[0], 3 * rh, w[0], L);
        g.AddText(rf, "BBD Date over", x[1], 3 * rh, w[1], L);
        g.AddField(rf, "{@RUN_INFO}", 0, 4 * rh + 60, 8000, L);

        g.Height(def.PageFooterArea.Sections[0], 200);

        g.Save(Path.Combine(dir, "FullsStockActual.rpt"));
        holder.Close();
        ShowPage(Path.Combine(dir, "FullsStockActual.rpt"));
    }

    // ------------------------------------------------------------------ Stock Report
    static void BuildStock(string dir)
    {
        ReportDocument holder;
        var g = Start(Path.Combine(dir, "StockReport.xsd"), "StockRpt", out holder);
        g.FontSize = 7.5m;
        g.Paper(true);
        g.CommonFormulas("HEADER1", "HEADER2", "TITLE", "UNIT_TEXT", "RUN_INFO", "SKIPPED");

        // Group on ROW_KIND ("D" item rows, then "T" grand totals); rows by SEQ.
        g.rcd.DataDefController.GroupController.Add(-1, new GroupClass { ConditionField = g.Field("{StockRpt.ROW_KIND}") });
        g.rcd.DataDefController.SortController.Add(-1, new SortClass { SortField = g.Field("{StockRpt.SEQ}"), Direction = CrSortDirectionEnum.crSortDirectionAscendingOrder });

        var L = CrAlignmentEnum.crAlignmentLeft; var R = CrAlignmentEnum.crAlignmentRight;
        const int right = 16100, rh = 215;
        int numLeft = 3700, nw = (right - numLeft) / 19;
        string[] cols = { "OP", "PROD", "PURCH", "MRETU", "TOTAL_RECT", "LOADOUT", "CHECKIN", "DIRECT_SALE", "INDIRECT_SALE",
                          "TRANS", "PURCH_RETU", "TOTAL_DESP", "ADJ", "SAMPLE", "BKG_LKG", "SHT", "TOTAL_OUT", "CL_STOCK", "GAIN_LOSS" };
        string[] h1 = { "Opening", "", "", "Market", "Total", "Load", "Check", "Direct", "", "", "Purch.", "Total", "", "", "Bkg./", "", "Total", "Closing", "Gain/" };
        string[] h2 = { "Balance", "Production", "Purchase", "Return", "Receipt", "Out", "In", "Sale", "Sale", "Transfers", "Return", "Despatch", "Adjust.", "Sample", "Lkg.", "Shortage", "Out", "Stock", "Loss" };

        var def = g.Def;
        g.Suppress(def.ReportHeaderArea.Sections[0]);

        var ph = def.PageHeaderArea.Sections[0];
        g.Height(ph, 1300);
        g.AddField(ph, "{@HEADER1}", 0, 0, 9000, L);
        g.AddField(ph, "{@HEADER2}", 0, rh, 9000, L);
        g.AddField(ph, "{@PAGE_NO}", right - 1500, rh, 1500, R);
        g.AddField(ph, "{@TITLE}", 0, 2 * rh, 7000, L);
        g.AddField(ph, "{@UNIT_TEXT}", 7100, 2 * rh, 6000, L);
        g.AddLine(ph, 0, right, 3 * rh + 40);
        int y1 = 3 * rh + 100, y2 = y1 + rh;
        g.AddText(ph, "Item", 0, y1, 600, L); g.AddText(ph, "Code", 0, y2, 600, L);
        g.AddText(ph, "Item", 620, y1, 2300, L); g.AddText(ph, "Description", 620, y2, 2300, L);
        g.AddText(ph, "Type", 2940, y2, 740, L);
        for (int i = 0; i < cols.Length; i++)
        {
            if (h1[i] != "") g.AddText(ph, h1[i], numLeft + i * nw, y1, nw, R);
            g.AddText(ph, h2[i], numLeft + i * nw, y2, nw, R);
        }
        g.AddLine(ph, 0, right, y2 + rh + 50);

        // Rule above the grand totals: group header shown only for ROW_KIND = "T".
        var gh = def.get_GroupHeaderArea(0).Sections[0];
        g.Height(gh, 120);
        g.AddLine(gh, 0, right, 50);
        g.Suppress(gh, "{StockRpt.ROW_KIND} <> \"T\"");
        g.Suppress(def.get_GroupFooterArea(0).Sections[0]);

        var bold = "if {StockRpt.ROW_KIND} = \"T\" then crBold else crRegular";
        var d = def.DetailArea.Sections[0];
        g.Height(d, rh + 10);
        g.AddField(d, g.F("ITCD"), 0, 0, 600, L, styleCond: bold);
        g.AddField(d, g.F("DESCN"), 620, 0, 2300, L, styleCond: bold);
        g.AddField(d, g.F("TYPE_TEXT"), 2940, 0, 740, L, styleCond: bold);
        for (int i = 0; i < cols.Length; i++)
            g.AddField(d, g.F(cols[i]), numLeft + i * nw, 0, nw, R, decimals: 2, suppressZero: true, styleCond: bold, redNegative: true);

        var rf = def.ReportFooterArea.Sections[0];
        g.Height(rf, 3 * rh + 120);
        g.AddLine(rf, 0, right, 40);
        g.AddField(rf, "{@SKIPPED}", 0, 110, 12000, L);
        g.AddField(rf, "{@RUN_INFO}", 0, rh + 130, 9000, L);

        g.Height(def.PageFooterArea.Sections[0], 200);

        g.Save(Path.Combine(dir, "StockReport.rpt"));
        holder.Close();
        ShowPage(Path.Combine(dir, "StockReport.rpt"));
    }

    static void ShowPage(string rpt)
    {
        var c = new ReportDocument();
        c.Load(rpt);
        Console.WriteLine("  engine page " + c.PrintOptions.PaperOrientation + " " + c.PrintOptions.PaperSize + " " + c.PrintOptions.PageContentWidth + "x" + c.PrintOptions.PageContentHeight);
        c.Close();
    }

    // ------------------------------------------------------------------ test render
    static void Render(string rpt, DataSet ds, string pdf, params string[] kv)
    {
        var r = new ReportDocument();
        r.Load(rpt);
        r.SetDataSource(ds);
        for (int i = 0; i + 1 < kv.Length; i += 2)
            foreach (FormulaFieldDefinition f in r.DataDefinition.FormulaFields)
                if (string.Equals(f.Name, kv[i], StringComparison.OrdinalIgnoreCase)) f.Text = "'" + kv[i + 1].Replace("'", "''") + "'";
        r.ExportToDisk(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat, pdf);
        r.Close();
    }

    static DataSet Sample(string xsd)
    {
        var ds = new DataSet();
        ds.ReadXmlSchema(xsd);
        return ds;
    }

    static void TestFulls(string dir, string outDir)
    {
        var ds = Sample(Path.Combine(dir, "FullsStockActual.xsd"));
        var t = ds.Tables[0];
        int seq = 0;
        void add(int item, bool first, string itcd, string name, string mrp, string mfg, string batch, decimal q, decimal gw, decimal nw,
                 string dod, string bbd, int age, string st, decimal iq, decimal ig, decimal inw)
            {
                var row = t.NewRow();
                row["SEQ"] = ++seq; row["ITEM_SEQ"] = item; row["FIRST_IN_ITEM"] = first; row["ITCD"] = itcd; row["DESCRIPTION"] = name;
                row["UNIT"] = "BGCH"; row["TM"] = "M"; row["MRP"] = mrp; row["MFGDT"] = mfg; row["BATCHNO"] = batch;
                row["QTY"] = q; row["GROSS_WEIGHT"] = gw; row["NET_WEIGHT"] = nw; row["IDOD"] = ""; row["DOD"] = dod; row["BBD"] = bbd;
                row["AGE"] = age; row["STATUS"] = st; row["ITEM_QTY"] = iq; row["ITEM_GROSS_WEIGHT"] = ig; row["ITEM_NET_WEIGHT"] = inw;
                row["GRAND_QTY"] = 535m; row["GRAND_GROSS_WEIGHT"] = -223.95m; row["GRAND_NET_WEIGHT"] = 150.52m;
                t.Rows.Add(row);
            }
        add(1, true, "018702", "PLANTERS RESERVE-(1500", "1500", "25/10/2024", "PR/GB/PTB1", 1, -5.40m, -2.55m, "02/02/2025", "13/05/2025", 697, "***", 1, -5.40m, -2.55m);
        add(2, true, "020007", "SUPER LEAF RS.5/-(10 5", "5", "14/07/2026", "PG34/1", 5, -50m, 50m, "15/07/2026", "08/04/2029", 70, "**", 5, -50m, 50m);
        add(3, true, "020011", "GREEN TEA 25 ETB 50", "160", "10/03/2026", "PTBG044", 29, -24.70m, 1.50m, "11/03/2026", "10/03/2027", 196, "**", 166, -32.08m, 9.07m);
        add(3, false, "020011", "GREEN TEA 25 ETB 50", "170", "17/07/2026", "PT/BG/019", 117, -8.38m, 6.57m, "18/07/2026", "17/07/2027", 67, "**", 166, -32.08m, 9.07m);
        add(3, false, "020011", "GREEN TEA 25 ETB 50", "160", "10/03/2026", "PTBG044", 20, 1m, 1m, "11/03/2026", "10/03/2027", 196, "**", 166, -32.08m, 9.07m);
        add(4, true, "020013", "ASSAM TEA 25 ETB 50", "100", "11/03/2026", "PTBG046", 60, -11.95m, 3m, "12/03/2026", "04/12/2028", 195, "**", 358, -27.05m, 17.90m);
        add(4, false, "020013", "ASSAM TEA 25 ETB 50", "120", "06/08/2026", "PT/BG/030", 298, -15.10m, 14.90m, "07/08/2026", "01/05/2029", 47, "**", 358, -27.05m, 17.90m);
        Render(Path.Combine(dir, "FullsStockActual.rpt"), ds, Path.Combine(outDir, "FullsStockActual-test.pdf"),
               "HEADER1", "JAY SHREE TEA & INDUSTRIES LTD.", "HEADER2", "10, CAMAC STREET,",
               "TITLE", "FULLS STOCK (Actual) CLOSING REPORT AS ON 22/09/2026", "UNIT_TEXT", "Unit: BGCH", "RUN_INFO", "User: TEST   Run: 22-Sep-2026 21:50");
    }

    static void TestStock(string dir, string outDir)
    {
        var ds = Sample(Path.Combine(dir, "StockReport.xsd"));
        var t = ds.Tables[0];
        int seq = 0;
        Action<string, string, string, string, string, decimal[]> add = (kind, itcd, name, typ, typeText, v) =>
        {
            var row = t.NewRow();
            row["SEQ"] = ++seq; row["ROW_KIND"] = kind; row["ITCD"] = itcd; row["DESCN"] = name; row["TYP"] = typ; row["TYPE_TEXT"] = typeText;
            string[] cols = { "OP", "PROD", "PURCH", "MRETU", "TOTAL_RECT", "LOADOUT", "CHECKIN", "DIRECT_SALE", "INDIRECT_SALE",
                              "TRANS", "PURCH_RETU", "TOTAL_DESP", "ADJ", "SAMPLE", "BKG_LKG", "SHT", "TOTAL_OUT", "CL_STOCK", "GAIN_LOSS" };
            for (int i = 0; i < cols.Length; i++) row[cols[i]] = i < v.Length ? v[i] : 0m;
            t.Rows.Add(row);
        };
        add("D", "020007", "SUPER LEAF RS.5/-(10 5", "A", "Packet/Bag", new[] { 10m, 20m, 0, 1, 31, 0, 0, 0, 26, 0, 0, 26, 0, 0, 0, 0, 26, 5, 0.35m });
        add("D", "", "", "B", "Net Wt.", new[] { 100m, 200, 0, 10, 310, 0, 0, 0, 260, 0, 0, 260, 0, 0, 0, 0, 260, 50, 0 });
        add("D", "020011", "GREEN TEA 25 ETB 50", "A", "Packet/Bag", new[] { 150m, 50, 0, 0, 200, 0, 0, 0, 34, 0, 0, 34, 0, 0, 0, 0, 34, 166, -1.2m });
        add("T", "", "Grand Total", "A", "Packet/Bag", new[] { 160m, 70, 0, 1, 231, 0, 0, 0, 60, 0, 0, 60, 0, 0, 0, 0, 60, 171, -0.85m });
        add("T", "", "", "B", "Net Wt.", new[] { 100m, 200, 0, 10, 310, 0, 0, 0, 260, 0, 0, 260, 0, 0, 0, 0, 260, 50, 0 });
        Render(Path.Combine(dir, "StockReport.rpt"), ds, Path.Combine(outDir, "StockReport-test.pdf"),
               "HEADER1", "JAY SHREE TEA & INDUSTRIES LTD.", "HEADER2", "10, CAMAC STREET,",
               "TITLE", "Stock Report for the period 01/04/2026 To 22/09/2026", "UNIT_TEXT", "[Unit: All]",
               "RUN_INFO", "Rundate 22-Sep-2026 21:50   User TEST", "SKIPPED", "");
    }

    static int Main(string[] args)
    {
        var dir = args[0]; var outDir = args[1];
        try
        {
            BuildFulls(dir); Console.WriteLine("FullsStockActual.rpt saved");
            BuildStock(dir); Console.WriteLine("StockReport.rpt saved");
            TestFulls(dir, outDir); Console.WriteLine("FullsStockActual-test.pdf rendered");
            TestStock(dir, outDir); Console.WriteLine("StockReport-test.pdf rendered");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR: " + ex);
            return 1;
        }
    }
}
