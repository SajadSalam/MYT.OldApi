namespace Events.Entities;

public class BaseChart : BaseEntity<Guid>
{
    public string Name { get; set; }

    public long RelatedChartId { get; set; }

    public string ChartKey { get; set; }

    public string PublishedVersionThumbnailUrl { get; set; }
    
    public string? DraftVersionThumbnailUrl { get; set; }

    public List<BaseCategory> Categories { get; set; }

    public string? WorkspaceKey { get; set; }

    public Guid? UserId { get; set; }

    public AppUser? User { get; set; }

    public EventEntity Event { get; set; }
    public bool IsTemplate { get; set; } = false;
}