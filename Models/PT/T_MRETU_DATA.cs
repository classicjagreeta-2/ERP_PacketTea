using System.Collections.Generic;

namespace PacketTea.Models.PT
{
    // Save payload posted to /api/MarketReturn/SaveOrUpdate -- head + item
    // detail lines, same shape as TEA_BLEND_DATA.
    public class T_MRETU_DATA
    {
        public T_MRETU_HED T_MRETU_HED { get; set; } = new T_MRETU_HED();
        public List<T_MRETU> T_MRETU_DET { get; set; } = new List<T_MRETU>();

        // Quantity-vs-Net-Weight toggle ("QTY" default or "WT") -- see
        // ClassicERPCoreAPI's MRETU_DATA.OptFlag for what this drives.
        public string OptFlag { get; set; } = "QTY";

        // 0 = current year, 1 = last year, 2 = before last year (VB6 ChkLastYr/chkLLYr).
        public int BillYearBack { get; set; }

        // Sent by the entry screen (IsNew: !IS_EDIT) so Save can apply the AEDV Add vs Edit check +
        // back-date window to the right case -- see MarketReturnController.Save.
        public bool? IsNew { get; set; }
    }
}
