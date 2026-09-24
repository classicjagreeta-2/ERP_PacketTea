using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.IO;
using System.Security.Cryptography;
using System.Data.OleDb;
using System.Data;
using System.Data.Common;
using System.Reflection;
using DocumentFormat.OpenXml.Spreadsheet;

namespace PacketTea
{
    public class Helpers
    {
        public class UserInfo
        {
            private string _userid;
            private string _username;
            private string _password;
            private string _dbname;
            private string _dbtype;
            private string _token;
            private string _moduleCode;
            private string _loca;
            private string _docyear;
            private string _financialyear;
            private string _curentCompany;
            private string _curentLocation;
            private string _curentUnit;
            private string _moduleName; 
            private string _controlSchema; 
            private string _menuSchema;
            private string _address;
            private DateTime? _lastLoginDt;
            private string _factoryedb;
            private string _firstname;
            private string _financedb;
            private string _salesdb;
            public string DocYear
            {
                get { return _docyear; }
                set { _docyear = value; }
            }
            public string Address
            {
                get { return _address; }
                set { _address = value; }
            }
            public string Loca
            {
                get { return _loca; }
                set { _loca = value; }
            }
            public string ModuleCode
            {
                get { return _moduleCode; }
                set { _moduleCode = value; }
            }
            public string getUserId
            {
                get { return _userid; }
                set { _userid = value; }
            }
            public string getUserName
            {
                get { return _username; }
                set { _username = value; }
            }
            public string UserPassword
            {
                get { return _password; }
                set { _password = value; }
            }
            public DateTime? lastLoginDt
            {
                get { return _lastLoginDt; }
                set { _lastLoginDt = value; }
            }
            public string getToken
            {
                get { return _token; }
                set { _token = value; }
            }
            public string getDbName
            {
                get { return _dbname; }
                set { _dbname = value; }
            }
            public string getDbType
            {
                get { return _dbtype; }
                set { _dbtype = value; }
            }

            public string getUserfullName
            {
                get { return _firstname; }
                set { _firstname = value; }
            }
            public string Financialyear
            {
                get { return _financialyear; }
                set { _financialyear = value; }
            }
            public string CurentLocation
            {
                get { return _curentLocation; }
                set { _curentLocation = value; }
            }
            public string CurentCompany
            {
                get { return _curentCompany; }
                set { _curentCompany = value; }
            }
            public string CurentUnit
            {
                get { return _curentUnit; }
                set { _curentUnit = value; }
            }
            public string ModuleName
            {
                get { return _moduleName; }
                set { _moduleName = value; }
            }
            public string ControlSchema
            {
                get { return _controlSchema; }
                set { _controlSchema = value; }
            }
            public string MenuSchema
            {
                get { return _menuSchema; }
                set { _menuSchema = value; }
            }
            public string Factorydb
            {
                get { return _factoryedb; }
                set { _factoryedb = value; }
            }
            public string Financedb
            {
                get { return _financedb; }
                set { _financedb = value; }
            }
            public string Salesdb
            {
                get { return _salesdb; }
                set { _salesdb = value; }
            }
        }

        //public static class UserInfoStatic1
        //{
        //    private static string _userid;
        //    private static string _username;
        //    private static string _dbname;
        //    private static string _dbtype;
        //    private static string _token;
        //    private static string _moduleCode;
        //    private static string _loca;
        //    private static DateTime? _lastLoginDt;
        //    public static string Loca
        //    {
        //        get { return _loca; }
        //        set { _loca = value; }
        //    }
        //    public static string ModuleCode
        //    {
        //        get { return _moduleCode; }
        //        set { _moduleCode = value; }
        //    }
        //    public static string getUserId
        //    {
        //        get { return _userid; }
        //        set { _userid = value; }
        //    }
        //    public static string getUserName
        //    {
        //        get { return _username; }
        //        set { _username = value; }
        //    }
        //    public static DateTime? lastLoginDt
        //    {
        //        get { return _lastLoginDt; }
        //        set { _lastLoginDt = value; }
        //    }
        //    public static string getToken
        //    {
        //        get { return _token; }
        //        set { _token = value; }
        //    }
        //    public static string getDbName
        //    {
        //        get { return _dbname; }
        //        set { _dbname = value; }
        //    }
        //    public static string getDbType
        //    {
        //        get { return _dbtype; }
        //        set { _dbtype = value; }
        //    }
        //}

        public static class UserAEDV
        {
            private static string _autoid;
            private static string _id;
            private static string _name;
            private static string _indeX_ORA;
            private static string _pid;
            private static string _ordercode;
            private static string _aedv;
            private static string _eday;
            private static string _aday;
            private static string _perdotnetmenu;
            private static string _atvdotnetmenu;
            private static string _dday;
            public static string AUTOID
            {
                get { return _autoid; }
                set { _autoid = value; }
            }
            public static string ID
            {
                get { return _id; }
                set { _id = value; }
            }
            public static string NAME
            {
                get { return _name; }
                set { _name = value; }
            }
            public static string INDEX_ORA
            {
                get { return _indeX_ORA; }
                set { _indeX_ORA = value; }
            }
            public static string PID
            {
                get { return _pid; }
                set { _pid = value; }
            }
            public static string ORDERCODE
            {
                get { return _ordercode; }
                set { _ordercode = value; }
            }
            public static string AEDV
            {
                get { return _aedv; }
                set { _aedv = value; }
            }
            public static string EDAY
            {
                get { return _eday; }
                set { _eday = value; }
            }
            public static string ADAY
            {
                get { return _aday; }
                set { _aday = value; }
            }
            public static string PERDOTNETMENU
            {
                get { return _perdotnetmenu; }
                set { _perdotnetmenu = value; }
            }
            public static string ATVDOTNETMENU
            {
                get { return _atvdotnetmenu; }
                set { _atvdotnetmenu = value; }
            }
            public static string DDAY
            {
                get { return _dday; }
                set { _dday = value; }
            }
        }
        public class AEDV
        {
            public object Autoid { get; set; }
            public string Id { get; set; }
            public string Name { get; set; }
            public string IndeXORA { get; set; }
            public string Pid { get; set; }
            public string Ordercode { get; set; }
            public string Aedv { get; set; }
            public int Eday { get; set; }
            public int Aday { get; set; }
            public string Perdotnetmenu { get; set; }
            public string Atvdotnetmenu { get; set; }
            public int Dday { get; set; }
            public string Controller { get; set; }
            private bool _add;
            private bool _edit;
            private bool _delete;
            private bool _view;

            // Null-safe right check (Aedv is null when the menu API sends no AEDV string
            // for this entry) -- every Add/Edit/Delete/View getter below routes through
            // this instead of calling Aedv.Contains(...) directly, each against its own
            // backing field (previously all four shared _add, so reading e.g. .Add right
            // after .Edit returned Edit's result).
            public bool Can(char right) => (Aedv ?? "").IndexOf(right) >= 0;

            public bool Add
            {
                get { _add = Can('A'); return _add; }
                set { _add = value; }
            }
            public bool Edit
            {
                get { _edit = Can('E'); return _edit; }
                set { _edit = value; }
            }
            public bool Delete
            {
                get { _delete = Can('D'); return _delete; }
                set { _delete = value; }
            }
            public bool View
            {
                get { _view = Can('V'); return _view; }
                set { _view = value; }
            }

            // Back-date policy: a new document may be dated at most Aday days back, and an
            // existing one may only be edited while its own date is within Eday days of today.
            public DateTime MinDocDate(bool isNew) => DateTime.Today.AddDays(-(isNew ? Aday : Eday));

            // Null when the user may add (isNew) / edit a document dated docDate, else the
            // message to show them.
            public string CheckAddEdit(bool isNew, DateTime docDate)
            {
                if (!Can(isNew ? 'A' : 'E'))
                    return isNew ? "You do not have permission to add a new entry."
                                 : "You do not have permission to edit this entry.";

                if (isNew && docDate.Date > DateTime.Today)
                    return "Doc Date cannot be a future date.";

                var minDate = MinDocDate(isNew);
                if (docDate.Date < minDate)
                    return isNew
                        ? $"Back date entry is not allowed. Doc Date cannot be earlier than {minDate:dd/MM/yyyy} ({Aday} day(s) back)."
                        : $"Edit is not allowed. Only entries dated on or after {minDate:dd/MM/yyyy} ({Eday} day(s) back) can be edited.";

                return null;
            }

            // Null-permission (user has no AEDV row for the screen) is treated as "no rights",
            // same as the list pages' "?? false".
            public static string CheckAddEdit(AEDV permission, bool isNew, DateTime docDate) =>
                permission == null
                    ? (isNew ? "You do not have permission to add a new entry." : "You do not have permission to edit this entry.")
                    : permission.CheckAddEdit(isNew, docDate);

            // Rights for a screen: its own row (Controller == screen) and the shared
            // "PacketTeaPurchaseEntry" row this app's blend screens have always read, combined
            // -- a letter held on either row counts, and the more generous back-date window
            // wins. Lets a screen work whether or not the menu setup has provisioned a row of its
            // own for it. Null only when the user has neither row.
            public static AEDV ForScreen(List<AEDV> all, string controller, string sharedBucket = "PacketTeaPurchaseEntry")
            {
                var own = all?.FirstOrDefault(l => l.Controller == controller);
                var shared = all?.FirstOrDefault(l => l.Controller == sharedBucket);
                if (own == null) return shared;
                if (shared == null || ReferenceEquals(own, shared)) return own;

                var letters = new string("AEDV".Where(c => own.Can(c) || shared.Can(c)).ToArray());
                return new AEDV
                {
                    Autoid = own.Autoid, Id = own.Id, Name = own.Name, IndeXORA = own.IndeXORA,
                    Pid = own.Pid, Ordercode = own.Ordercode, Perdotnetmenu = own.Perdotnetmenu,
                    Atvdotnetmenu = own.Atvdotnetmenu, Controller = own.Controller,
                    Aedv = letters,
                    Aday = Math.Max(own.Aday, shared.Aday),
                    Eday = Math.Max(own.Eday, shared.Eday),
                    Dday = Math.Max(own.Dday, shared.Dday)
                };
            }
        }
    }

    // Dates the API sends as text ("dd/MM/yyyy"). Never DateTime.Parse them: that uses the
    // SERVER's culture, so "24/09/2026" throws FormatException on an en-US host (worked on
    // the dev PC's en-IN, broke the Packing list when deployed).
    public static class DateText
    {
        static readonly string[] Formats = { "dd/MM/yyyy", "dd/MM/yyyy HH:mm:ss", "yyyy-MM-dd", "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm:ss.FFFFFFF" };

        // dd/MM/yyyy for display; blank for empty, the original text if it isn't a recognisable date.
        public static string Display(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";
            return DateTime.TryParseExact(s.Trim(), Formats, System.Globalization.CultureInfo.InvariantCulture,
                       System.Globalization.DateTimeStyles.None, out var d)
                ? d.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture)
                : s;
        }
    }

    // Unit scoping for the Blend screens (Master / Final / Packing) -- see CLAUDE.md's
    // "Unit-wise permission rule". `allowed` is the user's USER_SCHEMA_LINK units as returned
    // by the API's GetUnitsForUser (each controller's GetUnitsForUserAsync()).
    public static class UnitScope
    {
        // Matches no real unit code -- sent to the API when the user has no units at all, so
        // the list comes back empty rather than (with a blank `unit`) unfiltered.
        public const string NoUnits = "~";

        public static bool IsAllowed(string unit, IEnumerable<PacketTea.Models.PT.UnitOption> allowed) =>
            !string.IsNullOrWhiteSpace(unit) && allowed != null
            && allowed.Any(u => string.Equals((u.CODE ?? "").Trim(), unit.Trim(), StringComparison.OrdinalIgnoreCase));

        // Value for the API's `unit` parameter on a LIST: the one unit asked for (if the user
        // may see it), otherwise every unit the user is linked to as a comma-separated list.
        public static string ListFilter(string requested, List<PacketTea.Models.PT.UnitOption> allowed)
        {
            if (IsAllowed(requested, allowed)) return requested.Trim();
            if (allowed == null || allowed.Count == 0) return NoUnits;
            return string.Join(",", allowed.Select(u => (u.CODE ?? "").Trim()).Where(c => c.Length > 0));
        }
    }
}
