namespace Events.DATA.DTOs.Payment
{
    /// <summary>
    /// Unified payment request for all payment gateways
    /// </summary>
    public class PaymentRequest
    {
        public decimal Amount { get; set; }
        
        public string CustomerName { get; set; }
        
        public string CustomerPhone { get; set; }
        
        public string? CustomerEmail { get; set; }
        
        public Guid BookId { get; set; }
        
        public Guid EventId { get; set; }
        
        public DateTime? ExpireDate { get; set; }
        
        public string? RedirectUrl { get; set; }
        
        public string? Currency { get; set; } = "USD";
        
        /// <summary>
        /// Additional metadata specific to the payment provider
        /// </summary>
        public Dictionary<string, string>? Metadata { get; set; }
    }
}

