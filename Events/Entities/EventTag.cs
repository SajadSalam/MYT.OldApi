// EventTag.cs
using Events.Entities;

public class EventTag
{
    public Guid EventId { get; set; }

    public EventEntity Event { get; set; }

    public Guid TagId { get; set; }

    public Tag Tag { get; set; }
}