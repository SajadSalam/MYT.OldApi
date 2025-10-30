namespace Events.DATA.DTOs.Event;

public class EventFilter : BaseFilter
{
    public string? Name { get; set; }

    public EventFilterState? State { get; set; }

    public Guid? Tag { get; set; }

    public double? Lat { get; set; }
    
    public double? Lng { get; set; }

    public double? Distance { get; set; } = 10;
    
    public bool? IsFeature { get; set; }



}

public enum EventFilterState
{
    Coming,
    Ended,
    NotPosted
}