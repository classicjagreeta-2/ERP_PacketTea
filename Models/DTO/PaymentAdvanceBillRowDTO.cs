namespace PacketTea.Models.DTO
{
    public class PaymentAdvanceBillRowDTO
    {
        public bool status { get; set; }
        public string content { get; set; }=string.Empty;
        public int billId { get; set; }
    }
}
