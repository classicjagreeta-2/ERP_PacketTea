using System.Collections.Generic;

namespace PacketTea.Models.PT
{
    // Save payload posted to /api/TeaSampleDraw/SaveOrUpdate.
    public class T_SAMPLE_DRAW_PT_DATA
    {
        public List<T_SAMPLE_DRAW> T_SAMPLE_DRAW { get; set; } = new List<T_SAMPLE_DRAW>();
    }
}
