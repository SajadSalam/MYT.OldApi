namespace Events.Entities;

public class PointOfSale : AppUser
{
    // public Guid? ProviderId { get; set; }
    //     
    // public AppUser? Provider { get; set; }
    
    public List<string>? PhoneNumbers { get; set; }

    public string? Description { get; set; }
    
    public string? Address { get; set; }

    public string? Lat { get; set; }

    public string? Lng { get; set; }
    
    public string? Image { get; set; }

    public List<Book.Book> Books { get; set; } = new();
    
    


}