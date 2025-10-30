namespace Events.DATA.DTOs.Statistic;

public class StatisticEventCard
{
    public string? Name { get; set; }

    public Guid? Id { get; set; }

    public string? EventKey { get; set; }

    public string? WorkspaceKey { get; set; }
    public Guid
    ? UserId { get; set; }
}