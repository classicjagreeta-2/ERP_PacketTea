using System.Collections.Generic;

namespace PacketTea.Models.PT
{
    // Save payload posted to /api/TeaBlend/SaveOrUpdate
    public class TEA_BLEND_DATA
    {
        public T_TEA_BLEND T_TEA_BLEND { get; set; }
        public List<T_TEA_BLEND_DET> T_TEA_BLEND_DET { get; set; } = new List<T_TEA_BLEND_DET>();
    }
}
