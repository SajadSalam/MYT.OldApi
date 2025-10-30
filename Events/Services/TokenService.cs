using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using e_parliament.Interface;
using Events.DATA.DTOs.User;
using Events.Entities;
using Microsoft.IdentityModel.Tokens;

namespace Events.Services
{
    public class TokenService : ITokenService
    {
        private readonly SymmetricSecurityKey _key;

        public TokenService(IConfiguration config)
        {
            var tokenKey = config["TokenKey"];
            if (string.IsNullOrEmpty(tokenKey))
            {
                tokenKey = "super secret key mother fuckers";
            }
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));
        }
        public string CreateToken(UserDto user, UserRole role)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
                // add role Claim
                new Claim("id", user.Id.ToString()),
                new Claim(ClaimTypes.Role, role.ToString()),
                new Claim("Role", role.ToString()),

                // new Claim(JwtRegisteredClaimNames., user.Email.ToString()),
            };

            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(30),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);

        }

    }
}