using Events.DATA.DTOs;
using Events.DATA.DTOs.PointOfSale;
using Events.DATA.DTOs.User;
using Events.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Events.Controllers;

public class PointOfSaleController : BaseController
{
    private readonly IPointOfSaleService _pointOfSaleService;

    public PointOfSaleController(IPointOfSaleService pointOfSaleService)
    {
        _pointOfSaleService = pointOfSaleService;
    }


    [Authorize(Roles = "Admin,Provider")]
    [HttpPost("/api/point-of-sale")]
    public async Task<ActionResult<UserDto>> CreatePointOfSail(CreatePointOfSaleForm createPointOfSaleForm) =>
        Ok(await _pointOfSaleService.CreatePointOfSale(Id, Role, createPointOfSaleForm));


    [HttpGet("/api/point-of-sale")]
    public async Task<ActionResult<List<PointOfSaleDto>>> GetPointOfSail([FromQuery] PointOfSaleFilter filter) =>
        Ok(await _pointOfSaleService.GetPointOfSale(filter), filter.PageNumber);
    
    // get by id
    [HttpGet("/api/point-of-sale/{id}")]   
    public async Task<ActionResult<PointOfSaleDto>> GetPointOfSailById(Guid id) =>
        Ok(await _pointOfSaleService.GetPointOfSaleById(id));

    // update 
    [Authorize(Roles = "Admin")]
    [HttpPut("/api/point-of-sale/{id}")]
    public async Task<ActionResult<PointOfSaleDto>> UpdatePointOfSail(UpdatePointOfSaleForm updatePointOfSaleForm,
        Guid id) =>
        Ok(await _pointOfSaleService.UpdatePointOfSale(id, updatePointOfSaleForm));

    [Authorize(Roles = "Admin")]
    [HttpDelete("/api/point-of-sale/{id}")]
    public async Task<ActionResult<PointOfSaleDto>> DeletePointOfSail(Guid id) =>
        Ok(await _pointOfSaleService.DeletePointOfSale(id));
}