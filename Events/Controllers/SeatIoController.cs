using Events.Services;
using Microsoft.AspNetCore.Mvc;

namespace Events.Controllers;

public class SeatIoController : BaseController
{
    
    private readonly ISeatIoService _seatIoService;

    public SeatIoController(ISeatIoService seatIoService)
    {
        _seatIoService = seatIoService;
    }
    
    [HttpGet("charts")]
    public async Task<IActionResult> GetCharts() => Ok(await _seatIoService.GetChartAsync() , 1);
    
    [HttpGet("categories/{key}")]
    public async Task<IActionResult> GetCategories(string key) => Ok(await _seatIoService.GetCategoryByChartKeyAsync(key) , 1);
    
    // [HttpPost("charts")]
    // public async Task<IActionResult> CreateChart([FromBody] string name) => Ok(await _seatIoService.CreateChartAsync(name));
    //
    // [HttpPost("events")]
    // public async Task<IActionResult> CreateEvent([FromBody] string chartKey) => Ok(await _seatIoService.CreateEventAsync(chartKey));
    
    
    [HttpGet("events/{name}")]
    public async Task<IActionResult> RetrieveEvent(string name) => Ok(await _seatIoService.RetrieveEventAsync(name , ""));
    
   
    // Retrieve Object
    [HttpPost("objects")]
    public async Task<IActionResult> RetrieveObject(List<string> objs) => Ok(await _seatIoService.RetrieveObjectAsync(objs));
        
    
}