namespace Events.Entities.Book;

public class Book : BaseEntity<Guid>
{
    public Guid? EventId { get; set; }

    public EventEntity? Event { get; set; }

    public decimal TotalPrice { get; set; }

    public List<BookObject> Objects { get; set; } = new();

    public bool? IsPaid { get; set; } = false;

    public Guid? UserId { get; set; }
    public AppUser? User { get; set; }
    
    // book hold info
    public BookHoldInfo? BookHoldInfo { get; set; }
    
    public decimal? Discount { get; set; } = 0;
    public Bill? Bill { get; set; }

}