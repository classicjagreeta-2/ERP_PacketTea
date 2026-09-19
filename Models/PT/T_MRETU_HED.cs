using System;

namespace PacketTea.Models.PT
{
    // Mirrors ClassicERPCoreAPI.Models.PacketTea.PacketTea_OtherModel.MRETU_HED.
    public class T_MRETU_HED
    {
        public int ID { get; set; }

        public string DOC_YEAR { get; set; }
        public string LOCA { get; set; }
        public string UNIT { get; set; }
        public string DOCNO { get; set; }
        public DateTime? DATE_ORA { get; set; }
        public string BLNO { get; set; }
        public DateTime? BLDT { get; set; }
        // The table stores the bill date in EDATE (API entity MRETU_HED.EDATE);
        // sent to the API on save, BLDT stays the screen-side name.
        public DateTime? EDATE => BLDT;
        public string PCD { get; set; }
        public string TPT { get; set; }
        public string VEH { get; set; }
        public string VTYPE { get; set; }
        public string LR_NO { get; set; }
        public string INVNO { get; set; }
        public string RET_TYPE { get; set; }
        public string IRN { get; set; }
        public decimal? TRIP_NO { get; set; }
        public string TRIP_UNIT { get; set; }

        public string USER_ORA { get; set; }
        public DateTime? ENTDT { get; set; }
        public string TIME_ORA { get; set; }
        public string DTAG { get; set; }
        public string USER_NEW { get; set; }
        public DateTime? ENTDT_NEW { get; set; }
        public string TIME_NEW { get; set; }

        // Display-only fields returned by the API alongside the raw row.
        public string PartyName { get; set; }
        public string TransporterName { get; set; }
    }
}
