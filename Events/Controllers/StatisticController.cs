using Events.DATA.DTOs.Statistic;
using Events.Services;
using Events.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Events.Controllers;

public class StatisticController : BaseController
{
    
    private readonly  IStatisticService _statisticService;
    
    public StatisticController(IStatisticService statisticService)
    {
        _statisticService = statisticService;
    }
    
    //GetBaseCardStatistic
    [Authorize]
    [HttpGet("base-cards")]
    public async Task<ActionResult<StatisticDto>> GetBaseCardStatistic() => Ok(await _statisticService.GetBaseCardStatistic(Role , Id));
    
    //GetEventCardStatistic
    [Authorize]
    [HttpGet("event-cards")]
    public async Task<ActionResult<StatisticEventCard>> GetEventCardStatistic([FromQuery]Guid? eventId) => Ok(await _statisticService.GetEventCardStatistic(eventId,  Role , Id));
    
    //GetPosCardStatistic
    [Authorize]
    [HttpGet("pos-cards")]
    public async Task<ActionResult<List<StatisticPosCard>>> GetPosCardStatistic([FromQuery]Guid? eventId) => Ok(await _statisticService.GetPosCardStatistic(eventId , Role , Id ));
    
    // GetAudienceStatistic
    [Authorize]
    [HttpGet("audience")]
    public async Task<ActionResult<Respons<EventsAudienceStatistic>>> GetAudienceStatistic([FromQuery]Guid eventId) => Ok(await _statisticService.GetAudienceStatistic(eventId , Role , Id));
    
    //GetTicketsStatistic
    [Authorize]
    [HttpGet("tickets")]
    public async Task<ActionResult<Respons<TicketStatisticCard>>> GetTicketsStatistic([FromQuery]Guid posId, [FromQuery] Guid eventId) => Ok(await _statisticService.GetTicketsStatistic(posId, eventId , Role , Id));


}