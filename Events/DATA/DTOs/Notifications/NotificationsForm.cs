using System;

namespace Events.DATA.DTOs.Notifications;

public class NotificationsForm
{
    public required string Title { get; set; }
    public string? Description { get; set; }
}
