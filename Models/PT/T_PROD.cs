using System;

namespace PacketTea.Models.PT
{
    // Mirrors ClassicERPCoreAPI.Models.PacketTea.PacketTea_OtherModel.PROD.
    // Flat -- one row IS the whole document, no header/detail split.
    public class T_PROD
    {
        public int ID { get; set; }
        public string LOCA { get; set; }
        public DateTime? DATE_ORA { get; set; }
        public string ITCD { get; set; }
        public string TM { get; set; }
        public string MRP { get; set; }
        public decimal? PD { get; set; }
        public decimal? PD_BTLS { get; set; }
        public string UNIT { get; set; }
        public string DOCNO { get; set; }
        public decimal? ID_ENT { get; set; }
        public decimal? ID_EDIT { get; set; }
        public decimal? SRLNO { get; set; }
        public string BATCHNO { get; set; }
        public DateTime? MFGDT { get; set; }
        public string SHIFT { get; set; }
        public string TIME_FROM { get; set; }
        public string TIME_TO { get; set; }
        public decimal? GROSS_WEIGHT { get; set; }
        public decimal? NET_WEIGHT { get; set; }
        public decimal? GAIN_LOSS { get; set; }

        // Display-only field returned by the API alongside the raw row.
        public string ItemName { get; set; }
    }
}
