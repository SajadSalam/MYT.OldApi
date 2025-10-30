using Events.DATA.DTOs.Event;
using Events.DATA.DTOs.Tag;
using Events.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Events.Controllers;

// [Authorize]
public class EventController : BaseController
{
    private readonly IEventService _eventService;

    public EventController(IEventService eventService)
    {
        _eventService = eventService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateEvent([FromBody] EventForm eventForm) =>
        Ok(await _eventService.CreateEvent(eventForm));

    [HttpGet("{id}")]
    public async Task<ActionResult<EventDto>> GetEventById(Guid id) => Ok(await _eventService.GetEventById(id , Id));
    
    [HttpGet("/api/event/hash/{hash}")]
    public async Task<ActionResult<EventDto>> GetEventByHash(string hash) => Ok(await _eventService.GetEventByHash(hash, Id));

    
    [HttpGet]
    public async Task<ActionResult<EventDto>> GetAllEvents([FromQuery]EventFilter filter) => Ok(await _eventService.GetAllEvents(Id , filter), filter.PageNumber);

    
    // update event 
    [Authorize(Roles = "Admin,Provider")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEvent(Guid id, [FromBody] EventUpdate eventUpdate) =>
        Ok(await _eventService.UpdateEvent(id, eventUpdate));
    
    [HttpPut("/api/event/{id}/publish")]
    public async Task<IActionResult> UpdatePublishState(Guid id, [FromBody] PublishStateForm state) =>
        Ok(await _eventService.UpdatePublishState(id, state));
    
    
    // delete event
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEvent(Guid id) => Ok(await _eventService.DeleteEvent(id));
    
    // add point of sale to event
    [HttpPost("/api/event/{eventId}/point-of-sale/{pointOfSaleId}")]
    public async Task<IActionResult> AddPointOfSaleToEvent(Guid eventId, Guid pointOfSaleId) =>
        Ok(await _eventService.AddPointOfSale(eventId, pointOfSaleId));
    
    // AddTagToEvent
    [HttpPost("/api/event/{eventId}/tag")]
    public async Task<IActionResult> AddTagToEvent(Guid eventId, [FromBody] AddTagToEventForm form) =>
        Ok(await _eventService.AddTagToEvent(eventId, form));
    
    // // change feature state
    [HttpPut("/api/event/{eventId}/feature")]
    public async Task<IActionResult> ChangeFeatureState(Guid eventId, [FromBody] EventFeatureStateForm form) =>
        Ok(await _eventService.ChangeFeatureState(eventId, form));
    
    
    

}