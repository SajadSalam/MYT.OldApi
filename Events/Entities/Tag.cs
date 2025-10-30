// Tag.cs
namespace Events.Entities;

public class Tag : BaseEntity<Guid>
{
    public string? Name { get; set; }

    public string? Image { get; set; }
    public List<EventTag> EventTags { get; set; } = new(); // Updated navigation property
}