using AutoMapper;
using AutoMapper.QueryableExtensions;
using Events.DATA;
using Events.DATA.DTOs.Tag;
using Events.Entities;
using Microsoft.EntityFrameworkCore;

namespace Events.Services;

public interface ITagService
{
    Task<(TagDto? tagDto, string? error)> CreateTagAsync(TagForm tagForm);

    // get all tags with pagination
    Task<(List<TagDto> tagDtos, int? totalCount, string? error)> GetTagsAsync(TagFilter filter);

    // update 
    Task<(TagDto? tagDto, string? error)> UpdateTagAsync(Guid id, TagUpdate tagUpdate);

    // delete

    Task<(bool? state, string? error)> DeleteTagAsync(Guid id);
}

public class TagService : ITagService
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public TagService(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<(TagDto? tagDto, string? error)> CreateTagAsync(TagForm tagForm)
    {
        var tag = new Tag() { Name = tagForm.Name ,Image=tagForm.Image };
        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();
        return (new TagDto() { Id = tag.Id, Name = tag.Name }, null);
    }

    public async Task<(List<TagDto> tagDtos, int? totalCount, string? error)> GetTagsAsync(TagFilter filter)
    {
        var query = _context.Tags.AsNoTracking()
            .Where(x => filter.Name == null || x.Name.Contains(filter.Name))
            .AsQueryable();

        var totalCount = await query.CountAsync();

        var tags = await query
            .OrderByDescending(x => x.EventTags.Count(et => et.Event.StartEvent >= DateTime.Now && et.Event.IsPublish == true && et.Event.EndEvent <= DateTime.Now))
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ProjectTo<TagDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return (tags, totalCount, null);
    }

    public async Task<(TagDto? tagDto, string? error)> UpdateTagAsync(Guid id, TagUpdate tagUpdate)
    {
        var tag = await _context.Tags.FirstOrDefaultAsync(x => x.Id == id);
        if (tag == null) return (null, "Tag Not Found");
        tag.Name = tagUpdate.Name ?? tag.Name;
        tag.Image = tagUpdate.Image ?? tag.Image;
        _context.Tags.Update(tag);
        await _context.SaveChangesAsync();
        return (new TagDto() { Id = tag.Id, Name = tag.Name }, null);
    }

    public async Task<(bool? state, string? error)> DeleteTagAsync(Guid id)
    {
        var tag = await _context.Tags.FirstOrDefaultAsync(x => x.Id == id);
        if (tag == null) return (null, "Tag Not Found");
        _context.Tags.Remove(tag);
        await _context.SaveChangesAsync();
        return (true, null);
    }
}