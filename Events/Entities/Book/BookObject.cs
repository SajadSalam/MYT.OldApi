using System.ComponentModel.DataAnnotations.Schema;

namespace Events.Entities.Book;

public class BookObject : BaseEntity<Guid>
{
    
    public string? FullName { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Name { get; set; }

    public decimal Price { get; set; }

    public Guid? CategoryId { get; set; }
    
    public BaseCategory? Category { get; set; }

    public string? Type { get; set; } // table - seat 

    public Guid BookId { get; set; }
    
    public Book? Book { get; set; }

    // public Guid? TicketId { get; set; }
    //
    // [ForeignKey(nameof(TicketId))]
    public Ticket.Ticket? Ticket { get; set; }
    
    public BookHoldInfo? BookHoldInfo { get; set; }


    public bool IsCanceled { get; set; } = false;



}