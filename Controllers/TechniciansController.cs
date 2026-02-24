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
        var technicians = _db.Db.Queryable<User>().Where(u => u.IsTechnician).OrderByDescending(u => u.CreateTime).ToList();
        var allUsers = _db.Db.Queryable<User>().Where(u => !u.IsTechnician).ToList();
        ViewBag.AllUsers = allUsers;
        return View(technicians);
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreateTechnician(string userName, string password, string? realName, string? phone)
    {
        var (ok, msg) = _auth.Register(userName ?? "", password ?? "", realName, phone, false);
        if (!ok)
        {
            ViewBag.Error = msg;
            ViewBag.UserName = userName;
            ViewBag.RealName = realName;
            ViewBag.Phone = phone;
            return View();
        }
        var user = _db.Db.Queryable<User>().First(u => u.UserName == userName!.Trim());
        if (user != null)
        {
            user.IsTechnician = true;
            _db.Db.Updateable(user).ExecuteCommand();
        }
        TempData["Success"] = "师傅账号已创建";
        return RedirectToAction(nameof(Index));
    }
}
