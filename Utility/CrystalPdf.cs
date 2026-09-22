using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace PacketTea.Utility
{
    // Renders a Crystal .rpt (loaded from disk, e.g. ~/CrystalReport/X.rpt) to PDF
    // bytes -- same Load / SetDataSource / ExportToStream sequence as ERP_Payroll's
    // PayregisterController. Formula fields are set only when the .rpt defines them,
    // so a layout can use any subset of the headers the controller supplies.
    public static class CrystalPdf
    {
        public static byte[] Render(string rptPath, DataSet data, IDictionary<string, string> formulas)
        {
            var report = new ReportDocument();
            try
            {
                report.Load(rptPath);
                report.SetDataSource(data);

                if (formulas != null)
                {
                    var map = new Dictionary<string, string>(formulas, System.StringComparer.OrdinalIgnoreCase);
                    foreach (FormulaFieldDefinition f in report.DataDefinition.FormulaFields)
                    {
                        string value;
                        if (map.TryGetValue(f.Name, out value))
                            f.Text = "'" + (value ?? "").Replace("'", "''") + "'";
                    }
                }

                using (var stream = report.ExportToStream(ExportFormatType.PortableDocFormat))
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
            finally
            {
                try { report.Close(); } catch { }
                try { report.Dispose(); } catch { }
            }
        }
    }
}
