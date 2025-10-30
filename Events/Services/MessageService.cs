using System;
using System.Drawing;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Events.DATA;
using Events.DATA.DTOs.SupportMessage;
using Events.Entities;
using Microsoft.EntityFrameworkCore;

namespace Events.Services;

public interface ISupportMessageService
{
    Task<(SupportMessageDto? messageDto, string? error)> Create(SupportMessageForm form);
    Task<(SupportMessageDto? messageDto, string? error)> Update(SupportMessageUpdate update, Guid id);
    Task<(SupportMessageDto? messageDto, string? error)> Delete(Guid id);
    Task<(SupportMessageDto? messageDto, string? error)> GetById(Guid id);
    Task<(List<SupportMessageDto>? messageDto, int? totalCount, string? error)> GetAll(SupportMessageFilter filter);

}

public class SupportMessageService : ISupportMessageService
{
    private readonly IMapper _mapper;
    private readonly DataContext _context;

    public SupportMessageService(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<(SupportMessageDto? messageDto, string? error)> Create(SupportMessageForm form)
    {
        var message = _mapper.Map<SupportMessage>(form);
        var result = await _context.SupportMessages.AddAsync(message);
        await _context.SaveChangesAsync();

        return (_mapper.Map<SupportMessageDto>(result.Entity), null);
    }

    public async Task<(SupportMessageDto? messageDto, string? error)> Delete(Guid id)
    {
        var message = await _context.SupportMessages
        .Where(x => x.Id == id && x.Deleted == false)
        .FirstOrDefaultAsync();

        if (message == null)
            return (null, "Not Found");

        message.Deleted = true;

        var result = _context.SupportMessages.Update(message);
        await _context.SaveChangesAsync();

        return (_mapper.Map<SupportMessageDto>(result.Entity), null);
    }

    public async Task<(List<SupportMessageDto>? messageDto, int? totalCount, string? error)> GetAll(SupportMessageFilter filter)
    {
        var messages = _context.SupportMessages
        .AsNoTracking()
        .Where(w => w.Deleted == false)
        .ProjectTo<SupportMessageDto>(_mapper.ConfigurationProvider);

        var totalCount = messages.Count();

        var messagesDto = await messages
        .Skip(filter.PageSize * (filter.PageNumber - 1))
        .Take(filter.PageSize).ToListAsync();

        return (messagesDto, totalCount, null);

    }

    public async Task<(SupportMessageDto? messageDto, string? error)> GetById(Guid id)
    {
       var message = await _context.SupportMessages
       .AsNoTracking()
        .Where(x => x.Id == id && x.Deleted == false)
        .FirstOrDefaultAsync();
        
        if (message == null)
            return (null, "Not found");

        return (_mapper.Map<SupportMessageDto>(message), null);
    }

    public async Task<(SupportMessageDto? messageDto, string? error)> Update(SupportMessageUpdate update, Guid id)
    {
        var message = await _context.SupportMessages
        .Where(x => x.Id == id && x.Deleted == false)
        .FirstOrDefaultAsync();
        if (message == null) return (null, "Not Found");

        _mapper.Map(update, message);

        var results = _context.SupportMessages.Update(message);

        await _context.SaveChangesAsync();

        return (_mapper.Map<SupportMessageDto>(results.Entity), null);

    }
}
