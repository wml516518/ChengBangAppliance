using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApplianceRepair.Models;
using Microsoft.IdentityModel.Tokens;

namespace ApplianceRepair.Services;

public class JwtService
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "chengbang-jwt-secret-key-min-32-chars!!"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim("IsAdmin", user.IsAdmin.ToString()),
            new Claim("IsTechnician", user.IsTechnician.ToString())
        };
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"] ?? "ApplianceRepair",
            audience: _config["Jwt:Audience"] ?? "ApplianceRepair",
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
