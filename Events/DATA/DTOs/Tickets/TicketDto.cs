using Events.DATA.DTOs.Book;

namespace Events.DATA.DTOs.Tickets;

public class TicketDto : BaseDto<Guid>
{
    public long Number { get; set; }

    public string? TicketSeating { get; set; }

    public string? SeatCategory { get; set; }

    public bool IsUsed { get; set; } = false;

    public string? EventName { get; set; }

    public string? EventAddress { get; set; }

    public DateTime? EventDate { get; set; }

    public bool? IsPaid { get; set; }

    public TicketStateEnum? State { get; set; }

    public Entities.Ticket.TicketTemplate? TicketTemplate { get; set; }

    public BookObjectTicketDto? BookInfo { get; set; }

    public bool? IsCanceled { get; set; }
}

public enum TicketStateEnum
{
    Canceled,
    Paid,
    UnPaid

}