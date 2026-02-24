using ApplianceRepair.Models;
using ApplianceRepair.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApplianceRepair.Controllers;

[Authorize(Roles = "Admin")]
public class ServiceTypesController : Controller
{
    private readonly SqlSugarService _db;

    public ServiceTypesController(SqlSugarService db)
    {
        _db = db;
    }

    public IActionResult Index()
    {
        var list = _db.Db.Queryable<ServiceType>().OrderBy(t => t.SortOrder).ToList();
        return View(list);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View("Edit", new ServiceType { SortOrder = 0, IsEnabled = true });
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var item = _db.Db.Queryable<ServiceType>().First(x => x.Id == id);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(ServiceType model)
    {
        model.Name = (model.Name ?? "").Trim();
        if (string.IsNullOrEmpty(model.Name))
        {
            ModelState.AddModelError("Name", "请输入类型名称");
            return View(model);
        }

        if (model.Id == 0)
        {
            _db.Db.Insertable(model).ExecuteCommand();
            TempData["Success"] = "服务类型已添加";
        }
        else
        {
            var exist = _db.Db.Queryable<ServiceType>().First(x => x.Id == model.Id);
            if (exist == null) return NotFound();
            exist.Name = model.Name;
            exist.SortOrder = model.SortOrder;
            exist.IsEnabled = model.IsEnabled;
            _db.Db.Updateable(exist).ExecuteCommand();
            TempData["Success"] = "服务类型已更新";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var hasItems = _db.Db.Queryable<ServiceItem>().Any(x => x.ServiceTypeId == id);
        if (hasItems)
        {
            TempData["Success"] = "该类型下有服务项目，请先删除服务项目";
            return RedirectToAction(nameof(Index));
        }
        _db.Db.Deleteable<ServiceType>().Where(x => x.Id == id).ExecuteCommand();
        TempData["Success"] = "已删除";
        return RedirectToAction(nameof(Index));
    }
}
