using Events.Entities.Ticket;

namespace Events.DATA.DTOs.Tickets.TicketTemplate;

public class TicketTemplateDto : BaseDto<Guid>
{
    public string? Image { get; set; }
    
    public TicketTemplateStage? StageDetails { get; set; }
    
    public List<TicketTemplateField> Fields { get; set; } = new();
    
}