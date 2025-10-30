using Events.DATA;
using Events.DATA.DTOs.Category;
using Events.Entities;
using Microsoft.EntityFrameworkCore;
using SeatsioDotNet.Charts;

namespace Events.Services;

public interface ICategoryService
{
    Task<(List<BaseCategory>? category, int? totalCount, string? error)> GetAllByCategory(Guid chartId);

    // add category to chatr
    Task<(bool? state, string? error)> AddCategoryToChartAsync(Guid userId, Guid chartId, CategoryChartForm category);


    // update 
    Task<(bool? state, string? error)> UpdateCategoryAsync(Guid categoryId, UpdateCategory category);

    // Remove category from chart
    Task<(bool? state, string? error)> RemoveCategoryFromChartAsync(Guid categoryId);
}

public class CategoryService : ICategoryService
{
    private readonly DataContext _context;
    private readonly ISeatIoService _seatIoService;

    public CategoryService(DataContext context, ISeatIoService seatIoService)
    {
        _context = context;
        _seatIoService = seatIoService;
    }


    public async Task<(List<BaseCategory>? category, int? totalCount, string? error)> GetAllByCategory(Guid chartId)
    {
        var chart = await _context.Charts.FindAsync(chartId);
        if (chart == null)
        {
            return (null!, null, "Chart not found");
        }

        var categories = await _context.Categories.AsNoTracking()
            .Where(x => x.ChartId == chartId).ToListAsync();


        return (categories, categories.Count(), null);
    }

    public async Task<(bool? state, string? error)> AddCategoryToChartAsync(Guid userId, Guid chartId,
        CategoryChartForm category)
    {
        var transaction = await _context.Database.BeginTransactionAsync();


        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return (null!, "User not found");

            var chart = await _context.Charts.FindAsync(chartId);
            if (chart == null) return (null!, "Chart not found");
            if (user.Role != UserRole.Admin && userId != chart.UserId)
                return (null!, "المستخدم ليس مسؤولا عن هذه الخريطة");


            var baseCategory = new BaseCategory()
            {
                Name = category.Name,
                Color = category.Color,
                Price = category.Price,
                ChartId = chartId
            };

            var result = await _context.Categories.AddAsync(baseCategory);
            if (result.Entity == null!) return (null!, "Error adding category");
            await _context.SaveChangesAsync();

            var categoryAddResult = await _seatIoService.AddCategoryToChartAsync(chart.ChartKey,
                chart.WorkspaceKey,
                new Category(result.Entity.Id.ToString(), baseCategory.Name, baseCategory.Color));
            if (categoryAddResult.error != null) return (null!, categoryAddResult.error);


            await transaction.CommitAsync();

            return (true, null);
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            return (null!, e.Message);
        }
    }

    public async Task<(bool? state, string? error)> UpdateCategoryAsync(Guid categoryId, UpdateCategory category)
    {
        try
        {
            var baseCategory = await _context.Categories.AsNoTracking().Include(x => x.Chart)
                .FirstOrDefaultAsync(x => x.Id == categoryId);
            if (baseCategory == null) return (null!, "Category not found");

            if (category.Name != null) baseCategory.Name = category.Name;
            if (category.Color != null) baseCategory.Color = category.Color;
            if (category.Price != null) baseCategory.Price = (decimal)category.Price;

            var categoryUpdateDto = new CategoryUpdateParams()
            {
                Label = category.Name,
                Color = category.Color,
            };

            var updateSeatIo = await _seatIoService.UpdateCategory(baseCategory.Chart.ChartKey , baseCategory.Id.ToString() , baseCategory.Chart.WorkspaceKey , categoryUpdateDto);
            if (updateSeatIo.error != null) return (null!, updateSeatIo.error);

            _context.Categories.Update(baseCategory);
            await _context.SaveChangesAsync();
            return (true, null);
        }
        catch (Exception e)
        {
            return (null!, e.Message);
        }
    }

    public async Task<(bool? state, string? error)> RemoveCategoryFromChartAsync(Guid categoryId)
    {
        var category = await _context.Categories.FindAsync(categoryId);
        if (category == null) return (null!, "Category not found");

        var chart = await _context.Charts.FindAsync(category.ChartId);
        if (chart == null) return (null!, "Chart not found");

        var categoryRemoveResult =
            _seatIoService.RemoveCategoryFromChartAsync(chart.ChartKey, chart.WorkspaceKey,
                new RemoveCategoryForm(categoryId.ToString()));

        if (categoryRemoveResult.Result == null)
        {
            return (null!, "Error removing category from chart");
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return (true, null);
    }
}