using OneSignalApi.Model;
using SeatsioDotNet.Events;

namespace Events.Entities;

public class EventFavorite : BaseEntity<Guid>
{
    public Guid? UserId { get; set; }
    public AppUser? User { get; set; }
    
    public Guid? EventId { get; set; }
    public EventEntity? Event { get; set; }
}