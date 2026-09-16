using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacketTea.Models.PT
{
    public class SLMAS
    {
        public virtual string LOCA { get; set; }
        public virtual string PCD { get; set; }
        public virtual string ACNAME { get; set; }
        public virtual string ADD1 { get; set; }
        public virtual string ADD2 { get; set; }
        public virtual string CITY { get; set; }
        public virtual string PIN { get; set; }
        public virtual string ACD { get; set; }
        public virtual string OCCU { get; set; }
        public virtual string RSE { get; set; }

        public virtual decimal? POPU { get; set; }

        public virtual string RTCD { get; set; }
        public virtual string STATE { get; set; }
        public virtual string INTERU { get; set; }
        public virtual string CCACL { get; set; }
        public virtual string CCACE { get; set; }
        public virtual string CCAC_E { get; set; }
        public virtual string BRNCD { get; set; }
        public virtual string DISTRI { get; set; }

        public virtual decimal? CRLIMIT { get; set; }
        public virtual decimal? CRLIMIT_E { get; set; }

        public virtual DateTime? NONOP { get; set; }
        public virtual string LSTNO { get; set; }
        public virtual DateTime? LSTDT { get; set; }
        public virtual string CSTNO { get; set; }
        public virtual DateTime? CSTDT { get; set; }

        public virtual string DEST { get; set; }
        public virtual string TELNO { get; set; }
        public virtual string CONTACT { get; set; }
        public virtual string STAG { get; set; }
        public virtual string USER_ORA { get; set; }

        public virtual DateTime? ENTDT { get; set; }

        public virtual string TIME_ORA { get; set; }
        public virtual string CRACD { get; set; }
        public virtual string CRSUBCD { get; set; }
        public virtual string AUT_NAUT { get; set; }

        public virtual DateTime? AUT_DT { get; set; }
        public virtual DateTime? NAUT_DT { get; set; }

        public virtual string TPTRTCD { get; set; }

        public virtual decimal? DISTANCE { get; set; }

        public virtual string USER_NEW { get; set; }

        public virtual DateTime? ENTDT_NEW { get; set; }

        public virtual string TIME_NEW { get; set; }
        public virtual string DTAG { get; set; }

        public virtual decimal? INVNORLIQ { get; set; }
        public virtual decimal? INVNOREMP { get; set; }

        public virtual DateTime? ACTIVE_FROM { get; set; }
        public virtual DateTime? MARK_ACTDATE { get; set; }
        public virtual DateTime? FOOD_CERT_DT { get; set; }
        public virtual DateTime? GST_REGDT { get; set; }
        public virtual DateTime? EDT_UNREGD { get; set; }
        public virtual DateTime? EDT_COMPOSITE { get; set; }
        public virtual DateTime? EDT_REGD { get; set; }

        public virtual decimal? MIN_SERV_TM { get; set; }
        public virtual decimal? STOCK_NORMS { get; set; }
        public virtual decimal? GLASS_COL_BAL { get; set; }
        public virtual decimal? PLCRATE_COL_BAL { get; set; }
        public virtual decimal? STD_SERV_TM { get; set; }
        public virtual decimal? PSR_VISIT_FREQUENCY { get; set; }
        public virtual decimal? PSR_SERVICE_TIME { get; set; }

        public virtual int? ID { get; set; }
    }
}
