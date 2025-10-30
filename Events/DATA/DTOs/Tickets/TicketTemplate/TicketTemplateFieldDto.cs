using Events.Entities.Ticket;

namespace Events.DATA.DTOs.Tickets.TicketTemplate;

public class TicketTemplateFieldDto : BaseDto<Guid>
{
    public string? Label { get; set; }
    public string? Key { get; set; }
    public TicketFieldType? Type { get; set; }
    public TextAlignment? Alignment { get; set; }
    public string? FontSize { get; set; }
    public FontWeight? FontWeight { get; set; }
    public string? Color { get; set; }
    public string? X { get; set; }
    public string? Y { get; set; }
    public string? Width { get; set; }
    public string? Height { get; set; }
    public string? ScaleX { get; set; }
    public string? ScaleY { get; set; }
    public string? Value { get; set; }
    public bool? Required { get; set; }
    public string? ImageURL { get; set; }
    public TextVerticalAlignment? VerticalAlign { get; set; }
    public string? Rotation { get; set; }
}