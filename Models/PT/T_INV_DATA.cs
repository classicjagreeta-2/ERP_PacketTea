using System.Collections.Generic;

namespace PacketTea.Models.PT
{
    // Save/preview payload posted to /api/OtherInvoice/Preview and /SaveOrUpdate.
    // Property names match ClassicERPCoreAPI's DTO (Models\DTO\PacketTea\TRN_INV_DATA.cs)
    // exactly -- TRN_INV_HEAD / TRN_INV_DETAIL / LEGS -- so it round-trips through
    // plain JSON serialization.
    public class T_INV_DATA
    {
        public string TRAN { get; set; }
        public T_INV_HEAD TRN_INV_HEAD { get; set; } = new T_INV_HEAD();
        public List<T_INV_DETAIL> TRN_INV_DETAIL { get; set; } = new List<T_INV_DETAIL>();
        public List<T_INV_GL_LEG> LEGS { get; set; } = new List<T_INV_GL_LEG>();
    }

    // Ref Code / Cost Centre entered on the accounting preview, per voucher line.
    public class T_INV_GL_LEG
    {
        public int SRLNO { get; set; }
        public string ACODE { get; set; }
        public string REF_CODE { get; set; }
        public string COST_CENTRE { get; set; }
    }
}
