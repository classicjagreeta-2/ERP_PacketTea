using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PacketTea.Models
{
    public class MODULE
    {
        public virtual int ID { get; set; }
        public virtual string MODULE_NAME { get; set; }
        public virtual string MENU_TABLE { get; set; }
        public virtual string CONTROL_TABLE { get; set; }
        public virtual string MENU_SORT_BY { get; set; }
        public virtual string PERMISSION { get; set; }
        public virtual string MODULE_CODE { get; set; }
        public virtual string USRACS_TABLE { get; set; }
        public virtual string LOCA { get; set; } =Utility.SessionHelper.GetUser().Loca;
    }
}