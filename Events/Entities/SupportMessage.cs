using System;

namespace Events.Entities;

public class SupportMessage : BaseEntity<Guid>
{
    public string? PhoneNumber { get; set; }
    public string? FullName { get; set; }
    public string? Message { get; set; }

}
