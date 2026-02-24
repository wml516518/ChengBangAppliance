using ApplianceRepair.Models;
using ApplianceRepair.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApplianceRepair.Controllers;

[Authorize(Roles = "Admin")]
public class ServiceItemsController : Controller
{
    private readonly SqlSugarService _db;

    public ServiceItemsController(SqlSugarService db)
    {
        _db = db;
    }

    public IActionResult Index(int? typeId)
    {
        var types = _db.Db.Queryable<ServiceType>().OrderBy(t => t.SortOrder).ToList();
        ViewBag.Types = types;
        ViewBag.CurrentTypeId = typeId;

        var query = _db.Db.Queryable<ServiceItem>().OrderBy(i => i.SortOrder);
        if (typeId.HasValue)
            query = query.Where(i => i.ServiceTypeId == typeId.Value);

        var list = query.ToList();
        return View(list);
    }

    [HttpGet]
    public IActionResult Create(int? typeId)
    {
        ViewBag.Types = _db.Db.Queryable<ServiceType>().OrderBy(t => t.SortOrder).ToList();
        return View("Edit", new ServiceItem { ServiceTypeId = typeId ?? 0, SortOrder = 0, IsEnabled = true });
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var item = _db.Db.Queryable<ServiceItem>().First(x => x.Id == id);
        if (item == null) return NotFound();
        ViewBag.Types = _db.Db.Queryable<ServiceType>().OrderBy(t => t.SortOrder).ToList();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(ServiceItem model)
    {
        model.Name = (model.Name ?? "").Trim();
        if (string.IsNullOrEmpty(model.Name))
        {
            ModelState.AddModelError("Name", "请输入项目名称");
            ViewBag.Types = _db.Db.Queryable<ServiceType>().OrderBy(t => t.SortOrder).ToList();
            return View(model);
        }
        if (model.ServiceTypeId <= 0)
        {
            ModelState.AddModelError("ServiceTypeId", "请选择所属类型");
            ViewBag.Types = _db.Db.Queryable<ServiceType>().OrderBy(t => t.SortOrder).ToList();
            return View(model);
        }

        if (model.Id == 0)
        {
            _db.Db.Insertable(model).ExecuteCommand();
            TempData["Success"] = "服务项目已添加";
        }
        else
        {
            var exist = _db.Db.Queryable<ServiceItem>().First(x => x.Id == model.Id);
            if (exist == null) return NotFound();
            exist.Name = model.Name;
            exist.ServiceTypeId = model.ServiceTypeId;
            exist.SortOrder = model.SortOrder;
            exist.IsEnabled = model.IsEnabled;
            _db.Db.Updateable(exist).ExecuteCommand();
            TempData["Success"] = "服务项目已更新";
        }
        return RedirectToAction(nameof(Index), new { typeId = model.ServiceTypeId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var item = _db.Db.Queryable<ServiceItem>().First(x => x.Id == id);
        var typeId = item?.ServiceTypeId;
        _db.Db.Deleteable<ServiceItem>().Where(x => x.Id == id).ExecuteCommand();
        TempData["Success"] = "已删除";
        return RedirectToAction(nameof(Index), new { typeId });
    }
}
