using AutoMapper;
using AutoMapper.QueryableExtensions;
using e_parliament.Interface;
using Events.DATA;
using Events.DATA.DTOs;
using Events.DATA.DTOs.PointOfSale;
using Events.DATA.DTOs.User;
using Events.Entities;
using Microsoft.EntityFrameworkCore;

namespace Events.Services;

public interface IPointOfSaleService
{
    Task<(UserDto? userDto, string? error)> CreatePointOfSale(Guid userId , UserRole role, CreatePointOfSaleForm createPointOfSaleForm);

    Task<(List<PointOfSaleDto>? pointOfSale, int? totalCount, string? error)> GetPointOfSale(PointOfSaleFilter filter);
    
    // get by id
    Task<(PointOfSaleDto? pointOfSaleDto, string? error)> GetPointOfSaleById(Guid id);
    
    // update 
    Task<(PointOfSaleDto? pointOfSaleDto, string? error)> UpdatePointOfSale(Guid id, UpdatePointOfSaleForm updatePointOfSaleForm);
    
    // delete
    Task<(PointOfSaleDto? pointOfSaleDto, string? error)> DeletePointOfSale(Guid id);
    
    
    
    
}

public class PointOfSaleService : IPointOfSaleService
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    private readonly ITokenService _tokenService;

    public PointOfSaleService(DataContext context, IMapper mapper, ITokenService tokenService)
    {
        _context = context;
        _mapper = mapper;
        _tokenService = tokenService;
    }

    public async Task<(UserDto? userDto, string? error)> CreatePointOfSale(Guid userId,
        UserRole role,
        CreatePointOfSaleForm createPointOfSaleForm)
    {
        
        var user = await _context.Users.AnyAsync(u => u.PhoneNumber == createPointOfSaleForm.PhoneNumber && u.Deleted != true);
        if (user) return (null, "رقم الهاتف موجود مسبقا");
       
        var pointOfSale = new PointOfSale()
        {
            FullName = createPointOfSaleForm.FullName,
            PhoneNumber = createPointOfSaleForm.PhoneNumber,
            Password = BCrypt.Net.BCrypt.HashPassword(createPointOfSaleForm.Password),
            Role = UserRole.PointOfSale,
            Address = createPointOfSaleForm.Address,
            Description = createPointOfSaleForm.Description,
            Image = createPointOfSaleForm.Image,
            Lat = createPointOfSaleForm.Lat,
            Lng = createPointOfSaleForm.Lng,
            PhoneNumbers = createPointOfSaleForm.PhoneNumbers,
            // ProviderId = pointOfSaleId,
        };

        var newUser = await _context.PointOfSales.AddAsync(pointOfSale);
        await _context.SaveChangesAsync();
        var userDto = _mapper.Map<UserDto>(newUser.Entity);
        var token = _tokenService.CreateToken(userDto, UserRole.PointOfSale);
        userDto.Token = token;
        return (userDto, null);
    }

    public async Task<(List<PointOfSaleDto>? pointOfSale, int? totalCount, string? error)> GetPointOfSale(
        PointOfSaleFilter filter)
    {
        var query = _context.PointOfSales.AsNoTracking()
                .Where(x => string.IsNullOrEmpty(filter.Name) || x.FullName.Contains(filter.Name))
                .Where(x => x.Deleted != true)
            ;

        var totalCount = await query.CountAsync();
        var pointOfSales = await query.Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ProjectTo<PointOfSaleDto>(_mapper.ConfigurationProvider).ToListAsync();

        return (pointOfSales, totalCount, null);
    }

    public async Task<(PointOfSaleDto? pointOfSaleDto, string? error)> GetPointOfSaleById(Guid id)
    {
        var pointOfSale = await _context.PointOfSales.ProjectTo<PointOfSaleDto>(_mapper.ConfigurationProvider).FirstOrDefaultAsync(x => x.Id == id && x.Deleted != true);
        if (pointOfSale == null) return (null, "النقطة غير موجودة");
        return (pointOfSale, null);
    }

    public async Task<(PointOfSaleDto? pointOfSaleDto, string? error)> UpdatePointOfSale(Guid id, UpdatePointOfSaleForm updatePointOfSaleForm)
    {
        var pointOfSale = await _context.PointOfSales.FirstOrDefaultAsync(x => x.Id == id && x.Deleted != true);
        if (pointOfSale == null) return (null, "النقطة غير موجودة");

        if (!string.IsNullOrEmpty(updatePointOfSaleForm.Password))
        {
            pointOfSale.Password = BCrypt.Net.BCrypt.HashPassword(updatePointOfSaleForm.Password);
        }
        
        _mapper.Map(updatePointOfSaleForm, pointOfSale);
        _context.PointOfSales.Update(pointOfSale);
        await _context.SaveChangesAsync();
        return (new PointOfSaleDto(), null);
        

    }

    public async Task<(PointOfSaleDto? pointOfSaleDto, string? error)> DeletePointOfSale(Guid id)
    {
        var pointOfSale = await _context.PointOfSales.FirstOrDefaultAsync(x => x.Id == id && x.Deleted != true);
        if (pointOfSale == null) return (null, "النقطة غير موجودة");
        pointOfSale.Deleted = true;
        _context.PointOfSales.Update(pointOfSale);
        await _context.SaveChangesAsync();
        return (new PointOfSaleDto(), null);
    }
}