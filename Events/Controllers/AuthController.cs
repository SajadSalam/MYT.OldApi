using Events.DATA.DTOs.Provider;
using Events.DATA.DTOs.User;
using Events.Entities;
using Events.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Events.Controllers
{
    public class AuthController : BaseController
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("/api/Login")]
        public async Task<ActionResult> Login(LoginForm loginForm) => Ok(await _userService.Login(loginForm));

        [HttpPost("/api/Register")]
        public async Task<ActionResult> Register(RegisterForm registerForm)
        {
            registerForm.Role = UserRole.User;
            return Ok(await _userService.Register(registerForm));
        }

        [Authorize]
        [HttpGet("/api/User/{id}")]
        public async Task<ActionResult> GetUser(Guid id) => OkObject(await _userService.GetUserById(id));

        [Authorize]
        [HttpPut("/api/User/{id}")]
        public async Task<ActionResult> UpdateUser(UpdateUserForm updateUserForm, Guid id) =>
            Ok(await _userService.UpdateUser(updateUserForm, id));

        [Authorize]
        [HttpDelete("/api/User/{id}")]
        public async Task<ActionResult> DeleteUser(Guid id) => Ok(await _userService.DeleteUser(id));


      
        
      
        
        
    }
}