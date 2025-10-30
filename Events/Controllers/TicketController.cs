using Events.DATA.DTOs.Tickets;
using Events.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Events.Controllers;

public class TicketController : BaseController
{
    
    private readonly ITicketService _ticketService;
    
    public TicketController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }
    
    //
    // [HttpPost("release-ticket")]
    // public async Task<IActionResult> ReleaseTicketAsync([FromBody] Guid bookObjectId)
    // {
    //     var (ticket, error) = await _ticketService.ReleaseTicketAsync(bookObjectId);
    //     if (error != null) return BadRequest(new {error});
    //     return Ok(ticket);
    // }

    // TODO: in dashboard tickets return as empty List I couldn't find the problem if it was frontend or backend Issue 
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetTicketsAsync([FromQuery] TicketFilter filter) =>
        Ok(await _ticketService.GetTicketsAsync(Role , Id , filter) , filter.PageNumber);
    
    [Authorize]
    [HttpGet("{number}")]
    public async Task<IActionResult> GetTicketAsync(long number)
    {
        var (ticket, error) = await _ticketService.GetTicketAsync(number);
        if (error != null) return BadRequest(new {error});
        return Ok(ticket);
    }
    
    // change is used
    // [Authorize]
    [HttpPut("change-used-state/{ticketNumber:long}")]
    public async Task<IActionResult> ChangeIsUsedAsync(long ticketNumber) => Ok(await _ticketService.ChangeIsUsedAsync(ticketNumber));
    
    
    // ticket cancel
    [HttpPut("cancel-ticket")]
    public async Task<IActionResult> CancelTicketAsync([FromBody] ChangeTicketState changeTicketState) => Ok(await _ticketService.CancelTicketAsync(changeTicketState));
    
    
    
}