namespace Events.DATA.DTOs.Payment
{
    /// <summary>
    /// Unified response from payment gateway operations
    /// </summary>
    public class PaymentResponse
    {
        public bool IsSuccess { get; set; }
        
        public string? PaymentId { get; set; }
        
        public string? Error { get; set; }
        
        public string? PaymentUrl { get; set; }
        
        public string? ClientSecret { get; set; }
        
        /// <summary>
        /// Additional data specific to the payment provider
        /// </summary>
        public Dictionary<string, object>? AdditionalData { get; set; }
    }
}

