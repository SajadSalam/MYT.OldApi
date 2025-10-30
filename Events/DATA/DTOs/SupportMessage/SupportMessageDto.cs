using System;

namespace Events.DATA.DTOs.SupportMessage;

public class SupportMessageDto:BaseDto<Guid>
{
    public string? PhoneNumber { get; set; }
    public string? FullName { get; set; }
    public string? Message { get; set; }

}
