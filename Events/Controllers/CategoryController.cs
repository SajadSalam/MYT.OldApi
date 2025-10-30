using Events.DATA.DTOs.Category;
using Events.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Events.Controllers;

[Authorize]
public class CategoryController : BaseController
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }
    
    [HttpGet("/api/categories/{chartId}")]
    public async Task<IActionResult> GetAllByCategory(Guid chartId) => Ok(await _categoryService.GetAllByCategory(chartId) , 20);
    
    [Authorize(Roles = "Admin,Provider")]
    [HttpPost("/api/categories/{chartId}")]
    public async Task<IActionResult> AddCategoryToChartAsync(Guid chartId  ,[FromBody]CategoryChartForm category) => Ok(await _categoryService.AddCategoryToChartAsync(Id,chartId , category));
    
    // update
    [Authorize(Roles = "Admin,Provider")]
    [HttpPut("/api/categories/{categoryId}")]
    public async Task<IActionResult> UpdateCategoryAsync(Guid categoryId, [FromBody]UpdateCategory category) => Ok(await _categoryService.UpdateCategoryAsync(categoryId, category));
    
    
    [HttpDelete("/api/categories/{categoryId}")]
    public async Task<IActionResult> RemoveCategoryFromChartAsync(Guid categoryId) => Ok(await _categoryService.RemoveCategoryFromChartAsync(categoryId));
}