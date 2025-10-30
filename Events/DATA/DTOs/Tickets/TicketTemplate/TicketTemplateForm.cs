using System.ComponentModel.DataAnnotations;
using Events.Entities.Ticket;

namespace Events.DATA.DTOs.Tickets.TicketTemplate;

public class TicketTemplateForm
{
    [Required] public Guid EventId { get; set; }
    
    [Required] public string Image { get; set; }
    
    
    public TicketTemplateStage? StageDetails { get; set; }

    public List<TicketTemplateField> Fields { get; set; } = new();

  

}