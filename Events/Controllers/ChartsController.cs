using Events.DATA.DTOs;
using Events.DATA.DTOs.Chart;
using Events.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Events.Controllers;

[Authorize]
public class ChartsController : BaseController
{
    private readonly IChartService _chartService;

    public ChartsController(IChartService chartService)
    {
        _chartService = chartService;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateChart([FromBody] ChartForm form) => Ok(await _chartService. CreateChartAsync(form , Id));
    
    
    [Authorize(Roles = "Admin,Provider")]
    [HttpGet]
    public async Task<ActionResult<BaseDtoWithoutPagination<ChartDto>>> GetChart([FromQuery] ChartFilter filter) => Ok(await _chartService.GetChartAsync(filter,Id));

    [Authorize(Roles = "Admin,Provider")]
    [HttpGet("{id}")]
    public async Task<ActionResult<ChartDto>> GetChartById(Guid id) => Ok( await _chartService.GetChartByIdAsync(id, Id));
   
    
    // delete chart
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteChart(Guid id) => Ok(await _chartService.DeleteChartAsync(id));
    
    
    
}