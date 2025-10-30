

namespace Events.Entities
{
    public class AppUser : BaseEntity<Guid>
    {
        public string? FullName { get; set; }
        
        public string? PhoneNumber { get; set; }
        
        public string? Password { get; set; }
        
        public string? WorkspacePublicKey { get; set; }
        
        public string? WorkspaceSecretKey { get; set; }
        
        public UserRole Role { get; set; }

        public List<Book.Book> Books { get; set; } = new();
    }
    
    public enum UserRole
    {
        Admin,
        User,
        Provider ,
        PointOfSale
    }
    
}