using Events.DATA.DTOs;
using Events.Services;
using Microsoft.AspNetCore.Mvc;

namespace Events.Controllers;

public class EventFavoriteController : BaseController
{
    
    private readonly IEventFavoriteService _eventFavoriteService;

    public EventFavoriteController(IEventFavoriteService eventFavoriteService)
    {
        _eventFavoriteService = eventFavoriteService;
    }

    // add reomve favorite
    [HttpPost("/api/favorite/{eventId}")]
    public async Task<IActionResult> ToggleFavorite(Guid eventId) => Ok(await _eventFavoriteService.AddRemoveFavorite(eventId, Id));
    

    // get all favorite
    [HttpGet("/api/favorite")]
    public async Task<IActionResult> GetAllFavorite([FromQuery]BaseFilter filter) => Ok(await _eventFavoriteService.GetAll(Id , filter) , filter.PageNumber);
}