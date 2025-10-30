using AutoMapper;
using AutoMapper.QueryableExtensions;
using Events.DATA;
using Events.DATA.DTOs;
using Events.DATA.DTOs.Category;
using Events.DATA.DTOs.Chart;
using Events.Entities;
using Microsoft.EntityFrameworkCore;
using SeatsioDotNet.Charts;

namespace Events.Services;

public interface IChartService
{
    // create chart
    Task<(ChartDto? chart, string? error)> CreateChartAsync(ChartForm form, Guid userId);

    // get chart
    Task<(BaseDtoWithoutPagination<ChartDto> data, string? error)> GetChartAsync(ChartFilter filter, Guid? userId);
    Task<(ChartDto? chart, string? error)> GetChartByIdAsync(Guid id, Guid userId);
    

    // delete chart
    Task<(bool success, string? error)> DeleteChartAsync(Guid id);


}

public class ChartService : IChartService
{

    private readonly DataContext _context;
    private readonly IMapper _mapper;
    private readonly ISeatIoService _seatIoService;

    public ChartService(ISeatIoService seatIoService, IMapper mapper, DataContext context)
    {
        _seatIoService = seatIoService;
        _mapper = mapper;
        _context = context;
    }

    public async Task<(ChartDto? chart, string? error)> CreateChartAsync(ChartForm form, Guid userId)
    {
        var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var baseChart = new BaseChart()
            {
                Name = form.Name,
                Categories = _mapper.Map<List<BaseCategory>>(form.Categories)
            };
            _context.Charts.Add(baseChart);


            var (chart, workspaceKey, error) =
                await _seatIoService.CreateChartAsync(form.Name, _mapper.Map<List<Category>>(baseChart.Categories));
            if (error != null) return (null!, error);

            baseChart.RelatedChartId = chart.Id;
            baseChart.ChartKey = chart.Key;
            baseChart.PublishedVersionThumbnailUrl = chart.PublishedVersionThumbnailUrl;
            baseChart.DraftVersionThumbnailUrl = chart.DraftVersionThumbnailUrl;
            baseChart.WorkspaceKey = workspaceKey;
            baseChart.UserId = userId;
            baseChart.IsTemplate = true;
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return (_mapper.Map<ChartDto>(baseChart), null);
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            return (null!, e.Message);
        }
    }

    public async Task<(BaseDtoWithoutPagination<ChartDto> data, string? error)> GetChartAsync(ChartFilter filter, Guid? userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null) throw new UnauthorizedAccessException("المستخدم غير موجود");
        var charts = await _seatIoService.GetChartAsync();
        if (charts.error != null) return (null!, charts.error);

        var relatedIds = charts.charts!.Select(x => x.Id).ToList();

        var result = await _context.Categories
        .Include(x => x.Chart)
        .Where(x => relatedIds.Contains(x.Chart.RelatedChartId) && x.Deleted == false &&
        (filter.IsTemplate == null || filter.IsTemplate == x.Chart.IsTemplate)
        )
            // .Include(x => x.Categories)
            .Select(s => s.Chart)
            .ProjectTo<ChartDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        result.ForEach(x =>
        {
            x.Thumbnail = charts.charts!.FirstOrDefault(c => c.Key! == x.Key).PublishedVersionThumbnailUrl;
            x.Key = charts.charts!.FirstOrDefault(c => c.Key! == x.Key).Key;
        });


        return (new BaseDtoWithoutPagination<ChartDto>() { Data = result }, null);
    }

    public async Task<(ChartDto? chart, string? error)> GetChartByIdAsync(Guid id, Guid userId)
    {
        var chart = await _context.Charts.ProjectTo<ChartDto>(_mapper.ConfigurationProvider).FirstOrDefaultAsync(x=>x.Id==id);
        if (chart == null) return (null, "Chart not found");
        return (chart, null);
    }

    public async Task<(bool success, string? error)> DeleteChartAsync(Guid id)
    {
        var chart = await _context.Charts.FirstOrDefaultAsync(x => x.Id == id && x.Deleted == false);
        if (chart == null) return (false, "Chart not found");
        var (success, error) = await _seatIoService.DeleteChartAsync(chart.ChartKey);
        if (!success) return (false, error);
        chart.Deleted = true;
        _context.Charts.Update(chart);
        await _context.SaveChangesAsync();
        return (true, null);

    }


}