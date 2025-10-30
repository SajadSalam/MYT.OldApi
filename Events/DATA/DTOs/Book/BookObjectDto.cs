using Events.DATA.DTOs.Category;
using Events.DATA.DTOs.Tickets;
using Events.Entities.Book;

namespace Events.DATA.DTOs.Book;

public class BookObjectDto : BaseDto<Guid>
{
    public string? FullName { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Name { get; set; }

    public decimal Price { get; set; }

    public Guid? CategoryId { get; set; }
    
    public CategoryDto? Category { get; set; }

    public string? Type { get; set; }

    public Guid? BookId { get; set; }
    
    public BookDto? Book { get; set; }

    public Guid? TicketId { get; set; }
    
    public TicketDto Ticket { get; set; }
    
    public BookHoldInfo? BookHoldInfo { get; set; }
    
    public bool IsCanceled { get; set; } 
    

    public string? ExpiredTime { get; set; }
}