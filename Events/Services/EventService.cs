using AutoMapper;
using AutoMapper.QueryableExtensions;
using Events.DATA;
using Events.DATA.DTOs.Category;
using Events.DATA.DTOs.Event;
using Events.DATA.DTOs.PointOfSale;
using Events.DATA.DTOs.Tag;
using Events.Entities;
using Events.Helpers;
using Microsoft.EntityFrameworkCore;
using SeatsioDotNet.Charts;
using SeatsioDotNet.Events;

namespace Events.Services;

public interface IEventService
{
    Task<(EventDto? events, string error)> CreateEvent(EventForm eventForm);

    Task<(EventDto? events, string error)> GetEventById(Guid id, Guid userId);

    // update event
    Task<(bool? state, string? error)> UpdateEvent(Guid id, EventUpdate eventForm);

    // GetEventByHash
    Task<(EventDto? events, string error)> GetEventByHash(string hash, Guid userId);

    // get all events
    Task<(List<EventDto>? events, int? totalCount, string? error)> GetAllEvents(Guid userId, EventFilter filter);

    Task<(bool? state, string? error)> UpdatePublishState(Guid id, PublishStateForm state);

    // delete event 
    Task<(bool? state, string? error)> DeleteEvent(Guid id);

    // add point of sail to event 
    Task<(bool? state, string? error)> AddPointOfSale(Guid eventId, Guid pointOfSaleId);

    // add tags to event using AddTagToEventForm
    Task<(bool? state, string? error)> AddTagToEvent(Guid eventId, AddTagToEventForm tagForm);

    // change feature state 
    Task<(bool? state, string? error)> ChangeFeatureState(Guid id, EventFeatureStateForm state);
}

public class EventService : IEventService
{
    private readonly DataContext _context;
    private readonly ISeatIoService _seatIoService;
    private readonly IMapper _mapper;

    public EventService(DataContext context, ISeatIoService seatIoService, IMapper mapper)
    {
        _context = context;
        _seatIoService = seatIoService;
        _mapper = mapper;
    }

    public async Task<(EventDto? events, string error)> CreateEvent(EventForm eventForm)
    {
        var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var chart = await _context.Charts.Include(baseChart => baseChart.User)
                .Include(baseChart => baseChart.Categories).FirstOrDefaultAsync(x => x.Id == eventForm.ChartId);
            if (chart == null)
            {
                return (null!, "Chart not found");
            }

            var (validation, validationError) =
                await _seatIoService.ValidatePublishedVersionAsync(chart.ChartKey, chart.WorkspaceKey);
            if (validationError != null)
            {
                return (null!, validationError);
            }


            // add tags 
            var tags = await _context.Tags.AsNoTracking().Where(x => eventForm.TagIds.Contains(x.Id)).ToListAsync();
            if (tags.Count != eventForm?.TagIds?.Count) return (null!, "Tag not found");


            var newChartSeatIo =
                await _seatIoService.CopyChartAsync(chart.ChartKey, chart.WorkspaceKey, eventForm.Name);
            if (newChartSeatIo.error != null) return (null!, newChartSeatIo.error);



            var newCategories = chart.Categories.Select(x => new BaseCategory()
            {
                Name = x.Name,
                Color = x.Color,
                Price = x.Price,
            }).ToList();


            var newChartEntity = new BaseChart()
            {
                Name = eventForm.Name,
                RelatedChartId = newChartSeatIo.chart.Id,
                ChartKey = newChartSeatIo.chart.Key,
                PublishedVersionThumbnailUrl = newChartSeatIo.chart.PublishedVersionThumbnailUrl,
                DraftVersionThumbnailUrl = newChartSeatIo.chart.DraftVersionThumbnailUrl,
                WorkspaceKey = chart.WorkspaceKey,
                UserId = chart.UserId,
                User = chart.User,
                Categories = newCategories,
            };



            var newChartEntityAdded = await _context.Charts.AddAsync(newChartEntity);

            // remove categories and add new categories


            var events = new EventEntity
            {
                Name = eventForm.Name,
                StartEvent = eventForm.StartEvent,
                EndEvent = eventForm.EndEvent,
                Description = eventForm.Description,
                Attachments = eventForm.Attachments,
                ChartId = newChartEntityAdded.Entity.Id,
                Chart = newChartEntityAdded.Entity,
                SlugHash = HashHelper.GenerateHash(eventForm.Name),
                Lat = eventForm.Lat ?? (double)eventForm.Lat,
                Lng = eventForm.Lng ?? (double)eventForm.Lng,
                Address = eventForm.Address,
                IsFeature = eventForm.IsFeature,
                StartReservationDate = eventForm.StartReservationDate,
                EndReservationDate = eventForm.EndReservationDate,
                EventTags = tags.Select(x => new EventTag()
                {
                    TagId = x.Id
                }).ToList()
            };

            eventForm.PointOfSales?.ForEach(x =>
            {
                events.PointOfSales?.Add(new EventPointOfSale()
                {
                    PointOfSaleId = x
                });
            });


            var (seatsioEvent, error) =
                await _seatIoService.CreateEventAsync(events, newChartEntity.ChartKey, newChartEntity.WorkspaceKey);

            events.EventKey = seatsioEvent?.Key;


            await _seatIoService.RemoveAllCategoryFromChartAsync(newChartEntity.ChartKey, newChartEntity.WorkspaceKey);

            await _seatIoService.AddCategoriesToChartAsync(newChartEntity.ChartKey, newChartEntity.WorkspaceKey,
                newCategories.Select(x => new Category(x.Id.ToString(), x.Name, x.Color)).ToList()

            );



            await _context.Events.AddAsync(events);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (new EventDto()
            {
                Id = events.Id,
                Name = events.Name,
                StartEvent = events.StartEvent,
                EndEvent = events.EndEvent,
                Description = events.Description,
                Attachments = events.Attachments,
                ChartId = events.ChartId,
                Event = seatsioEvent,
                EventKey = events.EventKey,
                IsPublish = events.IsPublish,
                SlugHash = events.SlugHash,
                CreationDate = events.CreationDate,
                StartReservationDate = events.StartReservationDate,
                EndReservationDate = events.EndReservationDate,
                WorkspaceKey = chart.WorkspaceKey,
                Lat = events.Lat,
                Lng = events.Lng,
                Address = events.Address,
                SeatAvailable = eventForm.SeatAvailable,
                SeatBooked = eventForm.SeatBooked,

            }
                , null!);
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            return (null!, e.Message);
        }
    }

    public async Task<(EventDto? events, string error)> GetEventById(Guid id, Guid userId)
    {
        var events = await _context.Events.AsNoTracking().Include(x => x.TicketTemplates)
            .Include(x => x.Chart)
            .Include(x => x.PointOfSales)!
            .ThenInclude(x => x.PointOfSale)
            .Include(x => x.EventTags)
            .ThenInclude(x => x.Tag)
            .Include(x => x.Chart)
            .ThenInclude(x => x.User)
            .Where(x => x.Deleted != true)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (events == null) return (null!, "Event not found");


        var eventReports = await _seatIoService.GetEventReports(events.EventKey, events.Chart.WorkspaceKey);
        if (eventReports.error != null) return (null!, eventReports.error);

        events.SeatAvailable = eventReports.summaryItem["available"].Count;
        events.SeatBooked = eventReports.summaryItem["booked"].Count;

        _context.Events.Update(events);
        await _context.SaveChangesAsync();


        var chart = await _context.Charts.FindAsync(events.ChartId);
        if (chart == null) return (null!, "Chart not found");

        var seatsioEvent = await _seatIoService.RetrieveEventAsync(events.EventKey, events.Chart.WorkspaceKey);
        if (seatsioEvent.error != null) return (null!, seatsioEvent.error);


        var categories = await _context.Categories.AsNoTracking().Where(x => x.ChartId == chart.Id)
            .ProjectTo<CategoryDto>(_mapper.ConfigurationProvider).ToListAsync();


        var pointOfSale = _mapper.Map<List<PointOfSaleDto>>(events.PointOfSales.Select(x => x.PointOfSale).ToList());


        var eventDto = _mapper.Map<EventDto>(events);
        // var eventDto = new EventDto()
        // {
        //     Id = events.Id,
        //     Name = events.Name,
        //     StartEvent = events.StartEvent,
        //     EndEvent = events.EndEvent,
        //     Description = events.Description,
        //     Attachments = events.Attachments,
        //     ChartId = events.ChartId,
        //     Event = seatsioEvent.events,
        //     EventKey = events.EventKey,
        //     IsPublish = events.IsPublish,
        //     SlugHash = events.SlugHash,
        //     CreationDate = events.CreationDate,
        //     StartReservationDate = events.StartReservationDate,
        //     EndReservationDate = events.EndReservationDate,
        //     WorkspaceKey = events.Chart.WorkspaceKey,
        //     Categories = categories,
        //     Lat = events.Lat,
        //     Lng = events.Lng,
        //     Address = events.Address,
        //     SeatBooked = events.SeatBooked,
        //     SeatAvailable = events.SeatAvailable,
        //     PointOfSales = pointOfSale,
        //     
        // };

        var eventFavorites = await _context.EventFavorites.AsNoTracking().Where(x => x.UserId == userId).ToListAsync();


        if (eventDto.StartEvent > DateTime.Now)
        {
            eventDto.State = EventFilterState.Coming;
        }
        else if (eventDto.EndEvent < DateTime.Now)
        {
            eventDto.State = EventFilterState.Ended;
        }
        else if (eventDto.IsPublish == false)
        {
            eventDto.State = EventFilterState.NotPosted;
        }

        eventDto.IsFavorite = eventFavorites.Any(y => y.EventId == eventDto.Id);


        return (eventDto, null!);
    }

    public async Task<(bool? state, string? error)> UpdateEvent(Guid id, EventUpdate eventFormUpdate)
    {
        var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var events = await _context.Events.AsQueryable().Include(x => x.Chart)
                .Include(x => x.PointOfSales)
                .Include(x => x.EventTags)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (events == null) return (null!, "Event not found");

            _mapper.Map(eventFormUpdate, events);

            await _context.SaveChangesAsync();

            var seatio =
                await _seatIoService.UpdateEventAsync(events, events.Chart.ChartKey, events.Chart.WorkspaceKey);
            if (seatio.error != null) return (null!, seatio.error);

            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            return (null!, e.Message);
        }


        return (true, null!);
    }

    public async Task<(EventDto? events, string error)> GetEventByHash(string hash, Guid userId)
    {
        var events = await _context.Events
            .AsNoTracking()
            .Include(x => x.TicketTemplates)
            .Include(x => x.Chart)
            .ThenInclude(x => x.User)
            .Include(eventEntity => eventEntity.PointOfSales)!
            .ThenInclude(eventPointOfSale => eventPointOfSale.PointOfSale)
            .Include(x => x.EventTags)
            .ThenInclude(x => x.Tag)
            .Where(x => x.Deleted != true)
            .FirstOrDefaultAsync(x => x.SlugHash == hash);
        if (events == null)
        {
            return (null!, "Event not found");
        }

        var eventReports = await _seatIoService.GetEventReports(events.EventKey, events.Chart.WorkspaceKey);
        if (eventReports.error != null) return (null!, eventReports.error);

        events.SeatAvailable = eventReports.summaryItem["available"].Count;
        events.SeatBooked = eventReports.summaryItem["booked"].Count;

        _context.Events.Update(events);
        await _context.SaveChangesAsync();

        var chart = await _context.Charts.FindAsync(events.ChartId);
        if (chart == null)
        {
            return (null!, "Chart not found");
        }

        var seatsioEvent = await _seatIoService.RetrieveEventAsync(events.EventKey, events.Chart.WorkspaceKey);

        if (seatsioEvent.error != null)
        {
            return (null!, seatsioEvent.error);
        }

        var categories = await _context.Categories.AsNoTracking().Where(x => x.ChartId == chart.Id)
            .ProjectTo<CategoryDto>(_mapper.ConfigurationProvider).ToListAsync();

        var pointOfSale = _mapper.Map<List<PointOfSaleDto>>(events.PointOfSales.Select(x => x.PointOfSale).ToList());

        var eventDto = _mapper.Map<EventDto>(events);
        // var eventDto = new EventDto()
        // {
        //     Id = events.Id,
        //     Name = events.Name,
        //     StartEvent = events.StartEvent,
        //     EndEvent = events.EndEvent,
        //     Description = events.Description,
        //     Attachments = events.Attachments,
        //     ChartId = events.ChartId,
        //     Event = seatsioEvent.events,
        //     EventKey = events.EventKey,
        //     SlugHash = events.SlugHash,
        //     IsPublish = events.IsPublish,
        //     CreationDate = events.CreationDate,
        //     StartReservationDate = events.StartReservationDate,
        //     EndReservationDate = events.EndReservationDate,
        //     WorkspaceKey = events.Chart.WorkspaceKey,
        //     Categories = categories,
        //     Lat = events.Lat,
        //     Lng = events.Lng,
        //     Address = events.Address,
        //     SeatBooked = events.SeatBooked,
        //     SeatAvailable = events.SeatAvailable,
        //     PointOfSales = pointOfSale ,
        //     
        // };
        var eventFavorites = await _context.EventFavorites.AsNoTracking().Where(x => x.UserId == userId).ToListAsync();


        if (eventDto.StartEvent > DateTime.Now)
        {
            eventDto.State = EventFilterState.Coming;
        }
        else if (eventDto.EndEvent < DateTime.Now)
        {
            eventDto.State = EventFilterState.Ended;
        }
        else if (eventDto.IsPublish == false)
        {
            eventDto.State = EventFilterState.NotPosted;
        }

        eventDto.IsFavorite = eventFavorites.Any(y => y.EventId == eventDto.Id);


        return (eventDto, null!);
    }


    private double DegreesToRadians(double degrees)
    {
        return degrees * (Math.PI / 180);
    }


    public async Task<(List<EventDto>? events, int? totalCount, string? error)> GetAllEvents(Guid userId,
        EventFilter filter)
    {
        var baseEvents = _context.Events.AsNoTracking()
                .Include(x => x.Chart)
                .ThenInclude(x => x.Categories)
                .Where(x => filter.Name == null || x.Name.Contains(filter.Name))
                .Where(x => filter.Tag == null || x.EventTags.Any(y => y.TagId == filter.Tag))
                .Where(x => x.Deleted != true)
                // .Where(x => x.Deleted != true)
                // .Where(x => filter.Lat == null && filter.Lng == null ||
                //             (Math.Abs((double)(x.Lat - filter.Lat)) < (filter.Distance / 111) &&
                //              Math.Abs((double)(x.Lng - filter.Lng)) < (filter.Distance /
                //                                                        (111 * Math.Cos(
                //                                                            DegreesToRadians((double)filter.Lat))))))
                .Where(x => filter.IsFeature == null || x.IsFeature == filter.IsFeature)
            ;

        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId);

        if (userId != null && userId != Guid.Empty && user!=null)
        {


            baseEvents = user.Role switch
            {
                UserRole.Admin => baseEvents,
                UserRole.User => baseEvents.Where(x => x.StartEvent > DateTime.Now && x.Chart.Categories.Count > 0 && (bool)x.IsPublish),
                UserRole.Provider => baseEvents.Where(x => x.Chart.WorkspaceKey == user.WorkspacePublicKey),
                UserRole.PointOfSale => baseEvents.Where(x => x.PointOfSales.Any(y => y.PointOfSale.Id == userId) && (bool)x.IsPublish),
                _ => baseEvents.Where(x => x.StartEvent > DateTime.Now && x.Chart.Categories.Count > 0 && (bool)x.IsPublish),
            };
        }
        else
        {
            baseEvents = baseEvents.Where(x => x.StartEvent > DateTime.Now && x.Chart.Categories.Count > 0 && (bool)x.IsPublish);
        }


        if (filter.State != null)
        {
            baseEvents = filter.State switch
            {
                EventFilterState.Coming => baseEvents.Where(x => x.StartEvent > DateTime.Now),
                EventFilterState.Ended => baseEvents.Where(x => x.EndEvent < DateTime.Now),
                EventFilterState.NotPosted => baseEvents.Where(x => x.IsPublish == false),
                _ => baseEvents
            };
        }

        var totalCount = await baseEvents.CountAsync();

        if (filter is { Lat: not null, Lng: not null })
        {
            baseEvents = baseEvents.OrderByDescending(x =>
                Math.Abs((double)(x.Lat - filter.Lat)) < (filter.Distance / 111) &&
                Math.Abs((double)(x.Lng - filter.Lng)) < (filter.Distance /
                                                          (111 * Math.Cos(
                                                              DegreesToRadians(
                                                                  (double)filter.Lat)))));
        }

        var result = await baseEvents
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ProjectTo<EventDto>(_mapper.ConfigurationProvider)
            .ToListAsync();


        var eventFavorites = await _context.EventFavorites.AsNoTracking().Where(x => x.UserId == userId).ToListAsync();

        result.ForEach(x =>
        {
            if (x.StartEvent > DateTime.Now)
            {
                x.State = EventFilterState.Coming;
            }
            else if (x.EndEvent < DateTime.Now)
            {
                x.State = EventFilterState.Ended;
            }
            else if (x.IsPublish == false)
            {
                x.State = EventFilterState.NotPosted;
            }

            x.IsFavorite = eventFavorites.Any(y => y.EventId == x.Id);
        });

        return (result, totalCount, null);
    }

    public async Task<(bool? state, string? error)> UpdatePublishState(Guid id, PublishStateForm state)
    {
        var events = await _context.Events.Include(eventEntity => eventEntity.Chart)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (events == null) return (null!, "Event not found");

        var seatio = await _seatIoService.ValidateDraftVersionAsync(events.Chart.ChartKey, events.Chart.WorkspaceKey);
        if (seatio.error != null) return (null!, "يجب تعديل الخريطة اولا");

        events.SlugHash ??= HashHelper.GenerateHash(events.Name);

        events.IsPublish = state.IsPublish;

        var eventReports = await _seatIoService.GetEventReports(events.EventKey, events.Chart.WorkspaceKey);
        if (eventReports.error != null) return (null!, eventReports.error);

        events.SeatAvailable = eventReports.summaryItem["available"].Count;
        events.SeatBooked = eventReports.summaryItem["booked"].Count;

        _context.Events.Update(events);
        await _context.SaveChangesAsync();
        return (true, null!);
    }

    public async Task<(bool? state, string? error)> DeleteEvent(Guid id)
    {
        var events = await _context.Events.Include(x => x.Chart)
            .Include(x => x.PointOfSales).Include(eventEntity => eventEntity.Books)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (events == null) return (null!, "Event not found");

        await _seatIoService.DeleteEvent(events.EventKey, events.Chart.WorkspaceKey);

        if (events.Books?.Count > 0)
            return (null!, "الحدث لديه حجوزات لا يمكن حذفه");

        events.Deleted = true;
        _context.Events.Update(events);
        await _context.SaveChangesAsync();
        return (true, null!);
    }

    public async Task<(bool? state, string? error)> AddPointOfSale(Guid eventId, Guid pointOfSaleId)
    {
        var events = await _context.Events.FindAsync(eventId);
        if (events == null) return (null!, "Event not found");

        var pointOfSale = await _context.PointOfSales.FindAsync(pointOfSaleId);
        if (pointOfSale == null) return (null!, "Point of sale not found");

        events.PointOfSales?.Add(new EventPointOfSale()
        {
            PointOfSaleId = pointOfSaleId
        });

        _context.Events.Update(events);
        await _context.SaveChangesAsync();
        return (true, null!);
    }

    public async Task<(bool? state, string? error)> AddTagToEvent(Guid eventId, AddTagToEventForm tagForm)
    {
        var events = await _context.Events.AsNoTracking().Include(eventEntity => eventEntity.EventTags)
            .FirstOrDefaultAsync(x => x.Id == eventId);
        if (events == null) return (null!, "Event not found");

        var tags = await _context.Tags.AsNoTracking().Where(x => tagForm.TagIds.Contains(x.Id)).ToListAsync();
        if (tags.Count != tagForm.TagIds.Count) return (null!, "Tag not found");

        // remove and replace old tags with new tags

        _context.EventTags.RemoveRange(events.EventTags);
        events.EventTags = tags.Select(x => new EventTag()
        {
            TagId = x.Id
        }).ToList();

        _context.Events.Update(events);
        await _context.SaveChangesAsync();
        return (true, null!);
    }

    public async Task<(bool? state, string? error)> ChangeFeatureState(Guid id, EventFeatureStateForm form)
    {
        var events = await _context.Events.FindAsync(id);
        if (events == null) return (null!, "Event not found");

        events.IsFeature = form.IsFeature;

        _context.Events.Update(events);
        await _context.SaveChangesAsync();
        return (true, null!);
    }
}