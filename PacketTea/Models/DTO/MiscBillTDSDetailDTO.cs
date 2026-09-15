namespace PacketTea.Models.DTO
{
    public class MiscBillTDSDetailDTO
    {
        public string MainCode { get; set; }
        public string Subcode { get; set; }
        public string Description { get; set; }
        public string RefCode { get; set; }
        public string RefName { get; set; }
        public string CostCenterCode { get; set; }
        public string CostCenterName { get; set; }
        public decimal DrAmount { get; set; }
        public decimal CrAmount { get; set; }
    }
}
