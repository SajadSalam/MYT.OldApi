using System.ComponentModel.DataAnnotations;

namespace Events.DATA.DTOs.PointOfSale;

public class CreatePointOfSaleForm
{
    [Required]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string? Password { get; set; }
        
    [Required]
    public string? PhoneNumber { get; set; }
        
    [Required]
    [MinLength(2, ErrorMessage = "FullName must be at least 2 characters")]
    public string? FullName { get; set; }
    
    public List<string>? PhoneNumbers { get; set; }

    public string? Description { get; set; }
    
    public string? Address { get; set; }

    public string? Lat { get; set; }

    public string? Lng { get; set; }
    
    public string? Image { get; set; }


}