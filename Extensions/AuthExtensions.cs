using System.Security.Claims;
using ApplianceRepair.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace ApplianceRepair.Extensions;

public static class AuthExtensions
{
    public static async Task SignInAsync(this HttpContext context, User user, bool rememberMe)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new("IsAdmin", user.IsAdmin.ToString())
        };
        if (user.IsAdmin)
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var props = new AuthenticationProperties
        {
            IsPersistent = rememberMe,
            ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(7) : null
        };
        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);
    }

    public static int? GetUserId(this ClaimsPrincipal user)
    {
        var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(id, out var uid) ? uid : null;
    }

    public static bool IsAdmin(this ClaimsPrincipal user)
    {
        return user.FindFirst("IsAdmin")?.Value == "True";
    }
}
