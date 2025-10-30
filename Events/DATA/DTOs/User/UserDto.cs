using Events.Entities;

namespace Events.DATA.DTOs.User
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Token { get; set; }
        
        public string? WorkspacePublicKey { get; set; }
        
        public string? WorkspaceSecretKey { get; set; }
        
        public string? Role { get; set; }
        
    }
}