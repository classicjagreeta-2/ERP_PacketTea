using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacketTea.Models.PT
{
    public class UNBLPLT
    {
        public virtual string LOCA { get; set; }

        public virtual string RT { get; set; }

        public virtual int SRLNO { get; set; }

        public virtual string BLNO { get; set; }

        public virtual DateTime BLDT { get; set; }

        public virtual string PCD { get; set; }

        public virtual string PLCRCD { get; set; }

        public virtual decimal? PLCRQTY { get; set; } = 0;

        public virtual decimal? PLCRVAL { get; set; } = 0;

        public virtual string TM { get; set; }

        public virtual string UNIT { get; set; }

        public virtual string DOC_YEAR { get; set; }

        public virtual string DTAG { get; set; }

        public virtual int ID { get; set; }

        public virtual string USER_NAME_NEW { get; set; }

        public virtual DateTime? USER_ENTDT_NEW { get; set; }

        public virtual string USER_NAME { get; set; }

        public virtual DateTime? USER_ENTDT { get; set; }

        public virtual string OS_USER { get; set; }

        public virtual string TERMINAL_ID { get; set; }

        public virtual decimal? PLCRRATE { get; set; } = 0;
    }
}
