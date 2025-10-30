namespace Events.DATA.DTOs.Tickets;

public class TicketFilter : BaseFilter
{
    public string? Name { get; set; }

    public Guid? EventId { get; set; }

    public bool? IsPaid { get; set; }

    public bool? IsUsed { get; set; }
    public TicketStateEnum? State { get; set; }
    public Guid? bookId { get; set; }
}