// EventEntity.cs
using Events.Entities.Ticket;

namespace Events.Entities;

public class EventEntity : BaseEntity<Guid>
{
    public string? Name { get; set; }

    public DateTime? StartEvent { get; set; }

    public DateTime? EndEvent { get; set; }

    public string? Description { get; set; }

    public List<string>? Attachments { get; set; } = new();

    public Guid? ChartId { get; set; }

    public BaseChart? Chart { get; set; }

    public string? EventKey { get; set; }

    public string? SlugHash { get; set; }

    public bool? IsPublish { get; set; } = false;

    public List<TicketTemplate>? TicketTemplates { get; set; } = new();

    public DateTime? StartReservationDate { get; set; }

    public DateTime? EndReservationDate { get; set; }

    public List<Book.Book>? Books { get; set; } = new();

    public int? UsedTickets { get; set; } = 0;

    public double? Lat { get; set; }

    public double? Lng { get; set; }

    public string? Address { get; set; }

    public int? SeatAvailable { get; set; }

    public int? SeatBooked { get; set; }

    public List<EventPointOfSale>? PointOfSales { get; set; } = new();

    public List<EventTag>? EventTags { get; set; } = new(); // Updated navigation property


    public bool? IsFeature { get; set; } = false;
}