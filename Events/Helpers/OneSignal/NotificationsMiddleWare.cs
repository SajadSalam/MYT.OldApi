using Events.DATA;
using Microsoft.EntityFrameworkCore;

namespace Events.Helpers.OneSignal
{
    public class NotificationsMiddleware : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private Timer _timer;

        public NotificationsMiddleware(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var now = DateTime.Now;
            var nextMidnight = DateTime.Today.AddDays(1).AddHours(0);
            var initialDelay = nextMidnight - now;

            Console.WriteLine($"Date Of Now : {now.TimeOfDay}");
            Console.WriteLine($"Date Of repeat : {initialDelay}");

            _timer = new Timer(ExecuteTask, null, initialDelay, TimeSpan.FromHours(24));

            return Task.CompletedTask;
        }

        private async void ExecuteTask(object? state) // Change the parameter to object? (nullable)
        {
            using var scope = _serviceProvider.CreateScope();
            var _context = scope.ServiceProvider.GetRequiredService<DataContext>();

            var EventsBook = await _context.Books
                .Include(i => i.Event)
                .Include(i => i.User)
                .Where(w => w.Event!.StartEvent.Value.Day == DateTime.UtcNow.Day)
                .Select(s => new
                {
                    userId = s.UserId.ToString(),
                    EventName = s.Event!.Name,
                    EventId = s.EventId
                })
                .GroupBy(g => g.EventId)
                .ToArrayAsync(); // Change to asynchronous call

            foreach (var eventGroup in EventsBook)
            {
                foreach (var entity in eventGroup)
                {
                    var notification = new Entities.Notifications
                    {
                        Title = "حجوزات اليوم",
                        Description=$"لديك حجز في {entity.EventName}",
                        NotifyId = Guid.Parse(entity.userId!)
                    };

                    await _context.Notifications.AddAsync(notification);
                    await _context.SaveChangesAsync();

                    // var userIdsToSend = eventGroup.Select(s => s.userId!.ToString()).ToArray();

                    OneSignal.SendNoitications(notification, entity.userId!);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }
    }
}
