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
    }
    
    
    public enum PaymentStatus
    {
        NotPaid = 0,
        Paid = 1
    }
}