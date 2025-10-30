using Events.DATA;
using Microsoft.EntityFrameworkCore;
using SeatsioDotNet;
using SeatsioDotNet.Events;

namespace Events.Helpers.Hosted;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using OneSignalApi.Model;
using Events.Entities;

public class BookTimeoutNotification : IHostedService, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private Timer _timer;

    public BookTimeoutNotification(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(25));

        return Task.CompletedTask;
    }

    private async void DoWork(object state)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();

        var almostExpiredBooks = await dbContext.Books.AsNoTracking().Where(
                    x => x.BookHoldInfo != null &&
                         x.CreationDate!.Value.AddMinutes(x.BookHoldInfo.ExpiredMinutes) < DateTime.UtcNow.AddMinutes(1) &&
                          x.CreationDate!.Value.AddMinutes(x.BookHoldInfo.ExpiredMinutes) >= DateTime.UtcNow &&
                         x.IsPaid == false
                ).ToListAsync();

        if (!almostExpiredBooks.IsNullOrEmpty())
        {
            var notification = new Notifications
            {
                Title = "تنبيه ",
                Description = "وقت الحجز سينتهي بعد اقل من دقيقة واحدة"
            };
            almostExpiredBooks.ForEach(f =>
            {
                OneSignal.OneSignal.SendNoitications(notification, f.UserId.ToString()!);
            });

        }

    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}