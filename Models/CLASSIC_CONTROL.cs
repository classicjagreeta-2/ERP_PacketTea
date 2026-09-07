using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PacketTea.Models
{
    public class CLASSIC_CONTROL
    {
        public virtual long ID { get; set; }
        public virtual string SRLNO { get; set; }
        public virtual string NAME { get; set; }
        public virtual DateTime YRFR { get; set; }
        public virtual DateTime YRTO { get; set; }
        public virtual string ACFIL { get; set; }
        public virtual string ACMFIL { get; set; }
        public virtual string ACREF { get; set; }
        public virtual string SIMB { get; set; }
        public virtual string YR { get; set; }
        public virtual string ADD1 { get; set; }
        public virtual string CITY { get; set; }
        public virtual string PIN { get; set; }
        public virtual string LOCA { get; set; } =Utility.SessionHelper.GetUser().Loca;
        public virtual string UNITDEST { get; set; }
        public virtual string USR_ID { get; set; }
        public virtual string LUSR_ID { get; set; }
        public virtual string DATAVER { get; set; }
        public virtual string EXP_TAG { get; set; }
        public virtual string IMP_TAG { get; set; }
        public virtual string MERGE_TAG { get; set; }
        public virtual string MIRROR_TAG { get; set; }
        public virtual string TRIG_TAG { get; set; }
        public virtual string SCHEMA_SALES { get; set; }
        public virtual string SCHEMA_PROD { get; set; }
        public virtual string SCHEMA_PAY { get; set; }
        public virtual string SCHEMA_INV { get; set; }
        public virtual string SCHEMA_RM { get; set; }
        public virtual string MAIN_UNIT { get; set; }
        public virtual string VNAME { get; set; }
        public virtual string SCHEMA_FINANCE { get; set; }
        public virtual string SCHEMA_FACTORY { get; set; }
        public virtual string ACTIVE_TAG { get; set; }
        public virtual string GRP { get; set; }
        public virtual string Module { get; set; }
    }
}