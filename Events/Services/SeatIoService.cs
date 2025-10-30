using System.Security.Claims;
using Events.DATA;
using Events.DATA.DTOs.Category;
using Events.DATA.DTOs.seatIo;
using Events.Entities;
using Newtonsoft.Json;
using SeatsioDotNet;
using SeatsioDotNet.Charts;
using SeatsioDotNet.EventReports;
using SeatsioDotNet.Events;
using SeatsioDotNet.Reports.Events;

namespace Events.Services;

public interface ISeatIoService
{
    Task<(Workspace? workspace, string? error)> CreateWorkspaceAsync(string name);

    Task<(Workspace? workspace, string? error)> EditWorkspaceAsync(string name, string key);

    Task<(Workspace? workspace, string? error)> DeleteWorkspaceAsync(string key);

    Task<(List<Chart>? charts, int? totalCount, string? error)> GetChartAsync();
    
    // copy chart 
    Task<(Chart? chart, string? error)> CopyChartAsync(string key, string workspaceKey, string eventName);


    Task<(Event? events, string? error)> RetrieveEventAsync(string name, string workspaceKey);

    // update events
    Task<(Event? events, string? error)>
        UpdateEventAsync(EventEntity eventEntity, string chartKey, string workspaceKey);


    Task<(List<Category>? categories, int? totalCount, string? error)> GetCategoryByChartKeyAsync(string key);

    // update category 

    Task<(Category category, string? error)> UpdateCategory(string chartKey, string categoryKey, string? workspaceKey,
        CategoryUpdateParams categoryUpdateParams);
    
    
    Task<(Chart? chart, string workspaceKey, string? error)> CreateChartAsync(string name, List<Category> categories);

    // delete chart 
    Task<(bool success, string? error)> DeleteChartAsync(string key);


    Task<(Event? events, string? error)>
        CreateEventAsync(EventEntity eventEntity, string chartKey, string workspaceKey);

    // Retrieve Object
    Task<(Dictionary<string, EventObjectInfo> objs, string? error)> RetrieveObjectAsync(List<string> objectKeys);

    // validation chart
    Task<(ChartValidationResult? data, string? error)> ValidatePublishedVersionAsync(string chartKey, string workspaceKey);

    
    // validate draft version
    Task<(ChartValidationResult? data, string? error)> ValidateDraftVersionAsync(string chartKey, string workspaceKey);
    
    // add category to chatr 
    Task<(bool? state, string error)> AddCategoryToChartAsync(string chartKey, string workspaceKey, Category category);

    // Remove category from chart 
    Task<bool> RemoveCategoryFromChartAsync(string chartKey,
        string workspaceKey,
        RemoveCategoryForm category);

    // book 
    Task<(ChangeObjectStatusResult data, string? error)> BookTicketAsync(string eventKey,
        string workspaceKey,
        string holdToken,
        List<string> objects, string? bookId);

    // hold ticket 
    Task<(ChangeObjectStatusResult data, string? error)> HoldTicketAsync(string eventKey,
        string workspaceKey,
        int minutes,
        List<string> objects);

    Task<(Dictionary<string, EventObjectInfo> objs, string? error)> RetrieveObjectAsync(string eventKey,
        string workspaceKey,
        List<string> objectKeys);

    // delete event
    Task<(bool? state, string? error)> DeleteEvent(string eventKey, string workspaceKey);

    // get EventReports
    Task<(Dictionary<string, EventReportSummaryItem> summaryItem, string? error)> GetEventReports(string eventKey,
        string workspaceKey);

    // change objects state 
    Task<(ChangeObjectStatusResult data, string? error)> ChangeObjectStatusAsync(List<ChangeObjectStateSeatIo> data,
        string status);
    
    
    // remove all category from charts
    Task<(bool? state, string? error)> RemoveAllCategoryFromChartAsync(string chartKey, string workspaceKey);
    
    // add list of categories to chart
    Task<(bool? state, string? error)> AddCategoriesToChartAsync(string chartKey, string workspaceKey, List<Category> categories);
    
    // add category to object 
}

public class SeatIoService : ISeatIoService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly string _seatIoSecretKey;

    public SeatIoService(DataContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
        _seatIoSecretKey = _configuration["SeatIO:SecretKey"] ?? throw new InvalidOperationException("SeatIO:SecretKey is not configured");
    }

    private Task<(SeatsioClient client, string WorkspacePublicKey)> Login()
    {
        AppUser? user = null;
        string authorizationHeader = _httpContextAccessor?.HttpContext?.Request?.Headers["Authorization"];
        if (!string.IsNullOrEmpty(authorizationHeader) && (authorizationHeader.StartsWith("Bearer ") ||
                                                           authorizationHeader.StartsWith("bearer ")))
        {
            ClaimsPrincipal userClaims = _httpContextAccessor?.HttpContext?.User;
            Claim claim = userClaims?.FindFirst("id");

            if (claim != null)
            {
                user = _context.Users.Find(Guid.Parse(claim.Value));
            }
        }

        if (user == null)
        {
            throw new BadHttpRequestException("User not found");
        }

        if (user.WorkspacePublicKey == null)
        {
            throw new BadHttpRequestException("User does not have a workspace id");
        }

        var client = new SeatsioClient(Region.EU(), user.WorkspaceSecretKey, user.WorkspacePublicKey);

        return Task.FromResult((client, user.WorkspacePublicKey));

        // var client = new SeatsioClient(Region.EU(), "56e5371d-60dc-4c4c-941d-a5582bf19ef7",
        //     "7ed70378-bef8-4354-857f-f20960646737");
        // return client;
    }


    public async Task<(Workspace? workspace, string? error)> CreateWorkspaceAsync(string name)
    {
        var client = new SeatsioClient(Region.EU(), _seatIoSecretKey);
        var workspace = await client.Workspaces.CreateAsync(name, false);
        return (workspace, null);
    }


    // edit work space 
    public async Task<(Workspace? workspace, string? error)> EditWorkspaceAsync(string name, string key)
    {
        var client = new SeatsioClient(Region.EU(), _seatIoSecretKey);
        await client.Workspaces.UpdateAsync(key, name);
        return (new Workspace(), null);
    }


    public async Task<(Workspace? workspace, string? error)> DeleteWorkspaceAsync(string key)
    {
        var client = new SeatsioClient(Region.EU(), _seatIoSecretKey);
        await client.Workspaces.DeactivateAsync(key);
        return (new Workspace(), null);
    }


    public async Task<(List<Chart>? charts, int? totalCount, string? error)> GetChartAsync()
    {
        var (client, workspaceKey) = await Login();
        var chart = await client.Charts.ListFirstPageAsync(pageSize: 20, expandEvents: true);
        return (chart.Items, 20, null);
    }

    
    // copy chart 
    public async Task<(Chart? chart, string? error)> CopyChartAsync(string key, string workspaceKey , string eventName)
    {
        var client = new SeatsioClient(Region.EU(), _seatIoSecretKey, workspaceKey);
        var chart = await client.Charts.CopyAsync(key);
        // update chart 
        await client.Charts.UpdateAsync(chart.Key, eventName);
        return (chart, null);
    }

    public async Task<(List<Event>? events, string? error)> GetAllEvent()
    {
        var client = new SeatsioClient(Region.EU(), _seatIoSecretKey);
        var events = await client.Events.ListFirstPageAsync();
        return (events.Items, null);
    }
    

    public async Task<(Event? events, string? error)> RetrieveEventAsync(string name, string workSpaceKey)
    {
        var client = new SeatsioClient(Region.EU(), _seatIoSecretKey, workSpaceKey);
        var events = await client.Events.RetrieveAsync(name);
        return (events, null);
    }


    public async Task<(List<Category>? categories, int? totalCount, string? error)>
        GetCategoryByChartKeyAsync(string key)
    {
        var (client, workspaceKey) = await Login();
        var categories = await client.Charts.ListCategoriesAsync(key);
        return (categories.ToList(), 20, null);
    }

    public async Task<(Category category, string? error)> UpdateCategory(string chartKey, string categoryKey,
        string? workspaceKey, CategoryUpdateParams categoryUpdateParams)
    {
        var client = new SeatsioClient(Region.EU(), _seatIoSecretKey, workspaceKey);
        await client.Charts.UpdateCategoryAsync(chartKey, categoryKey, categoryUpdateParams);
        await client.Charts.PublishDraftVersionAsync(chartKey);
        return (new Category(), null);
    }


    // update events 
    public async Task<(Event? events, string? error)> UpdateEventAsync(EventEntity eventEntity, string chartKey,
        string workspaceKey)
    {
        var client = new SeatsioClient(Region.EU(), _seatIoSecretKey, workspaceKey);
        CreateEventParams createEventParams = new CreateEventParams();

        if (eventEntity.StartEvent != null)
        {
            createEventParams.WithDate(DateOnly.FromDateTime((DateTime)eventEntity.StartEvent));
        }

        createEventParams.WithName(eventEntity.Name);

        var events = await client.Events.CreateAsync(chartKey, createEventParams);
        return (events, null);
    }


    public async Task<(Chart? chart, string workspaceKey, string? error)> CreateChartAsync(string name,
        List<Category> categories)
    {
        var (client, workspaceKey) = await Login();
        var chart = await client.Charts.CreateAsync(name, categories: categories);

        return (chart, workspaceKey, null);
    }

    public async Task<(bool success, string? error)> DeleteChartAsync(string key)
    {
        var (client, workspaceKey) = await Login();
        await client.Charts.MoveToArchiveAsync(key);
        return (true, null);
    }


    public async Task<(Event? events, string? error)> CreateEventAsync(EventEntity eventEntity, string chartKey,
        string workspaceKey)
    {
        var client = new SeatsioClient(Region.EU(), _seatIoSecretKey, workspaceKey);
        CreateEventParams createEventParams = new CreateEventParams();
        if (eventEntity.StartEvent != null)
            createEventParams.WithDate(DateOnly.FromDateTime((DateTime)eventEntity.StartEvent));
        createEventParams.WithName(eventEntity.Name);


        var events = await client.Events.CreateAsync(chartKey, createEventParams);
        return (events, null);
    }

    public async Task<(Dictionary<string, EventObjectInfo> objs, string? error)> RetrieveObjectAsync(
        List<string> objectKeys)
    {
        var (client, workspaceKey) = await Login();
        var objects = await
            client.Events.RetrieveObjectInfosAsync("639d062b-22d0-4452-b5e5-926f9e7044a5", objectKeys.ToArray());

        return (objects, null);
    }

    public async Task<(ChartValidationResult? data, string? error)> ValidatePublishedVersionAsync(string chartKey,
        string workspaceKey)
    {
        var client = new SeatsioClient(Region.EU(), _seatIoSecretKey, workspaceKey);
        var validation = await client.Charts.ValidatePublishedVersionAsync(chartKey);

        List<string> errors = new();

        errors.AddRange(validation.Errors);
        errors.AddRange(validation.Warnings);

        if (errors.Count > 0)
        {
            return (null, JsonConvert.SerializeObject(errors));
        }

        return (validation, null);
    }
    
    
    // validate druft version
    public async Task<(ChartValidationResult? data, string? error)> ValidateDraftVersionAsync(string chartKey,
        string workspaceKey)
    {

        try
        {
            var client = new SeatsioClient(Region.EU(), _seatIoSecretKey, workspaceKey);
            var validation = await client.Charts.ValidateDraftVersionAsync(chartKey);
        
            List<string> errors = new();

            errors.AddRange(validation.Errors);
            errors.AddRange(validation.Warnings);

            if (errors.Count > 0)
            {
                return (null, JsonConvert.SerializeObject(errors));
            }

            return (validation, null);
        }
        catch (Exception e)
        {
            // log error
            return (new ChartValidationResult(), null);
        }
       
    }
    
    


    public async Task<(List<Event>? events, int? totalCount, string? error)> GetAllEvents()
    {
        var (client, workspaceKey) = await Login();
        var events = await client.Events.ListFirstPageAsync();
        return (events.Items, 20, null);
    }

    public async Task<(bool? state, string error)> AddCategoryToChartAsync(string chartKey,
        string workspaceKey,
        Category category)
    {
        var client = new SeatsioClient(Region.EU(), _seatIoSecretKey, workspaceKey);

        try
        {
            await client.Charts.AddCategoryAsync(chartKey, category);

            return (true, null!);
        }
        catch (Exception e)
        {
            return (null, e.Message);
        }
    }

    public async Task<bool> RemoveCategoryFromChartAsync(string chartKey,
        string workspaceKey,
        RemoveCategoryForm category)
    {
        try
        {
            var client = new SeatsioClient(Region.EU(), _seatIoSecretKey, workspaceKey);
            await client.Charts.RemoveCategoryAsync(chartKey, category.CategoryKey);
        }
        catch (Exception e)
        {
            return false;
        }

        return true;

        throw new BadHttpRequestException("Error removing category from chart");
    }


    public async Task<(ChangeObjectStatusResult data, string? error)> BookTicketAsync(string eventKey,
        string workspaceKey,
        string holdToken,
        List<string> objects, string? bookId)
    {
        var client = new SeatsioClient(Region.EU(), _seatIoSecretKey, workspaceKey);
        try
        {
            var book = await client.Events.BookAsync(eventKey, objects, orderId: bookId, holdToken: holdToken);
            return (book, null);
        }
        catch (Exception e)
        {
            return (null!, e.Message);
        }
    }

    public async Task<(ChangeObjectStatusResult data, string? error)> HoldTicketAsync(string eventKey,
        string workspaceKey,
        int minutes,
        List<string> objects)
    {
        var client = new SeatsioClient(Region.EU(), _seatIoSecretKey, workspaceKey);
        try
        {
            var holdToken = await client.HoldTokens.CreateAsync(minutes);
            var held = await client.Events.HoldAsync(eventKey, objects, holdToken.Token);
            return (held, null);
        }
        catch (Exception e)
        {
            return (null!, e.Message);
        }
    }

    public async Task<(Dictionary<string, EventObjectInfo> objs, string? error)> RetrieveObjectAsync(string eventKey,
        string workspaceKey,
        List<string> objectKeys)
    {
        var client = new SeatsioClient(Region.EU(), _seatIoSecretKey, workspaceKey);
        var objects = await client.Events.RetrieveObjectInfosAsync(eventKey, objectKeys.ToArray());
        return (objects, null);
    }

    public async Task<(bool? state, string? error)> DeleteEvent(string eventKey, string workspaceKey)
    {
        var client = new SeatsioClient(Region.EU(), _seatIoSecretKey, workspaceKey);
        var delete = client.Events.DeleteAsync(eventKey);
        if (delete != null) return (true, null!);

        return (null, "Error deleting event");
    }

    public async Task<(Dictionary<string, EventReportSummaryItem> summaryItem, string? error)> GetEventReports(
        string eventKey, string workspaceKey)
    {
        var client = new SeatsioClient(Region.EU(), _seatIoSecretKey, workspaceKey);
        var report = await client.EventReports.SummaryByAvailabilityReasonAsync(eventKey);
        return (report, null);
    }

    public async Task<(ChangeObjectStatusResult data, string? error)> ChangeObjectStatusAsync(
        List<ChangeObjectStateSeatIo> data, string status)
    {
        data.ForEach(x =>
        {
            var clinet = new SeatsioClient(Region.EU(), _seatIoSecretKey, x.WorkspaceKey);
            var changeState = clinet.Events.ChangeObjectStatusAsync(x.EventKey, x.ObjectKeys, status);
        });

        return (new ChangeObjectStatusResult(), null);
    }
    
    
    // remove all category from charts 
    public async Task<(bool? state, string? error)> RemoveAllCategoryFromChartAsync(string chartKey, string workspaceKey)
    {
        var client = new SeatsioClient(Region.EU(), _seatIoSecretKey, workspaceKey);
        try
        {
            var categories = await client.Charts.ListCategoriesAsync(chartKey);
            foreach (var category in categories)
            {
                await client.Charts.RemoveCategoryAsync(chartKey, category.Key);
            }
            
            return (true, null);
        }
        catch (Exception e)
        {
            return (null, e.Message);
        }
    }
    
    
    // add list of categories to chart 
    public async Task<(bool? state, string? error)> AddCategoriesToChartAsync(string chartKey, string workspaceKey, List<Category> categories)
    {
        var client = new SeatsioClient(Region.EU(), _seatIoSecretKey, workspaceKey);
        try
        {
            foreach (var category in categories)
            {
                await client.Charts.AddCategoryAsync(chartKey, category);
            }
            
            return (true, null);
        }
        catch (Exception e)
        {
            return (null, e.Message);
        }
    }

  
   
}