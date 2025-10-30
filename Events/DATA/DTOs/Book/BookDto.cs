using Events.DATA.DTOs.Event;
using Events.DATA.DTOs.User;
using Events.Entities.Book;

namespace Events.DATA.DTOs.Book
{
    public class BookDto : BaseDto<Guid>
    {
        public Guid? EventId { get; set; }

        public EventDto? Event { get; set; }

        public decimal TotalPrice { get; set; }
        public decimal? TotalPriceAfterDiscount { get; set; }

        public List<BookObjectDto> Objects { get; set; }

        public bool? IsPaid { get; set; } = false;

        public Guid? UserId { get; set; }

        public UserDto? User { get; set; }

        public string? InvoiceNumber  { get; set; }
        
        public decimal? Discount { get; set; }


    }
}