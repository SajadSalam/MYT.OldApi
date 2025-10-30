using AutoMapper;
using Events.DATA;
using Events.DATA.DTOs.Statistic;
using Events.Entities;
using Microsoft.EntityFrameworkCore;

namespace Events.Services;

public interface IStatisticService
{
    Task<(BaseResponse<StatisticDto>? data, string? error)> GetBaseCardStatistic(UserRole role, Guid userId);

    Task<(BaseResponse<StatisticEventCard>? data, string? error)> GetEventCardStatistic(Guid? eventId, UserRole role, Guid userId);

    Task<(BaseResponse<List<StatisticPosCard>>? data, string? error)> GetPosCardStatistic(Guid? eventId, UserRole role, Guid userId);

    // GetAudienceStatistic
    Task<(BaseResponse<List<EventsAudienceStatistic>>? data, string? error)> GetAudienceStatistic(Guid? eventId, UserRole role, Guid userId);

    //GetTicketsStatistic
    Task<(BaseResponse<List<TicketStatisticCard>>? data, string? error)>
        GetTicketsStatistic(Guid? posId, Guid? eventId, UserRole role, Guid userId);
}

public class StatisticService : IStatisticService
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public StatisticService(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<(BaseResponse<StatisticDto>? data, string? error)> GetBaseCardStatistic(UserRole role,
        Guid userId)
    {
        var totalComingEvents = _context.Events.Where(x => x.Deleted != true).AsNoTracking();


        var totalCompletedEvents = _context.Events.Where(x => x.Deleted != true).AsNoTracking();

        var totalAudience = _context.Tickets.Where(x => x.Deleted != true).AsNoTracking();

        var totalSoldTicket = _context.Tickets.Where(x => x.Deleted != true).AsNoTracking();


        switch (role)
        {
            case UserRole.Admin:
                break;
            case UserRole.User:
                throw new UnauthorizedAccessException();
            case UserRole.Provider:
                totalComingEvents = totalComingEvents.Where(x => x.Chart.UserId == userId);
                totalCompletedEvents = totalCompletedEvents.Where(x => x.Chart.UserId == userId);
                totalAudience = totalAudience.Where(x => x.BookObject != null && x.BookObject.Book != null && x.BookObject.Book.Event != null && x.BookObject.Book.Event.Chart.UserId == userId);
                totalSoldTicket = totalSoldTicket.Where(x => x.BookObject != null && x.BookObject.Book != null && x.BookObject.Book.Event != null && x.BookObject.Book.Event.Chart.UserId == userId);
                break;
            case UserRole.PointOfSale:
                totalComingEvents = totalComingEvents.Where(x => x.PointOfSales == null || x.PointOfSales.Any(x => x.PointOfSaleId == userId));
                totalCompletedEvents = totalCompletedEvents.Where(x => x.PointOfSales == null || x.PointOfSales.Any(x => x.PointOfSaleId == userId));
                totalAudience = totalAudience.Where(x => x.BookObject != null && x.BookObject.Book != null && x.BookObject.Book.UserId == userId);
                totalSoldTicket = totalSoldTicket.Where(x => x.BookObject != null && x.BookObject.Book != null && x.BookObject.Book.UserId == userId);
                break;
            default:
                break;
        }

        var newStatistic = new StatisticDto
        {
            TotalComingEvents =
                await totalComingEvents.CountAsync(x => x.StartEvent > DateTime.Now && x.IsPublish == true),
            TotalCompletedEvents =
                await totalCompletedEvents.CountAsync(x => x.EndEvent < DateTime.Now && x.IsPublish == true),
            TotalAudience = await totalAudience.CountAsync(x => x.IsUsed == true),
            TotalSoldTicket = await totalSoldTicket.CountAsync()
        };
        return (
            new BaseResponse<StatisticDto>
            {
                Data = newStatistic
            }
            , null);
    }

    public async Task<(BaseResponse<StatisticEventCard>? data, string? error)> GetEventCardStatistic(Guid? eventId, UserRole role, Guid userId)
    {


        var firstEvent = _context.Events.Where(x => x.Deleted != true).AsNoTracking()
            // .Where(x => x.CreationDate > DateTime.UtcNow && x.IsPublish == true)
            .OrderByDescending(x => x.CreationDate)
            .AsQueryable();

        // switch (role)
        // {
        //     case UserRole.Admin:
        //         eventId = await firstEvent.FirstOrDefaultAsync().ContinueWith(x => x.Result?.Id);
        //         break;
        //     case UserRole.User:
        //         throw new UnauthorizedAccessException();
        //         break;
        //     case UserRole.Provider:
        //         eventId = await firstEvent.Where(x => x.Chart.UserId == userId).FirstOrDefaultAsync().ContinueWith(x => x.Result?.Id);
        //         break;
        //     case UserRole.PointOfSale:
        //         eventId = await firstEvent.Where(x => x.PointOfSales == null || x.PointOfSales.Any(x => x.PointOfSaleId == userId)).FirstOrDefaultAsync().ContinueWith(x => x.Result?.Id);
        //         break;
        //     default:
        //         throw new ArgumentOutOfRangeException(nameof(role), role, null);
        // }


        var newStatistic = _context.Events.Where(x => x.Deleted != true).AsNoTracking()
            // .Where(x => x.CreationDate > DateTime.UtcNow && x.IsPublish == true)
            .Select(x => new StatisticEventCard
            {
                Name = x.Name,
                Id = x.Id,
                EventKey = x.EventKey,
                WorkspaceKey = x.Chart.WorkspaceKey,
                UserId = x.Chart.UserId
            }).AsQueryable();


        switch (role)
        {
            case UserRole.Admin:
                break;
            case UserRole.User:
                throw new UnauthorizedAccessException();
                break;
            case UserRole.Provider:
                newStatistic = newStatistic.Where(x => x.UserId == userId);
                break;
            case UserRole.PointOfSale:
                newStatistic = newStatistic.Where(x => x.Id == eventId);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(role), role, null);
        }


        if (eventId != null)
            newStatistic = newStatistic.Where(x => x.Id == eventId);


        return (
            new BaseResponse<StatisticEventCard>
            {
                Data = await newStatistic.FirstOrDefaultAsync()
            }
            , null);
    }


    public async Task<(BaseResponse<List<StatisticPosCard>>? data, string? error)> GetPosCardStatistic(Guid? eventId, UserRole role, Guid userId)
    {
        try
        {
            // eventId ??= await _context.Events.AsNoTracking()
            //     // .Where(x => x.CreationDate > DateTime.UtcNow && x.IsPublish == true)
            //     .OrderBy(x => x.CreationDate)
            //     .Select(x => x.Id).FirstAsync();

            if (eventId == Guid.Empty) return (null, null);
            var events = await _context.Events.Where(x => x.Deleted != true).FirstOrDefaultAsync(x => x.Id == eventId || x.Chart.UserId == userId);

            var books = _context.Books.AsNoTracking()
                .Include(x => x.User)
                .Include(x => x.Event)
                .Include(x => x.Objects)
                .ThenInclude(x => x.Ticket)
                .Where(x => x.Deleted == false)
                .Where(x => x.User != null && x.User.Role == UserRole.PointOfSale)
                .AsQueryable();

            if (eventId != null && eventId != Guid.Empty)
                books = books.Where(x => x.EventId == eventId);


            switch (role)
            {
                case UserRole.Admin:
                    break;
                case UserRole.User:
                    throw new UnauthorizedAccessException();
                    break;
                case UserRole.Provider:
                    books = books.Where(x => x.Event.Chart.UserId == userId);
                    break;
                case UserRole.PointOfSale:
                    books = books.Where(x => x.UserId == userId);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(role), role, null);
            }


            if (eventId != null && eventId != Guid.Empty) books = books.Where(x => x.EventId == eventId);
            if (books.Any(a => a.Event == null)) return (null, "Event null");
            var statistics = await _context.PointOfSales
            .Where(x => x.Deleted == false)
                // .GroupBy(x => x.UserId)
                .Select(group => new StatisticPosCard
                {
                    Id = group.Id,
                    FullName = group.FullName,
                    EventName = group.Books.First().Event.Name,
                    EventId = group.Books.First().Event.Id,
                    SoldTicketNumber = group.Books.Sum(y => y.Objects.Count()),
                })
                .ToListAsync();


            var onLineStatistics = await _context.Books.AsNoTracking()
                .CountAsync(x => x.User.Role != UserRole.PointOfSale && x.EventId == eventId);

            if (onLineStatistics > 0)
                statistics.Add(new StatisticPosCard
                {
                    Id = Guid.Empty,
                    FullName = "Online",
                    EventName = events?.Name,
                    EventId = events.Id,
                    SoldTicketNumber = onLineStatistics
                });

            return (
                new BaseResponse<List<StatisticPosCard>>
                {
                    Data = statistics,
                    TotalCount = statistics.Sum(x => x.SoldTicketNumber)
                }
                , null);
        }
        catch (Exception ex)
        {
            return (null, ex.Message);
        }
    }

    public async Task<(BaseResponse<List<EventsAudienceStatistic>>? data, string? error)> GetAudienceStatistic(
        Guid? eventId, UserRole role, Guid userId)
    {
        var query = _context.Events.Where(x => x.Deleted != true).AsNoTracking()
            // .Include(x => x.Objects)
            // .ThenInclude(x => x.Ticket)
            .Include(x => x.Chart)
            .Include(x => x.Books)
            .Where(x => x.Deleted == false)
            .AsQueryable();


        switch (role)
        {
            case UserRole.Admin:
                break;
            case UserRole.User:
                throw new UnauthorizedAccessException();
                break;
            case UserRole.Provider:
                query = query.Where(x => x.Chart.UserId == userId);
                break;
            case UserRole.PointOfSale:
                query = query.Where(x => x.Books.Select(s => s.UserId).Contains(userId));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(role), role, null);
        }

        if (eventId != null && eventId != Guid.Empty) query.Where(x => x.Id == eventId);


        var newStatistic = await query
            // .GroupBy(g => g.Id)
            .Select(x => new EventsAudienceStatistic
            {
                Name = x.Name,
                Count = x.Books.Sum(y => y.Objects.Count(x => x.Ticket.IsUsed == true))
            }).ToListAsync();


        return (
            new BaseResponse<List<EventsAudienceStatistic>>
            {
                Data = newStatistic,
                TotalCount = newStatistic.Sum(x => x.Count)
            }
            , null);
    }

    public async Task<(BaseResponse<List<TicketStatisticCard>>? data, string? error)> GetTicketsStatistic(Guid? posId,
        Guid? eventId, UserRole role, Guid userId)
    {
        var query = _context.Books.AsNoTracking()
            .Include(x => x.Objects)
            .ThenInclude(x => x.Ticket)
            .Include(x => x.Event)
            .AsQueryable();

        if (posId != Guid.Empty && posId != null)
            query = query.Where(x => x.UserId == posId);

        if (eventId != null && eventId != Guid.Empty)
            query = query.Where(x => x.EventId == eventId);

        var newStatistic = await query
            .GroupBy(g => g.EventId)
            .Select(x => new TicketStatisticCard
            {
                Name = x.First().Event.Name,
                SoldTicket = x.Sum(y => y.Objects.Count()),
                UsedTicket = x.Sum(y => y.Objects.Count(z => z.Ticket!.IsUsed == true))
            }).ToListAsync();


        return (
            new BaseResponse<List<TicketStatisticCard>>
            {
                Data = newStatistic,
                TotalCount = newStatistic.Sum(x => x.SoldTicket)
            }
            , null);
    }
}