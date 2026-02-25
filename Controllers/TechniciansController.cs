using ApplianceRepair.Models;
using ApplianceRepair.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApplianceRepair.Controllers;

[Authorize(Roles = "Admin")]
public class TechniciansController : Controller
{
    private readonly SqlSugarService _db;
    private readonly AuthService _auth;

    public TechniciansController(SqlSugarService db, AuthService auth)
    {
        _db = db;
        _auth = auth;
    }

    public IActionResult Index()
    {
        var allUsers = _db.Db.Queryable<User>().OrderByDescending(u => u.CreateTime).ToList();
        var nonTechnicians = allUsers.Where(u => !u.IsTechnician).ToList();
        ViewBag.AllUsers = nonTechnicians;
        return View(allUsers);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SetTechnician(int userId)
    {
        var user = _db.Db.Queryable<User>().First(u => u.Id == userId);
        if (user == null) return NotFound();
        user.IsTechnician = true;
        _db.Db.Updateable(user).ExecuteCommand();
        TempData["Success"] = $"已将「{user.RealName ?? user.UserName}」设为师傅";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RemoveTechnician(int userId)
    {
        var user = _db.Db.Queryable<User>().First(u => u.Id == userId);
        if (user == null) return NotFound();
        user.IsTechnician = false;
        _db.Db.Updateable(user).ExecuteCommand();
        TempData["Success"] = $"已取消「{user.RealName ?? user.UserName}」的师傅身份";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult CreateTechnician()
    {
        return View();
    }

    /// <summary>角色：1=管理员 2=用户 3=师傅</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreateTechnician(string userName, string password, string confirmPassword, string? realName, string? phone, int role = 2)
    {
        if (password != confirmPassword)
        {
            ViewBag.Error = "两次输入的密码不一致";
            ViewBag.RealName = realName;
            ViewBag.Phone = phone;
            ViewBag.Role = role;
            return View();
        }
        bool isAdmin = (role == 1);
        bool isTechnician = (role == 3);
        var (ok, msg) = _auth.Register(userName ?? "", password ?? "", realName, phone, isAdmin, isTechnician);
        if (!ok)
        {
            ViewBag.Error = msg;
            ViewBag.RealName = realName;
            ViewBag.Phone = phone;
            ViewBag.Role = role;
            return View();
        }
        TempData["Success"] = "用户已创建";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var user = _db.Db.Queryable<User>().First(u => u.Id == id);
        if (user == null) return NotFound();
        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, string? realName, string? phone, int role, string? newPassword, string? confirmNewPassword)
    {
        var user = _db.Db.Queryable<User>().First(u => u.Id == id);
        if (user == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(newPassword))
        {
            if (newPassword.Length < 6)
            {
                ViewBag.Error = "新密码至少6个字符";
                return View(user);
            }
            if (newPassword != confirmNewPassword)
            {
                ViewBag.Error = "两次输入的密码不一致";
                return View(user);
            }
            user.PasswordHash = AuthService.HashPassword(newPassword);
        }

        user.RealName = realName?.Trim();
        user.Phone = phone?.Trim();
        user.IsAdmin = (role == 1);
        user.IsTechnician = (role == 3);
        _db.Db.Updateable(user).ExecuteCommand();
        TempData["Success"] = "用户信息已更新";
        return RedirectToAction(nameof(Index));
    }
}
