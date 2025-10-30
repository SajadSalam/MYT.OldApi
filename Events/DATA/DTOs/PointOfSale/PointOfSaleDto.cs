namespace Events.DATA.DTOs.PointOfSale;

public class PointOfSaleDto : BaseDto<Guid>
{
    
    public List<string>? PhoneNumbers { get; set; }

    public string? Description { get; set; }
    
    public string? Address { get; set; }

    public string? Lat { get; set; }

    public string? Lng { get; set; }
    
    public string? Image { get; set; }
    
    public string? FullName { get; set; }
        
    public string? PhoneNumber { get; set; }

    public int? TotalSoldTicket { get; set; }

    public decimal? IncomeAmount { get; set; }
    
}