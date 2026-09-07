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
            public bool Add
            {
                get
                {
                    var aa = this.Aedv.Contains("A");
                    _add = aa;
                    return this._add;
                }
                set
                {
                    this._add = value;
                }
            }
            public bool Edit
            {
                get
                {
                    var aa = this.Aedv.Contains("E");
                    _add = aa;
                    return this._add;
                }
                set
                {
                    this._add = value;
                }
            }
            public bool Delete
            {
                get
                {
                    var aa = this.Aedv.Contains("D");
                    _add = aa;
                    return this._add;
                }
                set
                {
                    this._add = value;
                }
            }
            public bool View
            {
                get
                {
                    var aa = this.Aedv.Contains("V");
                    _add = aa;
                    return this._add;
                }
                set
                {
                    this._add = value;
                }
            }
        }
    }
}
