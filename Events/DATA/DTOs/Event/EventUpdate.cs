namespace Events.DATA.DTOs.Event;

public class EventUpdate
{
    public string? Name { get; set; }
    
    public DateTime? StartEvent { get; set; }

    public DateTime? EndEvent { get; set; }
    
    public string? Description { get; set; }
    
    public List<string>? Attachments { get; set; } = new();

    public Guid? ChartId { get; set; } = null;
    
    public DateTime? StartReservationDate { get; set; }
    
    public DateTime? EndReservationDate { get; set; }
    
    public double? Lat { get; set; }
    
    public double? Lng { get; set; }
    
    public string? Address { get; set; }

    public int? SeatAvailable { get; set; }
    
    public int? SeatBooked { get; set; }
    
    public List<Guid>? PointOfSales { get; set; }
    
    public List<Guid>? TagIds { get; set; }
    
    public bool? IsFeature { get; set; }
}