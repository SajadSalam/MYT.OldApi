namespace Events.Entities;

public class BaseCategory : BaseEntity<Guid>
{
    public string Name { get; set; }

    public string Color { get; set; }

    public decimal Price { get; set; }
    
    public Guid ChartId { get; set; }
    public BaseChart Chart { get; set; }
}