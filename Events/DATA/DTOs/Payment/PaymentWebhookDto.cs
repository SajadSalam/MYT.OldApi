namespace Events.DATA.DTOs.Payment
{
    /// <summary>
    /// Base class for payment webhook data
    /// </summary>
    public class PaymentWebhookDto
    {
        public string? PaymentId { get; set; }
        
        public string? Status { get; set; }
        
        public string? Event { get; set; }
        
        public DateTime? Timestamp { get; set; }
        
        public Dictionary<string, object>? Data { get; set; }
    }

    /// <summary>
    /// Sadid-specific webhook payload
    /// </summary>
    public class SadidWebhookDto : PaymentWebhookDto
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

    /// <summary>
    /// Stripe-specific webhook payload
    /// </summary>
    public class StripeWebhookDto : PaymentWebhookDto
    {
        public string? Type { get; set; }
        
        public string? ObjectType { get; set; }
        
        public bool? Livemode { get; set; }
    }
}

