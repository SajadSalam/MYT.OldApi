using Events.DATA;
using Microsoft.EntityFrameworkCore;
using SeatsioDotNet;
using SeatsioDotNet.Events;
using Microsoft.Extensions.Configuration;

namespace Events.Helpers.Hosted;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using OneSignalApi.Model;
using Events.Entities;

public class BookCleanupService : IHostedService, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private Timer _timer;

    public BookCleanupService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(20));

        return Task.CompletedTask;
    }

    private async void DoWork(object state)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var books =await dbContext.Books.Where(
                      x => x.BookHoldInfo != null &&
                           x.CreationDate!.Value.AddMinutes(x.BookHoldInfo.ExpiredMinutes) < DateTime.UtcNow &&
                           x.IsPaid == false
                  ).Include(book => book.Objects)
                  .Include(x => x.Event)
                  .ToListAsync();

        if (books.Count == 0)
        {
            return;
        }
        
        var seatIoSecretKey = configuration["SeatIO:SecretKey"] ?? throw new InvalidOperationException("SeatIO:SecretKey is not configured");
        var client = new SeatsioClient(Region.EU(), seatIoSecretKey);

        books.ForEach(b =>
            {
                var objects = b.Objects.Select(x => x.Name).ToArray();
                client.Events.ChangeObjectStatusAsync(new[]
                {
                    new StatusChangeRequest(b.Event!.EventKey, objects, "free")
                });
            }
        );

        await dbContext.Books.Where(x => books.Select(b => b.Id).ToList().Contains(x.Id)).ExecuteDeleteAsync();
        await dbContext.SaveChangesAsync();
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