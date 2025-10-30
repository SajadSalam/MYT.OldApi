using System.ComponentModel.DataAnnotations.Schema;
using Events.Entities.Book;

namespace Events.Entities.Ticket;

public class Ticket : BaseEntity<Guid>
{
   
    public long Number { get; set; }
    
    public string? TicketSeating { get; set; }
    
    public string? SeatCategory { get; set; }
    
    public Guid? BookObjectId { get; set; }
    public BookObject? BookObject { get; set; }

    public bool IsUsed { get; set; } = false;

    
    
}

