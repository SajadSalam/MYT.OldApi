namespace Events.DATA.DTOs.Payment
{
    /// <summary>
    /// Request model for confirming a payment (e.g. Point of Sale pay endpoint)
    /// </summary>
    public class PayBillRequest
    {
        public string BillId { get; set; }
        
        public string SecretKey { get; set; }
        
        public DateTime? PayDate { get; set; }
    }
}
