using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Finance.Models.Finance
{
    public class Reference
    {
        public string loca { get; set; }
        public int id { get; set; }
        public string cd { get; set; }
        public string nam { get; set; }
        public string adds { get; set; }
        public string addss { get; set; }
        public string tag { get; set; }
        public string dtag { get; set; }
        public string useR_ORA { get; set; }
        public DateTime? entdt { get; set; }
        public string timE_ORA { get; set; }
        public string useR_NEW { get; set; }
        public string entdT_NEW { get; set; }
        public string timE_NEW { get; set; }
        public string machinE_NO { get; set; }
        public string ffcode { get; set; }
        public string comP_NAME { get; set; }
        public string grP_CODE { get; set; }
        public string useR_NAME { get; set; }
        public string useR_ENTDT { get; set; }
        public string useR_NAME_NEW { get; set; }
        public string useR_ENTDT_NEW { get; set; }
        public string oS_USER { get; set; }
        public string terminaL_ID { get; set; }
    }
    /*public class Reference
    {
        public virtual string CODE { get; set; }
        public virtual string DESCRIPTION { get; set; }

        public virtual string SHTDES { get; set; }

        public virtual string FULLDES { get; set; }

        public virtual string ACTIVEFROM { get; set; }
        public virtual string ACTIVETO { get; set; }

        public virtual string FCCODE { get; set; }

        public List<Referenceacclist> Accountlist { get; set; }
        public List<Referenceunitlist> Unitlist { get; set; }



    }
    public class Referenceacclist
    {
        public virtual string ACCODE { get; set; }
        public virtual string SUBCODE { get; set; }
        public virtual string DESCRIPTION2 { get; set; }
    }

    public class Referenceunitlist
    {
        public virtual string UNIT { get; set; }
        public virtual string DESCRIPTION3 { get; set; }
    }*/

}
