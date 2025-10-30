namespace Events.DATA.DTOs.PointOfSale;

public class UpdatePointOfSaleForm
{
  
    public string? Password { get; set; }
        
    public string? PhoneNumber { get; set; }
        
    public string? FullName { get; set; }
    
    public List<string>? PhoneNumbers { get; set; }

    public string? Description { get; set; }
    
    public string? Address { get; set; }

    public string? Lat { get; set; }

    public string? Lng { get; set; }
    
    public string? Image { get; set; }
}