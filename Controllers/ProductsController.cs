using ApplianceRepair.Models;
using ApplianceRepair.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApplianceRepair.Controllers;

public class ProductsController : Controller
{
    private readonly SqlSugarService _db;
    private readonly IWebHostEnvironment _env;

    public ProductsController(SqlSugarService db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    [AllowAnonymous]
    public IActionResult Index(bool? onSaleOnly)
    {
        var query = _db.Db.Queryable<Product>().OrderByDescending(p => p.CreateTime);
        if (onSaleOnly == true)
            query = query.Where(p => p.IsOnSale);
        var list = query.ToList();
        return View(list);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Manage()
    {
        var list = _db.Db.Queryable<Product>().OrderByDescending(p => p.CreateTime).ToList();
        return View(list);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.Categories = _db.Db.Queryable<Category>().OrderBy(c => c.SortOrder).ToList().OrderBy(c => c.SortOrder).ThenBy(c => c.Name).ToList();
        return View("Edit", new ProductEditViewModel { IsOnSale = true });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult Edit(int id)
    {
        var p = _db.Db.Queryable<Product>().First(x => x.Id == id);
        if (p == null)
            return NotFound();
        ViewBag.Categories = _db.Db.Queryable<Category>().OrderBy(c => c.SortOrder).ToList().OrderBy(c => c.SortOrder).ThenBy(c => c.Name).ToList();
        var vm = new ProductEditViewModel
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            IsOnSale = p.IsOnSale,
            Price = p.Price,
            Category = p.Category,
            ImagePath = p.ImagePath
        };
        return View(vm);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProductEditViewModel vm, IFormFile? image, CancellationToken ct)
    {
        ViewBag.Categories = _db.Db.Queryable<Category>().OrderBy(c => c.SortOrder).ToList().OrderBy(c => c.SortOrder).ThenBy(c => c.Name).ToList();
        if (!ModelState.IsValid)
            return View(vm);

        string? savedPath = null;
        if (image != null && image.Length > 0)
        {
            var ext = Path.GetExtension(image.FileName).ToLowerInvariant();
            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".gif" && ext != ".webp")
            {
                ModelState.AddModelError("", "请上传图片文件（jpg/png/gif/webp）");
                return View(vm);
            }
            var uploadsDir = Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), "uploads");
            Directory.CreateDirectory(uploadsDir);
            var fileName = $"{Guid.NewGuid():N}{ext}";
            var filePath = Path.Combine(uploadsDir, fileName);
            await using (var stream = System.IO.File.Create(filePath))
                await image.CopyToAsync(stream, ct);
            savedPath = "/uploads/" + fileName;
        }

        if (vm.Id == 0)
        {
            var now = DateTime.Now;
            var p = new Product
            {
                Name = vm.Name,
                Description = vm.Description,
                IsOnSale = vm.IsOnSale,
                Price = vm.Price,
                Category = vm.Category,
                ImagePath = savedPath,
                CreateTime = now,
                UpdateTime = now
            };
            _db.Db.Insertable(p).ExecuteCommand();
            TempData["Success"] = "商品已添加";
        }
        else
        {
            var p = _db.Db.Queryable<Product>().First(x => x.Id == vm.Id);
            if (p == null)
                return NotFound();
            p.Name = vm.Name;
            p.Description = vm.Description;
            p.IsOnSale = vm.IsOnSale;
            p.Price = vm.Price;
            p.Category = vm.Category;
            if (savedPath != null)
                p.ImagePath = savedPath;
            p.UpdateTime = DateTime.Now;
            _db.Db.Updateable(p).ExecuteCommand();
            TempData["Success"] = "商品已更新";
        }
        return RedirectToAction(nameof(Manage));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ToggleSale(int id)
    {
        var p = _db.Db.Queryable<Product>().First(x => x.Id == id);
        if (p == null)
            return NotFound();
        p.IsOnSale = !p.IsOnSale;
        p.UpdateTime = DateTime.Now;
        _db.Db.Updateable(p).ExecuteCommand();
        TempData["Success"] = p.IsOnSale ? "已上架" : "已下架";
        return RedirectToAction(nameof(Manage));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var n = _db.Db.Deleteable<Product>().Where(x => x.Id == id).ExecuteCommand();
        if (n > 0)
            TempData["Success"] = "已删除";
        return RedirectToAction(nameof(Manage));
    }
}
