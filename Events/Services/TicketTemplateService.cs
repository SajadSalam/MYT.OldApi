using AutoMapper;
using AutoMapper.QueryableExtensions;
using Events.DATA;
using Events.DATA.DTOs;
using Events.DATA.DTOs.Tickets.TicketTemplate;
using Events.Entities.Ticket;
using Microsoft.EntityFrameworkCore;

namespace Events.Services;

public interface ITicketTemplateService
{
    Task<(bool? status, string? error)> CreateTicketTemplateAsync(
        TicketTemplateForm ticketTemplateForm);

    Task<(TicketTemplateDto? ticketTemplateDto, string? error)> GetTicketTemplateAsync(Guid id);

    // get by event id
    Task<(TicketTemplateDto? ticketTemplateDtos, string? error)> GetByEventAsync(
        Guid eventId);

    Task<(List<TicketTemplateDto>? ticketTemplateDtos, int? totalCount, string? error)> GetAllTicketTemplateAsync(
        BaseFilter filter);

    Task<(TicketTemplateDto? ticketTemplateDto, string? error)> UpdateTicketTemplateAsync(Guid id,
        TicketTemplateForm ticketTemplateForm);


    Task<(bool? isDeleted, string? error)> DeleteTicketTemplateAsync(Guid id);
}

public class TicketTemplateService : ITicketTemplateService
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public TicketTemplateService(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<(TicketTemplateDto? ticketTemplateDto, string? error)> GetTicketTemplateAsync(Guid id)
    {
        var ticketTemplate = await _context.TicketTemplates.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (ticketTemplate == null) return (null, "Ticket template not found");
        var ticketTemplateDto = _mapper.Map<TicketTemplateDto>(ticketTemplate);
        return (ticketTemplateDto, null);
    }

    public async Task<(TicketTemplateDto? ticketTemplateDtos, string? error)> GetByEventAsync(Guid eventId)
    {
        var ticketTemplates = await _context.TicketTemplates.AsNoTracking()
            .Where(x => x.EventId == eventId)
            .ProjectTo<TicketTemplateDto>(_mapper.ConfigurationProvider).FirstOrDefaultAsync();
        return (ticketTemplates, null);
    }

    public async Task<(List<TicketTemplateDto>? ticketTemplateDtos, int? totalCount, string? error)>
        GetAllTicketTemplateAsync(BaseFilter filter)
    {
        var query = _context.TicketTemplates.AsNoTracking().AsQueryable();
        var totalCount = await query.CountAsync();
        var ticketTemplates = await query.Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize)
            .ProjectTo<TicketTemplateDto>(_mapper.ConfigurationProvider).ToListAsync();
        return (ticketTemplates, totalCount, null);
    }


    public async Task<(bool? status, string? error)> CreateTicketTemplateAsync(
        TicketTemplateForm ticketTemplateForm)
    {
        try
        {
            var eventz = await _context.Events.AsNoTracking()
                .Include(x => x.TicketTemplates)
                .FirstOrDefaultAsync(x => x.Id == ticketTemplateForm.EventId);
            if (eventz == null) return (null, "Event not found");

            TicketTemplate? ticketTemplate = null;

            if (eventz.TicketTemplates is { Count: > 0 })
            {
                ticketTemplate = eventz.TicketTemplates.FirstOrDefault();
                _mapper.Map(ticketTemplateForm, ticketTemplate);
                _context.TicketTemplates.Update(ticketTemplate);
                await _context.SaveChangesAsync();
                return (true, null);
            }

            ticketTemplate = _mapper.Map<TicketTemplate>(ticketTemplateForm);

            await _context.TicketTemplates.AddAsync(ticketTemplate);
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            return (null, e.Message);
        }

        return (true, null);
    }

    public async Task<(TicketTemplateDto? ticketTemplateDto, string? error)> UpdateTicketTemplateAsync(Guid id,
        TicketTemplateForm ticketTemplateForm)
    {
        var ticketTemplate = await _context.TicketTemplates.FirstOrDefaultAsync(x => x.Id == id);
        if (ticketTemplate == null) return (null, "Ticket template not found");
        var eventz = await _context.Events.FindAsync(ticketTemplateForm.EventId);
        if (eventz == null) return (null, "Event not found");
        _mapper.Map(ticketTemplateForm, ticketTemplate);
        await _context.SaveChangesAsync();
        var ticketTemplateDto = _mapper.Map<TicketTemplateDto>(ticketTemplate);
        return (ticketTemplateDto, null);
    }


    public async Task<(bool? isDeleted, string? error)> DeleteTicketTemplateAsync(Guid id)
    {
        var ticketTemplate = await _context.TicketTemplates.FirstOrDefaultAsync(x => x.Id == id);
        if (ticketTemplate == null) return (null, "Ticket template not found");
        _context.TicketTemplates.Remove(ticketTemplate);
        await _context.SaveChangesAsync();
        return (true, null);
    }
}