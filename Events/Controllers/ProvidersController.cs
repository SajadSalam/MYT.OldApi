using Events.DATA.DTOs.Provider;
using Events.DATA.DTOs.User;
using Events.Entities;
using Events.Services;
using Events.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Events.Controllers;

public class ProvidersController : BaseController
{
    
    private readonly IUserService _userService;

    public ProvidersController(IUserService userService)
    {
        _userService = userService;
    }
    
    
    [Authorize(Roles = "Admin")]
    [HttpPost("/api/providers")]
    public async Task<ActionResult<UserDto>> CreateProvider(ProviderForm createProviderForm)
    {
        RegisterForm registerForm = new RegisterForm
        {
            FullName = createProviderForm.FullName,
            Password = createProviderForm.Password,
            PhoneNumber = createProviderForm.PhoneNumber,
            Role = UserRole.Provider
        };

        return Ok(await _userService.Register(registerForm));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("/api/providers")]
    public async Task<ActionResult<Respons<UserWithoutTokenDto>>> GetAllProviders([FromQuery]UserFilter filter)
    {
        filter.Role = UserRole.Provider;
        return Ok(await _userService.GetAllUsers(filter) , filter.PageNumber);
    }

    
    [Authorize(Roles = "Admin")]
    [HttpPut("/api/providers/{id}")]
    public async Task<ActionResult<UserDto>> UpdateProvider(UpdateUserForm updateUserForm, Guid id)
    {
        return Ok(await _userService.UpdateUser(updateUserForm, id));
    }
    
    
    [Authorize(Roles = "Admin")]
    [HttpDelete("/api/providers/{id}")]
    public async Task<ActionResult<AppUser>> DeleteProvider(Guid id)
    {
        return Ok(await _userService.DeleteUser(id));
    }
    
    
    [Authorize(Roles = "Admin")]
    [HttpGet("/api/providers/{id}")]
    public async Task<ActionResult<UserDto>> GetProvider(Guid id)
    {
        return Ok(await _userService.GetUserById(id));
    }
    
   
    
}