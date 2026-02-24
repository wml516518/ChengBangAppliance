using ApplianceRepair.Models;
using ApplianceRepair.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApplianceRepair.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class AuthApiController : ControllerBase
{
    private readonly AuthService _auth;
    private readonly JwtService _jwt;

    public AuthApiController(AuthService auth, JwtService jwt)
    {
        _auth = auth;
        _jwt = jwt;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public IActionResult Login([FromBody] LoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.UserName) || string.IsNullOrWhiteSpace(req?.Password))
            return Ok(new { ok = false, msg = "请输入用户名和密码" });
        var user = _auth.Login(req.UserName.Trim(), req.Password);
        if (user == null)
            return Ok(new { ok = false, msg = "用户名或密码错误" });
        var token = _jwt.GenerateToken(user);
        return Ok(new { ok = true, token, userName = user.UserName, userId = user.Id, isAdmin = user.IsAdmin, isTechnician = user.IsTechnician });
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public IActionResult Register([FromBody] RegisterRequest req)
    {
        if (req == null) return Ok(new { ok = false, msg = "参数无效" });
        var (ok, msg) = _auth.Register(req.UserName ?? "", req.Password ?? "", req.RealName, req.Phone, false);
        return Ok(new { ok, msg });
    }

    [HttpGet("info")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public IActionResult Info()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("nameid")?.Value
                  ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userId)) return Ok(new { ok = false });
        var user = _auth.GetById(int.Parse(userId));
        if (user == null) return Ok(new { ok = false });
        return Ok(new { ok = true, userId = user.Id, userName = user.UserName, realName = user.RealName, phone = user.Phone, isAdmin = user.IsAdmin, isTechnician = user.IsTechnician });
    }
}

public class LoginRequest
{
    public string? UserName { get; set; }
    public string? Password { get; set; }
}

public class RegisterRequest
{
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string? RealName { get; set; }
    public string? Phone { get; set; }
}
