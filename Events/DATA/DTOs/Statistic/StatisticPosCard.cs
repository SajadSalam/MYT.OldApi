namespace Events.DATA.DTOs.Statistic;

public class StatisticPosCard
{

    public Guid? Id { get; set; }
    
    public string? FullName { get; set; }

    public string? EventName { get; set; }

    public Guid? EventId { get; set; }

    public long? SoldTicketNumber { get; set; }
}