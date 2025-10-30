using Events.DATA.DTOs.User;
using Events.Entities;

namespace e_parliament.Interface
{
    public interface ITokenService
    {
        string CreateToken(UserDto user, UserRole role);
    }
}