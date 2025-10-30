using System;
using Events.DATA.DTOs;
using Events.DATA.DTOs.Notifications;
using Events.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Events.Controllers;

public class NotificationsController:BaseController
{
    private readonly INotificationService _service;

    public NotificationsController(INotificationService service)
    {
        _service = service;
    }
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery]BaseFilter filter) => Ok(await _service.GetAll(Id, filter) , filter.PageNumber);

    [HttpPost]
    public async Task<IActionResult> SendNotification([FromBody]NotificationsForm form) => Ok(await _service.SendNotifications(form));
  

}
