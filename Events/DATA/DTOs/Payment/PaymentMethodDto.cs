using Events.Entities;

namespace Events.DATA.DTOs.Payment
{
    /// <summary>
    /// Information about available payment methods
    /// </summary>
    public class PaymentMethodDto
    {
        public PaymentProvider Provider { get; set; }
        
        public string Name { get; set; }
        
        public string DisplayName { get; set; }
        
        public bool IsEnabled { get; set; }
        
        public string? Description { get; set; }
        
        public string? IconUrl { get; set; }
    }
}

