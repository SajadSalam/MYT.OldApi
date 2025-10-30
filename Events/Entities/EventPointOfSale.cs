namespace Events.Entities;

public class EventPointOfSale : BaseEntity<Guid>
{
    public Guid? EventId { get; set; }
    public EventEntity? Event { get; set; }
    
    public Guid? PointOfSaleId { get; set; }
    public PointOfSale? PointOfSale { get; set; }
    
}