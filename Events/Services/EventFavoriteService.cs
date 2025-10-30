using AutoMapper;
using AutoMapper.QueryableExtensions;
using Events.DATA;
using Events.DATA.DTOs;
using Events.DATA.DTOs.Event;
using Events.Entities;
using Microsoft.EntityFrameworkCore;

namespace Events.Services;

public interface IEventFavoriteService
{
    Task<(bool? state, string? error)> AddRemoveFavorite(Guid eventId, Guid userId);
    
    Task<(List<EventDto>? events, int? totalCount, string? error)> GetAll(Guid userId , BaseFilter filter);
}

public class EventFavoriteService : IEventFavoriteService
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public EventFavoriteService(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<(bool? state, string? error)> AddRemoveFavorite(Guid eventId, Guid userId)
    {
        var favorite = await _context.EventFavorites
            .FirstOrDefaultAsync(x => x.EventId == eventId && x.UserId == userId);

        if (favorite != null)
        {
            _context.EventFavorites.Remove(favorite);
            await _context.SaveChangesAsync();
            return (false, null);
        }


        var eventFavorite = new EventFavorite
        {
            EventId = eventId,
            UserId = userId
        };

        await _context.EventFavorites.AddAsync(eventFavorite);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(List<EventDto>? events, int? totalCount, string? error)> GetAll(Guid userId , BaseFilter filter)
    {
        var events = _context.EventFavorites.AsNoTracking()
            .Include(x => x.Event)
            .Where(x => x.UserId == userId)
            .Select(x => x.Event)
            .AsQueryable();
        var totalCount = await events.CountAsync();
        var eventList = await events
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ProjectTo<EventDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
        
        
        return (eventList, totalCount, null);
    }
}