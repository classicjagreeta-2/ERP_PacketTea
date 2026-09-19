using System;

namespace PacketTea.Models.PT
{
    // Mirrors ClassicERPCoreAPI.Models.PacketTea.PacketTea_OtherModel.MRETU.
    // Note the quantity-returned-in-good-condition column is exposed by the API
    // entity as "Mretu" (mixed case) -- C# forbids a member named identically to
    // its enclosing class (CS0542); Oracle folds it back to the real MRETU
    // column regardless of case. Mirrored here with the same casing so the JSON
    // round-trip (API's CamelCase serialization -> "mretu" -> this property via
    // Newtonsoft's case-insensitive binding) lines up without a custom mapping.
    public class T_MRETU
    {
        public int ID { get; set; }

        public string DOC_YEAR { get; set; }
        public string LOCA { get; set; }
        public string UNIT { get; set; }
        public string DOCNO { get; set; }
        public decimal? SRLNO { get; set; }
        public DateTime? DATE_ORA { get; set; }

        public string ITCD { get; set; }
        public string MRP { get; set; }
        public DateTime? MFGDT { get; set; }
        public string BATCHNO { get; set; }

        public decimal? Mretu { get; set; }
        public decimal? SHT { get; set; }
        public decimal? BKG { get; set; }
        public decimal? LKG { get; set; }

        public decimal? MRETU_BTLS { get; set; }
        public decimal? SHT_BTLS { get; set; }
        public decimal? BKG_BTLS { get; set; }
        public decimal? LKG_BTLS { get; set; }

        public string PLCRCD { get; set; }
        public decimal? PLCRQTY { get; set; }

        public decimal? RATE { get; set; }
        public decimal? DISCOUNT { get; set; }
        public decimal? AMT { get; set; }
        public string GST_CODE { get; set; }
        public decimal? CGST_RATE { get; set; }
        public decimal? CGST_AMT { get; set; }
        public decimal? SGST_RATE { get; set; }
        public decimal? SGST_AMT { get; set; }
        public decimal? IGST_RATE { get; set; }
        public decimal? IGST_AMT { get; set; }
        public decimal? GCESS_RATE { get; set; }
        public decimal? GCESS_AMT { get; set; }

        public decimal? WEIGHT { get; set; }
        public decimal? GROSS_WEIGHT { get; set; }

        public string DTAG { get; set; }

        // Display-only field returned by the API alongside the raw row.
        public string ItemName { get; set; }
    }
}
