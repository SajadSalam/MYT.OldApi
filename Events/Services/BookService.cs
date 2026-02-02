using AutoMapper;
using AutoMapper.QueryableExtensions;
using Events.DATA;
using Events.DATA.DTOs;
using Events.DATA.DTOs.Book;
using Events.DATA.DTOs.Category;
using Events.DATA.DTOs.Event;
using Events.DATA.DTOs.Payment;
using Events.Entities;
using Events.Entities.Book;
using Events.Entities.Ticket;
using Events.Services.Payment;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SeatsioDotNet.Events;

namespace Events.Services;

public interface IBookService
{
    Task<(BookDto? state, string? error)> CreateBook(Guid eventId, Guid userId, ObjectForm objectForm);

    // get all 
    Task<(List<BookDto>? books, int? totalCount, string? error)> GetBooksAsync(UserRole role, Guid userId,
        BookFilter filter);

    // get by id 
    Task<(BookDto? book, string? error)> GetBookAsync(Guid id);

    Task<(bool seccess, string? error)> Pay(PayBillRequest billResponse);
}

public class BookService : IBookService
{
    private readonly ISeatIoService _seatIoService;

    private readonly DataContext _context;

    private readonly ITicketService _ticketService;

    private readonly IMapper _mapper;
    private readonly IPaymentGatewayFactory _paymentGatewayFactory;

    private readonly string _secretKey;

    public BookService(ISeatIoService seatIoService, DataContext context, ITicketService ticketService, IMapper mapper,
        IConfiguration configuration, IPaymentGatewayFactory paymentGatewayFactory)
    {
        _seatIoService = seatIoService;
        _context = context;
        _ticketService = ticketService;
        _mapper = mapper;
        _paymentGatewayFactory = paymentGatewayFactory;
        _secretKey = configuration["sadid:ticket_key"];
    }

    public async Task<(BookDto? state, string? error)> CreateBook(Guid eventId, Guid userId, ObjectForm objectForm)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) throw new UnauthorizedAccessException("User Not Found");

        var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var eventEntity = await _context.Events
                .Include(eventEntity => eventEntity.Chart)
                .ThenInclude(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == eventId);
            if (eventEntity == null) return (null, "Event not found");


            var getObjectInfo = await _seatIoService.RetrieveObjectAsync(
                eventEntity.EventKey,
                eventEntity.Chart.WorkspaceKey,
                objectForm.Objects
            );
            if (getObjectInfo.error != null) return (null, getObjectInfo.error);


            if (getObjectInfo.objs.Count != objectForm.Objects.Count)
                return (null, "Some objects not found in the event");
            if (getObjectInfo.objs.Any(x => x.Value.Status != "free" || x.Value.IsAvailable != true))
                return (null, "Some objects already booked or not available");

            var categories = await _context.Categories.Where(x => x.ChartId == eventEntity.ChartId).ToListAsync();
            if (categories.Count == 0) return (null, "Categories not found");

            var newObjects = objectForm.Objects.SelectMany(x =>
            {
                var obj = getObjectInfo.objs[x];
                if (obj.ObjectType == "table")
                {
                    var objects = new List<BookObject>();

                    for (var i = 0; i < obj.NumSeats; i++)
                    {
                        var cat = categories.FirstOrDefault(y => y.Id == Guid.Parse(obj.CategoryKey));
                        objects.Add(new BookObject
                        {
                            Name = $"{obj.Label}-{i + 1}",
                            FullName = objectForm.FullName,
                            PhoneNumber = objectForm.PhoneNumber,
                            CategoryId = cat.Id,
                            Price = cat.Price,
                            Type = obj.ObjectType,
                        });
                    }

                    return objects;
                }
                else
                {
                    var cat = categories.FirstOrDefault(y => y.Id == Guid.Parse(obj.CategoryKey));
                    return new List<BookObject>
                    {
                        new()
                        {
                            Name = obj.Label,
                            FullName = objectForm.FullName,
                            PhoneNumber = objectForm.PhoneNumber,
                            CategoryId = cat.Id,
                            Price = cat.Price,
                            Type = obj.ObjectType
                        }
                    };
                }
            }).ToList();

            var totalPrice = newObjects.Sum(x => x.Price);

            if (objectForm.Discount > totalPrice)
                return (null, "Discount cannot exceed the total price.");


            // TODO : Get Minutes From Settings

            var createBook = await _seatIoService.HoldTicketAsync(
                eventEntity.EventKey,
                eventEntity.Chart.WorkspaceKey,
                15,
                objectForm.Objects
            );

            if (createBook.error != null) return (null, createBook.error);

            var bookId = Guid.NewGuid();

            // TODO: when add user id throws database exception: there is no FK_Book_PointOfSell_UserId constraint so I comment it for continue testing
            var book = new Book
            {
                Id = bookId,
                Event = eventEntity,
                EventId = eventId,
                Objects = newObjects,
                TotalPrice = totalPrice,
                UserId = userId,
                Discount = objectForm.Discount
            };


            book.Objects = new List<BookObject>();


            foreach (var x in createBook.data.Objects)
            {
                if (x.Value.ObjectType == "table")
                {
                    int index = 1;
                    foreach (var t in newObjects.Where(b => b.Type == "table"))
                    {
                        var name = $"{x.Key}-{index}";
                        var newObject = newObjects.FirstOrDefault(y => y.Name == name);
                        if (newObject != null)
                        {
                            book.Objects.Add(new BookObject
                            {
                                Name = name,
                                FullName = newObject.FullName,
                                PhoneNumber = newObject.PhoneNumber,
                                CategoryId = newObject.CategoryId,
                                Price = newObject.Price,
                                Type = x.Value.ObjectType,
                                Book = book,
                                BookHoldInfo = new BookHoldInfo(x.Value.HoldToken, 15)
                            });
                        }

                        index++;
                    }
                }
                else
                {
                    var newObject = newObjects.FirstOrDefault(y => y.Name == x.Key);
                    if (newObject != null)
                    {
                        book.Objects.Add(new BookObject
                        {
                            Name = x.Key,
                            FullName = newObject.FullName,
                            PhoneNumber = newObject.PhoneNumber,
                            CategoryId = newObject.CategoryId,
                            Price = newObject.Price,
                            Book = book,
                            Type = x.Value.ObjectType,
                            BookHoldInfo = new BookHoldInfo(x.Value.HoldToken, 15)
                        });
                    }
                }
            }


            book.BookHoldInfo = new BookHoldInfo(book.Objects.FirstOrDefault().BookHoldInfo.HoldToken, 15);

            var result = await _context.Books.AddAsync(book);
            if (result == null!) return (null, "Some thing went wrong");

            // Determine which payment provider to use
            var paymentProvider = objectForm.PreferredPaymentMethod ?? PaymentProvider.Amwal;
            
            // Check if provider is available
            if (!_paymentGatewayFactory.IsProviderAvailable(paymentProvider))
            {
                return (null, $"Payment provider {paymentProvider} is not available");
            }

            // Get the appropriate payment gateway
            var paymentGateway = _paymentGatewayFactory.GetGateway(paymentProvider);

            // Create payment request
            var paymentRequest = new PaymentRequest
            {
                Amount = newObjects.Sum(x => x.Price) - objectForm.Discount,
                CustomerName = newObjects.First().FullName,
                CustomerPhone = newObjects.First().PhoneNumber,
                BookId = bookId,
                EventId = eventId,
                ExpireDate = eventEntity.EndEvent,
                RedirectUrl = "https://events-api.future-wave.co/api/book/pay"
            };

            // Create payment through the gateway
            var paymentResponse = await paymentGateway.CreatePaymentAsync(paymentRequest);
            
            Bill? bill = null;
            if (paymentResponse.IsSuccess)
            {
                var billsaved = await _context.Bills.AddAsync(new Bill
                {
                    BillId = paymentResponse.PaymentId,
                    BookId = bookId,
                    Book = book,
                    PaymentStatus = PaymentStatus.NotPaid,
                    TotalPrice = newObjects.Sum(x => x.Price) - objectForm.Discount,
                    PaymentProvider = paymentProvider,
                    PaymentProviderId = paymentResponse.PaymentId,
                    PaymentMetadata = paymentResponse.AdditionalData != null 
                        ? JsonConvert.SerializeObject(paymentResponse.AdditionalData) 
                        : null
                });
                bill = billsaved.Entity;
            }
            else
            {
                await transaction.RollbackAsync();
                return (null, $"Payment creation failed: {paymentResponse.Error}");
            }


            await _context.SaveChangesAsync();

            if (user.Role == UserRole.PointOfSale)
            {
                if (bill == null)
                    return (null, "Payment creation not successful");

                var payResult = await Pay(new PayBillRequest { BillId = bill.BillId, SecretKey = _secretKey });

                if (payResult.error != null)
                    return (null, $"Error From Pay To Sale Point : {payResult.error}");
            }

            var resultBook = new BookDto
            {
                Id = book.Id,
                EventId = book.EventId,
                Objects = book.Objects.Select(x =>
                {
                    // TODO: check if book hold info is null
                    // TODO: I made it as a fast way for testing try to make it better or leave it for now it's working correctly
                    var remainingTime = TimeSpan.FromMinutes(x.BookHoldInfo!.ExpiredMinutes) -
                                        (DateTime.UtcNow - x.CreationDate!.Value);

                    return new BookObjectDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                        FullName = x.FullName,
                        PhoneNumber = x.PhoneNumber,
                        BookId = null,
                        CategoryId = x.CategoryId,
                        Category = _mapper.Map<CategoryDto>(x.Category),
                        Price = x.Price,
                        Type = x.Type,
                        ExpiredTime = $"{(int)remainingTime.TotalMinutes}:{remainingTime.Seconds}",
                    };
                }).ToList(),
                TotalPrice = book.TotalPrice,
                TotalPriceAfterDiscount = book.TotalPrice - book.Discount
            };


            await transaction.CommitAsync();

            return (resultBook, null);
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            return (null, e.Message);
        }
    }

    public async Task<(List<BookDto>? books, int? totalCount, string? error)> GetBooksAsync(UserRole role, Guid userId,
        BookFilter filter)
    {
        var IsUser = await _context.Users.AsNoTracking().AnyAsync(x => x.Id == userId);
        if (!IsUser) throw new UnauthorizedAccessException();

        var books = _context.Books.AsNoTracking().AsQueryable()
                .Where(x =>
                    filter.IsComingBook == null || (filter.IsComingBook == true
                        ? x.Event.StartEvent > DateTime.UtcNow
                        : x.Event.StartEvent <= DateTime.UtcNow)
                )
                .Where(x => filter.eventName == null || x.Event.Name.Contains(filter.eventName))
            ;

        switch (role)
        {
            case UserRole.Admin:
                break;
            case UserRole.User:
                books = books.Where(x => x.UserId == userId);
                break;
        }

        var totalCount = await books.CountAsync();

        if (filter.IsComingBook == true)
            books = books.OrderByDescending(x => x.Event.StartEvent > DateTime.Now)
                .ThenByDescending(x => x.Event.StartEvent);
        else
            books = books.OrderByDescending(x => x.CreationDate);

        var result = await books
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ProjectTo<BookDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return (result, totalCount, null);
    }

    public async Task<(BookDto? book, string? error)> GetBookAsync(Guid id)
    {
        var book = await _context.Books.AsNoTracking()
            .Where(x => x.Id == id)
            .ProjectTo<BookDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (book == null)
            return (null, "Book Not Found");

        return (book, null);
    }


    public async Task<(bool seccess, string? error)> Pay(PayBillRequest billResponse)
    {
        if (billResponse.SecretKey != _secretKey) return (false, "Secret key is not valid");

        // Retrieve and detach the bill
        var bill = await _context.Bills.AsNoTracking()
            .FirstOrDefaultAsync(x => x.BillId == billResponse.BillId);
        if (bill == null) return (false, "not found");

        // Get the payment provider for this bill
        var paymentProvider = bill.PaymentProvider ?? PaymentProvider.Amwal;
        
        try
        {
            // Get the appropriate payment gateway and confirm payment
            var paymentGateway = _paymentGatewayFactory.GetGateway(paymentProvider);
            var confirmResult = await paymentGateway.ConfirmPaymentAsync(bill.BillId, _secretKey);
            
            if (!confirmResult.IsSuccess)
            {
                return (false, $"Payment confirmation failed: {confirmResult.Error}");
            }
        }
        catch (Exception ex)
        {
            return (false, $"Payment gateway error: {ex.Message}");
        }

        // Retrieve bookObjects with AsNoTracking to prevent tracking issues
        var bookObjects = await _context.BookObjects.AsNoTracking().Include(x => x.Book)
            .ThenInclude(b => b.Event)
            .ThenInclude(e => e.Chart)
            .ThenInclude(c => c.User).Include(bookObject => bookObject.Category)
            .Where(x => x.BookId == bill.BookId)
            .ToListAsync();

        // Detach to prevent tracking issues
        foreach (var bookObject in bookObjects)
        {
            _context.Entry(bookObject).State = EntityState.Detached;
        }

        // Prepare list for bookObjects grouped by name
        var categorizedObjects = bookObjects.Select(o =>
        {
            if (o.Type == "table")
            {
                var nameList = o.Name.Split("-").ToList();
                nameList.RemoveAt(nameList.Count - 1);
                return string.Join("-", nameList);
            }

            return o.Name;
        }).ToList();

        var groupedObjects = categorizedObjects.GroupBy(e => e).Select(e => e.Key).ToList();

        // Call external service
        var changeBookState = await _seatIoService.BookTicketAsync(
            bookObjects.First().Book.Event.EventKey,
            bookObjects.First().Book.Event.Chart.User.WorkspacePublicKey,
            bookObjects.First().Book.BookHoldInfo.HoldToken, groupedObjects,
            bill.BookId.ToString());

        if (changeBookState.error != null) return (false, $" from BookTicketAsync SeatIo : {changeBookState.error}");

        // Update `IsPaid` on fresh tracked instance
        var trackedBill = await _context.Bills.FindAsync(bill.Id);
        trackedBill.PaymentStatus = PaymentStatus.Paid;
        trackedBill.PaymentDate = DateTime.Now;

        // Mark Book as paid
        var book = await _context.Books.FindAsync(bookObjects.First().BookId);
        book.IsPaid = true;

        List<Ticket> tickets = new();

        foreach (var bookObject in bookObjects)
        {
            tickets.Add(new Ticket()
            {
                TicketSeating = $"{bookObject.Name} , {bookObject.Type}",
                SeatCategory = bookObject.Category?.Name,
                BookObjectId = bookObject.Id
            });



        }

        _context.Bills.Update(trackedBill);
        await _context.AddRangeAsync(tickets);
        await _context.SaveChangesAsync();

        return (true, null);
    }
}