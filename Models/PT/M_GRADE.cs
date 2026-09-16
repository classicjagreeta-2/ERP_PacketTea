using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Finance.Models.PT
{
    public class M_GRADE
    {
        // ==================== NOT NULL ====================

        public virtual string LOCA { get; set; }
        public virtual string GCODE { get; set; }
        public virtual string U_NAME { get; set; }
        public virtual string O_USER { get; set; }
        public virtual string T_ID { get; set; }


        // ==================== NULLABLE ====================

        public virtual decimal? ID { get; set; }
        public virtual string GLOCA { get; set; }
        public virtual string UNIT { get; set; }
        public virtual string GR { get; set; }
        public virtual string NAME { get; set; }
        public virtual string SORT_NAME { get; set; }
        public virtual string GR_CODE { get; set; }
        public virtual string GRADE_TYPE { get; set; }

        public virtual int? PRINT_ORD { get; set; }
        public virtual int? GPRINT_ORD { get; set; }
        public virtual int? GRADEORDER { get; set; }

        public virtual string SALE_ACODE { get; set; }
        public virtual string REFCODE { get; set; }
        public virtual string OWNLEAF_P { get; set; }
        public virtual string BOUGHTLEAF_P { get; set; }
        public virtual string HSN_CODE { get; set; }
        public virtual string UNIT_CD { get; set; }
        public virtual string DESCN { get; set; }
        public virtual string COSTCODE { get; set; }
        public virtual string AUCTION_GRADE { get; set; }
        public virtual string TEA_TYPE { get; set; }
        public virtual string P_GCODE { get; set; }
        public virtual string OLDGCODE { get; set; }

        public virtual DateTime? NON_OP_DT { get; set; }

        public virtual string OWN_GRADE { get; set; }
        public virtual DateTime? U_ENTDT { get; set; }

        public virtual string U_NAMENEW { get; set; }
        public virtual DateTime? U_ENTDTNEW { get; set; }

        public virtual string DTAG { get; set; }
        public virtual string O_USERNEW { get; set; }
        public virtual string T_IDNEW { get; set; }
        public virtual string USER_NAME { get; set; }
        public virtual DateTime? USER_ENTDT { get; set; }
        public virtual string USER_NAME_NEW { get; set; }
        public virtual DateTime? USER_ENTDT_NEW { get; set; }

        public virtual string OS_USER { get; set; }
        public virtual string TERMINAL_ID { get; set; }
        public virtual string LOCKEDBYUSERID { get; set; }
        public virtual DateTime? LOCKEDUNTIL { get; set; }
    }
}