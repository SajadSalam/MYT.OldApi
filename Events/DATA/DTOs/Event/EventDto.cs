using Events.DATA.DTOs.Category;
using Events.DATA.DTOs.PointOfSale;
using Events.DATA.DTOs.Tag;
using Events.DATA.DTOs.Tickets.TicketTemplate;
using SeatsioDotNet.Charts;

namespace Events.DATA.DTOs.Event;

public class EventDto : BaseDto<Guid>
{
    public string Name { get; set; }

    public DateTime? StartEvent { get; set; }

    public DateTime? EndEvent { get; set; }

    public string? Description { get; set; }

    public List<string> Attachments { get; set; } = new();

    public Guid? ChartId { get; set; }

    public SeatsioDotNet.Events.Event? Event { get; set; }

    public string? EventKey { get; set; }
    
    public bool? IsPublish { get; set; }
    
    public string? SlugHash { get; set; }

    public List<TicketTemplateDto>? TicketTemplate { get; set; }
    
    public DateTime? StartReservationDate { get; set; }
    
    public DateTime? EndReservationDate { get; set; }

    public string? WorkspaceKey { get; set; }
    
    public string? SecretKey { get; set; }

    public List<CategoryDto>? Categories { get; set; }
    
    public double? Lat { get; set; }
    
    public double? Lng { get; set; }
    
    public string? Address { get; set; }
    
    public int? SeatAvailable { get; set; }
    
    public int? SeatBooked { get; set; }
    
    public List<PointOfSaleDto>? PointOfSales { get; set; }
    
    
    public EventFilterState? State { get; set; }

    public bool? IsFavorite { get; set; }    
    
    public List<TagDto>? Tags { get; set; }
    
    public bool IsFeature { get; set; }

    public int? NumberOfAttendance { get; set; }



}