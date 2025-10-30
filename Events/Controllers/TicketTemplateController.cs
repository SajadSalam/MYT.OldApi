using Events.DATA.DTOs;
using Events.DATA.DTOs.Tickets.TicketTemplate;
using Events.Entities.Ticket;
using Events.Services;
using Microsoft.AspNetCore.Mvc;

namespace Events.Controllers;

public class TicketTemplateController : BaseController
{

    private readonly ITicketTemplateService _ticketTemplateService;

    public TicketTemplateController(ITicketTemplateService ticketTemplateService)
    {
        _ticketTemplateService = ticketTemplateService;
    }
    
    [HttpPost("/api/ticket-templates")]
    public async Task<IActionResult> Create([FromBody] TicketTemplateForm ticketTemplateForm) => Ok(await _ticketTemplateService.CreateTicketTemplateAsync(ticketTemplateForm));
    
    [HttpGet("/api/ticket-templates/{id}")]
    public async Task<IActionResult> Get(Guid id) => Ok(await _ticketTemplateService.GetTicketTemplateAsync(id));
    
    [HttpGet("/api/ticket-templates/event/{eventId}")]
    public async Task<ActionResult<TicketTemplateDto>> GetByEvent(Guid eventId) => Ok(await _ticketTemplateService.GetByEventAsync(eventId));
    
    
    [HttpGet("/api/ticket-templates")]
    public async Task<IActionResult> GetAll([FromQuery] BaseFilter filter) => Ok(await _ticketTemplateService.GetAllTicketTemplateAsync(filter) , filter.PageNumber);
    
  
    // [HttpPut("/api/ticket-templates/{id}")]
    // public async Task<IActionResult> Update(Guid id, [FromBody] TicketTemplateForm ticketTemplateForm) => Ok(await _ticketTemplateService.UpdateTicketTemplateAsync(id, ticketTemplateForm));
    //
    [HttpDelete("/api/ticket-templates/{id}")]
    public async Task<IActionResult> Delete(Guid id) => Ok(await _ticketTemplateService.DeleteTicketTemplateAsync(id));
    
}