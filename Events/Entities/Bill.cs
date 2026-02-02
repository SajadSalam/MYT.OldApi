using System.ComponentModel.DataAnnotations.Schema;

namespace Events.Entities
{
    public class Bill : BaseEntity<Guid>
    {
        public Guid? BookId { get; set; }
        [ForeignKey(nameof(BookId))]
        public Book.Book? Book { get; set; }
        public decimal? TotalPrice { get; set; }
        
        public string? BillId { get; set; }
        public PaymentStatus? PaymentStatus { get; set; }

        public DateTime? PaymentDate { get; set; }
        
        // New fields for multi-provider support
        public PaymentProvider? PaymentProvider { get; set; }
        
        public string? PaymentProviderId { get; set; }
        
        /// <summary>
        /// JSON field for storing provider-specific metadata
        /// </summary>
        public string? PaymentMetadata { get; set; }
    }
    
    
    public enum PaymentStatus
    {
        NotPaid = 0,
        Paid = 1,
        Canceled = 2,
        Refunded = 3,
        Failed = 4
    }
    
    public enum PaymentProvider
    {
        Amwal = 0,
        Cash = 3
    }
}