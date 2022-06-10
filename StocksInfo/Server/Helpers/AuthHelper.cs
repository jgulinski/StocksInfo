using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Shared.Models;

namespace Server.Helpers;

public class AuthHelper
{
    public static JwtSecurityToken GenerateToken(User user, string issuer, string key)
    {
        List<Claim> userClaims = new()
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.EmailAddress),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.IdUser.ToString())
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: issuer,
            claims: userClaims,
            expires: DateTime.Now.AddDays(28),
            signingCredentials: credentials
        );

        return token;
    }
}