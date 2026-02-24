using ApplianceRepair.Models;
using ApplianceRepair.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace ApplianceRepair.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class WorkerApiController : ControllerBase
{
    private readonly SqlSugarService _db;
    private readonly IWebHostEnvironment _env;

    public WorkerApiController(SqlSugarService db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    private User? GetCurrentUser()
    {
        var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
              ?? User.FindFirst("nameid")?.Value
              ?? User.FindFirst("sub")?.Value;
        if (!int.TryParse(id, out var uid)) return null;
        return _db.Db.Queryable<User>().First(u => u.Id == uid);
    }

    [HttpGet("my-orders")]
    public IActionResult GetMyOrders([FromQuery] int? status)
    {
        var user = GetCurrentUser();
        if (user == null || !user.IsTechnician) return Ok(new { ok = false, msg = "非师傅账号" });

        var query = _db.Db.Queryable<ManualOrder>()
            .Where(o => o.AssignedUserId == user.Id)
            .OrderByDescending(o => o.CreateTime);
        if (status.HasValue)
            query = query.Where(o => o.Status == status.Value);

        var list = query.ToList();
        var typeIds = list.Select(o => o.ServiceTypeId).Distinct().ToList();
        var itemIds = list.Select(o => o.ServiceItemId).Distinct().ToList();
        var typeDict = _db.Db.Queryable<ServiceType>().Where(t => typeIds.Contains(t.Id)).ToList().ToDictionary(t => t.Id, t => t.Name);
        var itemDict = _db.Db.Queryable<ServiceItem>().Where(i => itemIds.Contains(i.Id)).ToList().ToDictionary(i => i.Id, i => i.Name);

        var result = list.Select(o => new
        {
            o.Id, o.OrderNo, o.ServiceTypeId, o.ServiceItemId,
            typeName = typeDict.GetValueOrDefault(o.ServiceTypeId, "-"),
            itemName = itemDict.GetValueOrDefault(o.ServiceItemId, "-"),
            o.WarrantyType, o.Amount,
            o.ContactName, o.ContactPhone, o.Area, o.Address,
            o.Status, o.CreateTime, o.AppointmentStart, o.AppointmentEnd
        });
        return Ok(new { ok = true, list = result });
    }

    [HttpGet("my-orders/{id:int}")]
    public IActionResult GetMyOrder(int id)
    {
        var user = GetCurrentUser();
        if (user == null || !user.IsTechnician) return Ok(new { ok = false, msg = "非师傅账号" });

        var order = _db.Db.Queryable<ManualOrder>().First(o => o.Id == id && o.AssignedUserId == user.Id);
        if (order == null) return Ok(new { ok = false, msg = "工单不存在或非您的工单" });

        var typeName = _db.Db.Queryable<ServiceType>().First(t => t.Id == order.ServiceTypeId)?.Name ?? "-";
        var itemName = _db.Db.Queryable<ServiceItem>().First(i => i.Id == order.ServiceItemId)?.Name ?? "-";

        List<string> photos = new();
        if (!string.IsNullOrEmpty(order.Photos))
        {
            try { photos = JsonSerializer.Deserialize<List<string>>(order.Photos) ?? new(); } catch { }
        }

        return Ok(new
        {
            ok = true,
            order = new
            {
                order.Id, order.OrderNo, order.ServiceTypeId, order.ServiceItemId,
                typeName, itemName,
                order.WarrantyType, order.Amount,
                order.ContactName, order.ContactPhone, order.Area, order.Address,
                order.Remark, order.Status,
                order.AppointmentStart, order.AppointmentEnd,
                order.CreateTime, order.AssignTime, order.CompleteTime,
                photos
            }
        });
    }

    [HttpPut("my-orders/{id:int}")]
    public IActionResult UpdateMyOrder(int id, [FromBody] WorkerOrderUpdateRequest req)
    {
        var user = GetCurrentUser();
        if (user == null || !user.IsTechnician) return Ok(new { ok = false, msg = "非师傅账号" });

        var order = _db.Db.Queryable<ManualOrder>().First(o => o.Id == id && o.AssignedUserId == user.Id);
        if (order == null) return Ok(new { ok = false, msg = "工单不存在" });

        order.Amount = req.Amount;
        order.Remark = req.Remark ?? "";
        order.AppointmentStart = req.AppointmentStart;
        order.AppointmentEnd = req.AppointmentEnd;
        _db.Db.Updateable(order).UpdateColumns(o => new { o.Amount, o.Remark, o.AppointmentStart, o.AppointmentEnd })
            .ExecuteCommand();

        return Ok(new { ok = true });
    }

    [HttpPost("my-orders/{id:int}/status")]
    public IActionResult UpdateOrderStatus(int id, [FromBody] WorkerStatusRequest req)
    {
        var user = GetCurrentUser();
        if (user == null || !user.IsTechnician) return Ok(new { ok = false, msg = "非师傅账号" });

        var order = _db.Db.Queryable<ManualOrder>().First(o => o.Id == id && o.AssignedUserId == user.Id);
        if (order == null) return Ok(new { ok = false, msg = "工单不存在" });

        if (req.Status == 2 && order.Status == 1) order.Status = 2;
        else if (req.Status == 3 && (order.Status == 1 || order.Status == 2)) { order.Status = 3; order.CompleteTime = DateTime.Now; }
        else return Ok(new { ok = false, msg = "不允许的状态变更" });

        _db.Db.Updateable(order).UpdateColumns(o => new { o.Status, o.CompleteTime }).ExecuteCommand();
        return Ok(new { ok = true });
    }

    [HttpPost("upload-photo")]
    public async Task<IActionResult> UploadPhoto(IFormFile file)
    {
        var user = GetCurrentUser();
        if (user == null || !user.IsTechnician) return Ok(new { ok = false, msg = "非师傅账号" });

        if (file == null || file.Length == 0) return Ok(new { ok = false, msg = "未选择文件" });
        if (file.Length > 10 * 1024 * 1024) return Ok(new { ok = false, msg = "文件不能超过10MB" });

        var ext = Path.GetExtension(file.FileName).ToLower();
        if (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".webp")
            return Ok(new { ok = false, msg = "仅支持 jpg/png/webp 格式" });

        var uploadDir = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");
        Directory.CreateDirectory(uploadDir);

        var fileName = $"{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(uploadDir, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return Ok(new { ok = true, url = $"/uploads/{fileName}" });
    }

    [HttpPost("my-orders/{id:int}/photos")]
    public IActionResult AddPhoto(int id, [FromBody] AddPhotoRequest req)
    {
        var user = GetCurrentUser();
        if (user == null || !user.IsTechnician) return Ok(new { ok = false, msg = "非师傅账号" });

        var order = _db.Db.Queryable<ManualOrder>().First(o => o.Id == id && o.AssignedUserId == user.Id);
        if (order == null) return Ok(new { ok = false, msg = "工单不存在" });

        List<string> photos = new();
        if (!string.IsNullOrEmpty(order.Photos))
        {
            try { photos = JsonSerializer.Deserialize<List<string>>(order.Photos) ?? new(); } catch { }
        }
        photos.Add(req.Url);
        order.Photos = JsonSerializer.Serialize(photos);
        _db.Db.Updateable(order).UpdateColumns(o => new { o.Photos }).ExecuteCommand();

        return Ok(new { ok = true, photos });
    }

    [HttpPost("my-orders/{id:int}/photos/remove")]
    public IActionResult RemovePhoto(int id, [FromBody] AddPhotoRequest req)
    {
        var user = GetCurrentUser();
        if (user == null || !user.IsTechnician) return Ok(new { ok = false, msg = "非师傅账号" });

        var order = _db.Db.Queryable<ManualOrder>().First(o => o.Id == id && o.AssignedUserId == user.Id);
        if (order == null) return Ok(new { ok = false, msg = "工单不存在" });

        List<string> photos = new();
        if (!string.IsNullOrEmpty(order.Photos))
        {
            try { photos = JsonSerializer.Deserialize<List<string>>(order.Photos) ?? new(); } catch { }
        }
        photos.Remove(req.Url);
        order.Photos = JsonSerializer.Serialize(photos);
        _db.Db.Updateable(order).UpdateColumns(o => new { o.Photos }).ExecuteCommand();

        return Ok(new { ok = true, photos });
    }
}

public class WorkerOrderUpdateRequest
{
    public decimal Amount { get; set; }
    public string? Remark { get; set; }
    public DateTime? AppointmentStart { get; set; }
    public DateTime? AppointmentEnd { get; set; }
}

public class WorkerStatusRequest
{
    public int Status { get; set; }
}

public class AddPhotoRequest
{
    public string Url { get; set; } = "";
}
