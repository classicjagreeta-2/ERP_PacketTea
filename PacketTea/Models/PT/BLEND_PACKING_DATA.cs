using System;
using System.Collections.Generic;

namespace PacketTea.Models.PT
{
    // Mirrors ClassicERPCoreAPI.Models.DTO.PacketTea.BLEND_PACKING_DATA -- property
    // names must match exactly (see T_BLEND_PACKING's comment on why).
    public class BLEND_PACKING_DATA
    {
        public string LOCA { get; set; }
        public string GLOCA { get; set; }
        public string UNIT { get; set; }
        public string DOCNO { get; set; }
        public DateTime? DOCDT { get; set; }
        public string BLEND_TYPE { get; set; }

        public string BlendDocNo { get; set; }
        public DateTime? BlendDocDt { get; set; }

        public string SaleCentre { get; set; }
        public string Remarks { get; set; }
        public decimal? ShortExcess { get; set; }

        public List<T_BLEND_PACKING> Details { get; set; } = new List<T_BLEND_PACKING>();

        // Display-only fields returned by the API's GetByDocNo/GetByPage
        public string MasterBlendNo { get; set; }
        public string MasterBlendDate { get; set; }
        public string BlendDocDate { get; set; }
        public string PCODE { get; set; }
        public string PartyName { get; set; }
        public string ShipMark { get; set; }
        public string SaleCentreName { get; set; }
        public decimal? TotalBag { get; set; }
        public decimal? TotalQty { get; set; }
    }
}
