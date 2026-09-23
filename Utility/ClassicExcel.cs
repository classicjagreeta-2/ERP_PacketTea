using ClosedXML.Excel;
using System;
using System.Diagnostics;
using System.IO;
using System.Web;
using System.Web.Caching;

namespace PacketTea.Utility
{
    // Report file output, same convention as ERP_Payroll's Daily Attendance Checklist:
    // when the web server runs in the signed-in user's own desktop session (IIS Express
    // on that PC) every earlier Excel file in C:\classic is deleted, the new workbook is
    // saved there and opened in Excel (text reports replace only their own file).
    // On the hosted server (e.g. http://103.234.116.64:90/PT) the file could only be
    // opened on the server itself, and C:\classic is shared by every user (one user's
    // Excel click deleted another's file before it was downloaded), so nothing is
    // written to disk: the bytes are kept in memory for a few minutes under a one-time
    // token and the page downloads them through Take.
    public static class ClassicExcel
    {
        public const string Folder = @"C:\classic";
        private static readonly TimeSpan Keep = TimeSpan.FromMinutes(10);

        public class Result
        {
            public string FileName { get; set; }
            public bool Opened { get; set; }
            // Set when the file was not opened here; the page downloads it with this.
            public string Token { get; set; }
        }

        private class Held
        {
            public string FileName;
            public byte[] Bytes;
        }

        public static Result SaveAndOpen(XLWorkbook wb, string fileName, HttpRequestBase request)
        {
            byte[] bytes;
            using (var ms = new MemoryStream())
            {
                wb.SaveAs(ms);
                bytes = ms.ToArray();
            }
            return SaveAndOpen(bytes, fileName, "*.xls*", request);
        }

        // Character-format (dot-matrix) text report: saved to C:\classic, replacing only
        // a file of the same name, and opened with the default .txt program.
        public static Result SaveTextAndOpen(string text, string fileName, HttpRequestBase request)
        {
            return SaveAndOpen(System.Text.Encoding.ASCII.GetBytes(text), fileName, null, request);
        }

        private static Result SaveAndOpen(byte[] bytes, string fileName, string deletePattern, HttpRequestBase request)
        {
            var safeName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));

            if (!CanOpenHere(request))
                return Hold(bytes, safeName);

            if (!Directory.Exists(Folder)) Directory.CreateDirectory(Folder);
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

            File.WriteAllBytes(path, bytes);

            try
            {
                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
                return new Result { FileName = safeName, Opened = true };
            }
            catch (Exception)
            {
                // Saved but no program could open it: hand it to the browser instead.
                return Hold(bytes, safeName);
            }
        }

        private static Result Hold(byte[] bytes, string fileName)
        {
            var token = Guid.NewGuid().ToString("N");
            HttpRuntime.Cache.Insert(Key(token), new Held { FileName = fileName, Bytes = bytes }, null,
                DateTime.UtcNow.Add(Keep), Cache.NoSlidingExpiration);
            return new Result { FileName = fileName, Opened = false, Token = token };
        }

        // Only a browser on the web server's own PC, with the site running in a desktop
        // session, can see a file opened by Process.Start.
        private static bool CanOpenHere(HttpRequestBase request)
        {
            return Environment.UserInteractive && request != null && request.IsLocal;
        }

        private static string Key(string token) => "ClassicExcel:" + token;

        // The file held by SaveAndOpen under "token" (removed once taken), or null when
        // the token is unknown or has expired.
        public static byte[] Take(string token, out string fileName)
        {
            fileName = null;
            if (string.IsNullOrWhiteSpace(token)) return null;
            var held = HttpRuntime.Cache.Remove(Key(token.Trim())) as Held;
            if (held == null) return null;
            fileName = held.FileName;
            return held.Bytes;
        }
    }
}
