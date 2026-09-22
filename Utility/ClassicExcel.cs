using ClosedXML.Excel;
using System;
using System.Diagnostics;
using System.IO;
using System.Web;

namespace PacketTea.Utility
{
    // Report file output, same convention as ERP_Payroll's Daily Attendance Checklist:
    // for Excel every earlier Excel file in C:\classic is deleted, the new workbook is
    // saved there and opened in Excel (text reports replace only their own file). Opening only works when the web server runs in the
    // signed-in user's own desktop session (IIS Express / local IIS on that PC); on a
    // shared IIS server the caller falls back to streaming the saved file back to the
    // browser (see Download).
    public static class ClassicExcel
    {
        public const string Folder = @"C:\classic";

        public class Result
        {
            public string FileName { get; set; }
            public bool Opened { get; set; }
        }

        public static Result SaveAndOpen(XLWorkbook wb, string fileName, HttpRequestBase request)
        {
            return SaveAndOpen(path => wb.SaveAs(path), fileName, "*.xls*", request);
        }

        // Character-format (dot-matrix) text report: saved to C:\classic, replacing only
        // a file of the same name, and opened with the default .txt program.
        public static Result SaveTextAndOpen(string text, string fileName, HttpRequestBase request)
        {
            return SaveAndOpen(path => File.WriteAllText(path, text, System.Text.Encoding.ASCII), fileName, null, request);
        }

        private static Result SaveAndOpen(Action<string> write, string fileName, string deletePattern, HttpRequestBase request)
        {
            if (!Directory.Exists(Folder)) Directory.CreateDirectory(Folder);
            var safeName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
            var path = Path.Combine(Folder, safeName);

            var toDelete = deletePattern != null ? Directory.EnumerateFiles(Folder, deletePattern)
                         : File.Exists(path) ? new[] { path } : new string[0];
            foreach (var old in toDelete)
            {
                try
                {
                    File.Delete(old);
                }
                catch (IOException)
                {
                    throw new IOException("An earlier report file is still open - please close " + old + " and try again.");
                }
                catch (UnauthorizedAccessException)
                {
                    throw new IOException("Cannot delete " + old + " - check that it is not read-only or open, then try again.");
                }
            }

            write(path);

            var opened = false;
            if (Environment.UserInteractive && request != null && request.IsLocal)
            {
                try
                {
                    Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
                    opened = true;
                }
                catch (Exception)
                {
                    opened = false;
                }
            }
            return new Result { FileName = safeName, Opened = opened };
        }

        // Bytes of a file saved by SaveAndOpen; "file" is reduced to a bare name so
        // nothing outside C:\classic can be read. Null when it does not exist.
        public static byte[] Read(string file)
        {
            if (string.IsNullOrWhiteSpace(file)) return null;
            var path = Path.Combine(Folder, Path.GetFileName(file));
            return File.Exists(path) ? File.ReadAllBytes(path) : null;
        }
    }
}
