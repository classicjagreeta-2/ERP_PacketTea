using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PacketTea.Models
{
    public class Model_User
    {
        public string User_Name { get; set; }
        public string UserName { get; set; }
        public string LoginId { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public string FKLoginId { get; set; }
        public int FkCompanyId { get; set; }
        public Nullable<int> UserType { get; set; }
        public bool? IsActive { get; set; }
        public int? GroupCategory { get; set; }
        public string LastLoginData { get; set; }
        public string DbType { get; set; }
        public string LOCA { get; set; }
        public string P1 { get; set; }


    }
}