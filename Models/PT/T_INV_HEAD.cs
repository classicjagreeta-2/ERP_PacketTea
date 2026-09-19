using System;

namespace PacketTea.Models.PT
{
    // Mirrors ClassicERPCoreAPI.Models.PacketTea.PacketTea_OtherModel.TRN_INV_HEAD.
    public class T_INV_HEAD
    {
        public int ID { get; set; }

        public string DOC_YEAR { get; set; }
        public string LOCA { get; set; }
        public string UNIT { get; set; }
        public string DOCTYPE { get; set; }
        public string DOCNO { get; set; }
        public DateTime? DOCDT { get; set; }
        public string PCD { get; set; }

        public string TRANS_CODE { get; set; }
        public string VEHICLE_NO { get; set; }
        public string CHALLAN_NO { get; set; }
        public DateTime? CHALLAN_DT { get; set; }
        public string REMARKS { get; set; }
        public string BANK_CODE { get; set; }
        public string RATE_TYPE { get; set; } // "I" = Incl. Tax, "E" = Excl. Tax

        public string DELIVERY_ADD1 { get; set; }
        public string DELIVERY_ADD2 { get; set; }
        public string DELIVERY_ADD3 { get; set; }

        public decimal? GROSS_AMT { get; set; }
        public decimal? ROUND_OFF { get; set; }
        public decimal? TOTAL_AMT { get; set; }

        public string IRN { get; set; }

        // Display-only fields returned by the API alongside the raw row.
        public string PartyName { get; set; }
        public string PartyStateCode { get; set; }
        public string BankName { get; set; }
    }
}
