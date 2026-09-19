using System;

namespace PacketTea.Models.PT
{
    // Mirrors ClassicERPCoreAPI.Models.PacketTea.PacketTea_OtherModel.TRN_INV_HEAD
    // (real TRN_INV_HEAD column names) plus the display fields the API's
    // OtherInvoice/GetByDocNo returns alongside the row.
    public class T_INV_HEAD
    {
        public string TRAN { get; set; }            // "1" Invoice, "2" Debit Note, "3" Credit Note
        public string DOC_YEAR { get; set; }
        public string LOCA { get; set; }
        public string UNIT { get; set; }
        public string DOCTYPE { get; set; }
        public string DOCNO { get; set; }
        public DateTime? DOCDT { get; set; }
        public string PCD { get; set; }

        public string CONTRACT_NO { get; set; }     // notes: the original invoice (DOCTYPE+DOCNO)
        public DateTime? CONTRACT_DATE { get; set; }
        public string CHL_NO { get; set; }
        public DateTime? CHL_DATE { get; set; }
        public string VEH_NO { get; set; }
        public string CNNO { get; set; }            // RR No
        public DateTime? CNDT { get; set; }         // RR Date
        public string NAR3 { get; set; }            // Remarks
        public string DELIV_ADD1 { get; set; }
        public string DELIV_ADD2 { get; set; }
        public string DELIV_ADD3 { get; set; }

        public string TRANS_CODE { get; set; }
        public string BANK_CODE { get; set; }
        public string RATE_TYPE { get; set; }       // "Y" = rate includes tax, "N" = excludes
        public string INV_TYPE { get; set; }
        public decimal? R_OFF { get; set; }
        public decimal? TOTAL_AMT { get; set; }
        public string IRN { get; set; }

        // Display-only fields returned by the API alongside the row.
        public string PartyName { get; set; }
        public string PartyStateCode { get; set; }
        public string PartyInterU { get; set; }
        public string PartyPan { get; set; }
        public string PartyTcsTag { get; set; }
        public string UnitStateCode { get; set; }
        public bool RefLastYear { get; set; }
        public string TransName { get; set; }
        public string BankName { get; set; }
        public string LockMessage { get; set; }
    }
}
