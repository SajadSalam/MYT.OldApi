using System;
using AutoMapper;
using Events.DATA;
using Events.DATA.DTOs;
using Events.DATA.DTOs.Notifications;
using Events.Entities;
using Events.Helpers.OneSignal;
using Microsoft.EntityFrameworkCore;
using OneSignalApi.Model;

namespace Events.Services;

public interface INotificationService
{
    Task<(List<Notifications>? notifications, int? totalCount, string? error)> GetAll(Guid Id, BaseFilter filter);
    Task<(Notifications notifications, string? error)> SendNotifications(NotificationsForm form);
}

public class NotificationService : INotificationService
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;


    public NotificationService(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<(List<Notifications>? notifications, int? totalCount, string? error)> GetAll(Guid Id, BaseFilter filter)
    {
        var notifications = _context.Notifications.Where(w => w.NotifyId == Id || w.NotifyId==Guid.Empty);

        var totalCount = notifications.Count();

        var notificationsDto = await notifications
        .Skip(filter.PageSize * (filter.PageNumber - 1))
        .Take(filter.PageSize).ToListAsync();

        return (notificationsDto, totalCount, null);
    }

    public async Task<(Notifications notifications, string? error)> SendNotifications(NotificationsForm form)
    {
        var notification = _mapper.Map<Notifications>(form);

        var result = await _context.Notifications.AddAsync(notification);
        await _context.SaveChangesAsync();

       var state= OneSignal.SendNoitications(notification,"All");
       if(state !=true) return(null!,"Notification didn't Sent");

        return (result.Entity, null);
    }
}
