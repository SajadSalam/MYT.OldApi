using AutoMapper;
using AutoMapper.QueryableExtensions;
using Events.DATA;
using Events.DATA.DTOs.seatIo;
using Events.DATA.DTOs.Tickets;
using Events.Entities;
using Events.Entities.Book;
using Events.Entities.Ticket;
using Microsoft.EntityFrameworkCore;

namespace Events.Services;

public interface ITicketService
{
    Task<(Ticket? ticket, string? error)> ReleaseTicketAsync(Guid bookObjectId);

    Task<(List<Ticket>? tickets, string? error)> ReleaseTicketsAsync(List<BookObject> bookObjects);


    // get all tickets
    Task<(List<TicketDto>? tickets, int? totalCount, string? error)> GetTicketsAsync(UserRole role, Guid userId,
        TicketFilter filter);

    // ticket by id 
    Task<(TicketDto? ticket, string? error)> GetTicketAsync(long number);

    // change is used 
    Task<(bool? state, string? error)> ChangeIsUsedAsync(long ticketNumber);

    // cancel ticket
    Task<(bool? state, string? error)> CancelTicketAsync(ChangeTicketState changeTicketState);
}

public class TicketService : ITicketService
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    private readonly ISeatIoService _seatIoService;
    private readonly ISadidService _sadidService;
    private readonly string _secretKey;

    public TicketService(DataContext context, IMapper mapper, IConfiguration configuration, ISeatIoService seatIoService, ISadidService sadidService)
    {
        _context = context;
        _mapper = mapper;
        _seatIoService = seatIoService;
        _secretKey = configuration["sadid:ticket_key"];
        _sadidService = sadidService;
    }

    public async Task<(Ticket? ticket, string? error)> ReleaseTicketAsync(Guid bookObjectId)
    {
        var bookObject = await _context.BookObjects.AsNoTracking()
            .Include(x => x.Ticket)
            .Include(bookObject => bookObject.Category)
            .FirstOrDefaultAsync(x => x.Id == bookObjectId);

        if (bookObject == null) return (null, "Book Object Not Found");
        if (bookObject.Ticket != null) return (null, "Ticket Already Released");

        var ticket = new Ticket()
        {
            TicketSeating = bookObject.Name + " , " + bookObject.Type,
            SeatCategory = bookObject.Category?.Name,
            BookObjectId = bookObject.Id
        };

        await _context.Tickets.AddAsync(ticket);


        return (ticket, null);
    }

    public async Task<(List<Ticket>? tickets, string? error)> ReleaseTicketsAsync(List<BookObject> bookObjects)
    {
        try
        {

            //    var bookObjects = await _context.BookObjects.AsNoTracking()
            //     .Include(x => x.Ticket)
            //     .Include(bookObject => bookObject.Category)
            //     .Where(x => bookObjectIds.Contains(x.Id))
            //     .ToListAsync();


            if (bookObjects.Count == 0) return (null, "Book Objects Not Found");
            if (bookObjects.Any(x => x.Ticket != null)) return (null, "Some Tickets Already Released");

            var tickets = bookObjects.Select(x => new Ticket()
            {
                TicketSeating = x.Name + " , " + x.Type,
                SeatCategory = x.Category?.Name,
                BookObjectId = x.Id
            }).ToList();


            bookObjects.ForEach(x => x.Ticket = tickets.First(t => t.BookObjectId == x.Id));
            await _context.Tickets.AddRangeAsync(tickets);
            _context.BookObjects.UpdateRange(bookObjects);
            // await _context.SaveChangesAsync();
            return (tickets, null);
        }
        catch (Exception e)
        {
            return (null, e.Message);
        }
    }


    public async Task<(List<TicketDto>? tickets, int? totalCount, string? error)> GetTicketsAsync(UserRole role,
        Guid userId,
        TicketFilter filter)
    {
        var tickets = _context.Tickets.AsNoTracking().AsQueryable()
       .Where(x => (filter.Name == null || x.TicketSeating.Contains(filter.Name))
                   && (filter.EventId == null || x.BookObject.Book.EventId == filter.EventId)
                   && (filter.IsPaid == null || x.BookObject.Book.IsPaid == filter.IsPaid)
                   && (filter.bookId== null || x.BookObject.BookId == filter.bookId) 
                   && (filter.IsUsed == null || x.IsUsed == filter.IsUsed)
                   && (filter.State == null ||
                       (filter.State == TicketStateEnum.Canceled && x.BookObject.IsCanceled) ||
                       (filter.State == TicketStateEnum.Paid && x.BookObject.Book.IsPaid == true) ||
                       (filter.State == TicketStateEnum.UnPaid && !x.BookObject.Book.IsPaid == false)
                   )
       );



        switch (role)
        {
            case UserRole.Admin:
                break;
            case UserRole.User:
                tickets.Where(x => x.BookObject.Book.UserId == userId);
                break;
            case UserRole.Provider:
                tickets
                    .Where(x => x.BookObject.Book.Event.Chart.UserId == userId);
                break;
            case UserRole.PointOfSale:
                tickets
                    .Where(x => x.BookObject.Book.Event.PointOfSales.Any(p => p.PointOfSaleId == userId));
                break;
            default:
                break;
        }

        var query = tickets.OrderByDescending(x => x.CreationDate);

        var totalCount = await tickets.CountAsync();

        var result = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ProjectTo<TicketDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        result.ForEach(x => x.State = filter.State);

        return (result, totalCount, null);
    }

    public async Task<(TicketDto? ticket, string? error)> GetTicketAsync(long number)
    {
        var ticket = await _context.Tickets.AsNoTracking()
            .Where(x => x.Number == number)
            .ProjectTo<TicketDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (ticket == null) return (null, "Ticket Not Found");

        return (ticket, null);
    }

    public async Task<(bool? state, string? error)> ChangeIsUsedAsync(long ticketNumber)
    {
        var ticket = await _context.Tickets.FirstOrDefaultAsync(x => x.Number == ticketNumber);
        if (ticket == null) return (null, "Ticket Not Found");
        if (ticket.IsUsed) return (null, "التذكرة مستخدمة بالفعل");
        ticket.IsUsed = true;
        _context.Tickets.Update(ticket);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool? state, string? error)> CancelTicketAsync(ChangeTicketState changeTicketState)
    {
        var tickets = await _context.Tickets
            .Where(ticket => changeTicketState.TicketNumbers.Contains(ticket.Number))
            .Include(ticket => ticket.BookObject)
            .ThenInclude(bookObject => bookObject.Book)
            .ThenInclude(book => book.Bill)
            .Include(ticket => ticket.BookObject)
            .ThenInclude(bookObject => bookObject.Book)
            .ThenInclude(book => book.Event)
            .ToListAsync();




        
        if (tickets.Count == 0) return (null, "Tickets Not Found");
        if (tickets.Count != changeTicketState.TicketNumbers.Count) return (null, "Some Tickets Not Found");

        var objectToChange = tickets.GroupBy(k => k.BookObject?.Book?.Event?.EventKey).Select(x =>
            new ChangeObjectStateSeatIo
            {
                EventKey = x.Key,
                WorkspaceKey = x.Select(x => x.BookObject?.Book?.Event?.EventKey).FirstOrDefault(),
                ObjectKeys = x.Select(x => x.BookObject?.Book?.Event?.EventKey).ToList()
            }).ToList();

        await _seatIoService.ChangeObjectStatusAsync(objectToChange, "free");
        tickets.ForEach(x =>
        {
            _sadidService.ChangeBillState(x.BookObject?.Book?.Bill?.BillId, _secretKey, SadidService.SadidBillState.Canceled);
            x.BookObject.IsCanceled = true;
            x.BookObject.Book.TotalPrice -= x.BookObject.Price;
        });



        // TODO : Sadid cancel order

        _context.Tickets.UpdateRange(tickets);

        await _context.SaveChangesAsync();
        return (true, null);
    }
}