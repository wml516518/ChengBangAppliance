using ApplianceRepair.Models;
using ApplianceRepair.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApplianceRepair.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class ClientApiController : ControllerBase
{
    private readonly SqlSugarService _db;

    public ClientApiController(SqlSugarService db)
    {
        _db = db;
    }

    [HttpGet("categories")]
    [AllowAnonymous]
    public IActionResult GetCategories()
    {
        var list = _db.Db.Queryable<Category>().OrderBy(c => c.SortOrder).ToList();
        var names = list.OrderBy(c => c.SortOrder).ThenBy(c => c.Name).Select(c => c.Name).ToList();
        return Ok(names);
    }

    [HttpGet("products")]
    [AllowAnonymous]
    public IActionResult GetProducts([FromQuery] string? category)
    {
        var query = _db.Db.Queryable<Product>().Where(p => p.IsOnSale);
        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.Category == category.Trim());
        var list = query.OrderByDescending(p => p.CreateTime).ToList();
        return Ok(list);
    }

    [HttpGet("products/{id:int}")]
    [AllowAnonymous]
    public IActionResult GetProduct(int id)
    {
        var p = _db.Db.Queryable<Product>().First(x => x.Id == id);
        if (p == null || !p.IsOnSale) return NotFound();
        return Ok(p);
    }
}
