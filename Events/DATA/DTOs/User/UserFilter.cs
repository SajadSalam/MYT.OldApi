using Events.Entities;

namespace Events.DATA.DTOs.User;

public class UserFilter : BaseFilter
{
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public UserRole? Role { get; set; }
}