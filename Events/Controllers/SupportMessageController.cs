using System;
using Events.DATA.DTOs.SupportMessage;
using Events.Services;
using Events.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Events.Controllers;

public class SupportMessageController : BaseController
{
    private readonly ISupportMessageService _service;

    public SupportMessageController(ISupportMessageService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SupportMessageForm form) => Ok(await _service.Create(form));


    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromBody] SupportMessageUpdate form, Guid id) => Ok(await _service.Update(form, id));


    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(Guid id) => Ok(await _service.GetById(id));

    [HttpGet]
    public async Task<ActionResult<SupportMessageDto>> GetAll([FromQuery]SupportMessageFilter filter) => Ok(await _service.GetAll(filter),filter.PageNumber);


    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteChart(Guid id) => Ok(await _service.Delete(id));




}
