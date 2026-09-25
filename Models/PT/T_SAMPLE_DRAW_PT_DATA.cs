using System.Collections.Generic;

namespace PacketTea.Models.PT
{
    // Save payload posted to /api/TeaSampleDraw/SaveOrUpdate.
    public class T_SAMPLE_DRAW_PT_DATA
    {
        public List<T_SAMPLE_DRAW> T_SAMPLE_DRAW { get; set; } = new List<T_SAMPLE_DRAW>();

        // Sent by the entry screen's Save (`IsNew: !IS_EDIT`) so the controller can enforce the
        // AEDV Add-vs-Edit right and back-date window -- the rows themselves don't reliably say
        // whether this is a new document. The API's DTO has no such member and ignores it.
        public bool? IsNew { get; set; }
    }
}
