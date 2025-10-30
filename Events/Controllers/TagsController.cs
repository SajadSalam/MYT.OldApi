using Events.DATA.DTOs.Tag;
using Events.Services;
using Microsoft.AspNetCore.Mvc;

namespace Events.Controllers;

public class TagsController : BaseController
{
    private readonly ITagService _tagService;

    public TagsController(ITagService tagService)
    {
        _tagService = tagService;
    }


    [HttpPost]
    public async Task<IActionResult> CreateTagAsync([FromBody] TagForm tagForm) => Ok(await _tagService.CreateTagAsync(tagForm));
    
    [HttpGet]
    public async Task<IActionResult> GetTagsAsync([FromQuery] TagFilter filter) => Ok(await _tagService.GetTagsAsync(filter) , filter.PageNumber);
    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTagAsync(Guid id, [FromBody] TagUpdate tagUpdate) => Ok(await _tagService.UpdateTagAsync(id, tagUpdate));
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTagAsync(Guid id) => Ok(await _tagService.DeleteTagAsync(id));
    
    

}