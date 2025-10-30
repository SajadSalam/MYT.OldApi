namespace Events.DATA.DTOs.Sadid
{
    public class CreateBillResponse
    {
        public string? BillId { get; set; }
    }
    
    public class PayBillResponse
    {
            public string? BillId { get; set; }
            public string? PhoneNumber { get; set; }
            public string? CustomerName { get; set; }
            public Guid? ServiceId { get; set; }
            public DateTime? CreateDate { get; set; }
            public DateTime? PayDate { get; set; }
            public decimal? Amount { get; set; }
            public int? PaymentMethod { get; set; }
            public string? SecretKey { get; set; }
    }
}