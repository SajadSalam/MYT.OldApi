using Events.Entities;

namespace Events.DATA.DTOs.Payment
{
    /// <summary>
    /// Response containing payment status information
    /// </summary>
    public class PaymentStatusResponse
    {
        public bool IsSuccess { get; set; }
        
        public PaymentStatus Status { get; set; }
        
        public string? Error { get; set; }
        
        public string? PaymentId { get; set; }
        
        public decimal? Amount { get; set; }
        
        public DateTime? PaymentDate { get; set; }
        
        public Dictionary<string, object>? AdditionalData { get; set; }
    }
}

