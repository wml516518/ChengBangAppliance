using ApplianceRepair.Models;
using ApplianceRepair.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApplianceRepair.Controllers;

[Authorize(Roles = "Admin")]
public class CategoriesController : Controller
{
    private readonly SqlSugarService _db;

    public CategoriesController(SqlSugarService db)
    {
        _db = db;
    }

    public IActionResult Index()
    {
        var list = _db.Db.Queryable<Category>().OrderBy(c => c.SortOrder).ToList();
        list = list.OrderBy(c => c.SortOrder).ThenBy(c => c.Name).ToList();
        return View(list);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View("Edit", new Category { SortOrder = 0 });
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var c = _db.Db.Queryable<Category>().First(x => x.Id == id);
        if (c == null) return NotFound();
        return View(c);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Category model)
    {
        model.Name = (model.Name ?? "").Trim();
        if (string.IsNullOrEmpty(model.Name))
        {
            ModelState.AddModelError("Name", "请输入分类名称");
            return View(model);
        }

        if (model.Id == 0)
        {
            _db.Db.Insertable(model).ExecuteCommand();
            TempData["Success"] = "分类已添加";
        }
        else
        {
            var exist = _db.Db.Queryable<Category>().First(x => x.Id == model.Id);
            if (exist == null) return NotFound();
            exist.Name = model.Name;
            exist.SortOrder = model.SortOrder;
            _db.Db.Updateable(exist).ExecuteCommand();
            TempData["Success"] = "分类已更新";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var n = _db.Db.Deleteable<Category>().Where(x => x.Id == id).ExecuteCommand();
        if (n > 0)
            TempData["Success"] = "已删除";
        return RedirectToAction(nameof(Index));
    }
}
