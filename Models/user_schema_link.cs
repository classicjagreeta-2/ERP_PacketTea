using System;

namespace Finance.Models
{
    public class user_schema_link
    {
        public string LOCA { get; set; }
        public string UNIT { get; set; }
        public string USER_ORA { get; set; }
        public string USR_ID { get; set; }
        public string MODULE_NAME { get; set; }
        public DateTime? ENTDT { get; set; }
        public string TIME_ORA { get; set; }
        public string USER_NEW { get; set; }
        public DateTime? ENTDT_NEW { get; set; }
        public string TIME_NEW { get; set; }
        public string DTAG { get; set; }
        public string MACHINE_NO { get; set; }
        public string USER_ENT { get; set; }
        public long? API_ALLOW { get; set; }
        public long ID { get; set; } // NOT NULL
        public string MODULE_CODE { get; set; }
        public string TECHNOLOGY { get; set; }
    }
}
