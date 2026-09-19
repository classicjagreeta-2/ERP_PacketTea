using System.Collections.Generic;

namespace PacketTea.Models.PT
{
    // Save payload posted to /api/OtherInvoice/SaveOrUpdate. Property names
    // deliberately match ClassicERPCoreAPI's DTO (Models\DTO\PacketTea\TRN_INV_DATA.cs)
    // exactly -- TRN_INV_HEAD / TRN_INV_DETAIL, not T_INV_HEAD/T_INV_DETAIL -- so this
    // round-trips through plain JSON serialization the same way TEA_BLEND_DATA's
    // properties (T_TEA_BLEND/T_TEA_BLEND_DET) match their API-side counterpart.
    public class T_INV_DATA
    {
        public T_INV_HEAD TRN_INV_HEAD { get; set; } = new T_INV_HEAD();
        public List<T_INV_DETAIL> TRN_INV_DETAIL { get; set; } = new List<T_INV_DETAIL>();
    }
}
