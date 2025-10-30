using Events.DATA.DTOs.Category;
using Events.DATA.DTOs.Tickets;

namespace Events.DATA.DTOs.Book;

public class BookObjectTicketDto
{
    public string? FullName { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Name { get; set; }

    public decimal Price { get; set; }
    
    public CategoryDto? Category { get; set; }

    public string? Type { get; set; }
    
}