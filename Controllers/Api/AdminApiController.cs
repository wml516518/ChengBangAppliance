using ApplianceRepair.Models;
using ApplianceRepair.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApplianceRepair.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class AdminApiController : ControllerBase
{
    private readonly SqlSugarService _db;
    private readonly AuthService _auth;

    public AdminApiController(SqlSugarService db, AuthService auth)
    {
        _db = db;
        _auth = auth;
    }

    private User? GetCurrentUser()
    {
        var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
              ?? User.FindFirst("nameid")?.Value
              ?? User.FindFirst("sub")?.Value;
        if (!int.TryParse(id, out var uid)) return null;
        return _db.Db.Queryable<User>().First(u => u.Id == uid);
    }

    private IActionResult Forbidden() => Ok(new { ok = false, msg = "无管理员权限" });

    // ==================== 自建工单 ====================

    [HttpGet("manual-orders")]
    public IActionResult GetManualOrders([FromQuery] int? status)
    {
        var user = GetCurrentUser();
        if (user == null || !user.IsAdmin) return Forbidden();

        var query = _db.Db.Queryable<ManualOrder>().OrderByDescending(o => o.CreateTime);
        if (status.HasValue)
            query = query.Where(o => o.Status == status.Value);
        var list = query.ToList();

        var typeIds = list.Select(o => o.ServiceTypeId).Distinct().ToList();
        var itemIds = list.Select(o => o.ServiceItemId).Distinct().ToList();
        var workerIds = list.Where(o => o.AssignedUserId.HasValue).Select(o => o.AssignedUserId!.Value).Distinct().ToList();

        var typeDict = _db.Db.Queryable<ServiceType>().Where(t => typeIds.Contains(t.Id)).ToList().ToDictionary(t => t.Id, t => t.Name);
        var itemDict = _db.Db.Queryable<ServiceItem>().Where(i => itemIds.Contains(i.Id)).ToList().ToDictionary(i => i.Id, i => i.Name);
        var workerDict = workerIds.Count > 0
            ? _db.Db.Queryable<User>().Where(u => workerIds.Contains(u.Id)).ToList().ToDictionary(u => u.Id, u => u.RealName ?? u.UserName)
            : new Dictionary<int, string>();

        var result = list.Select(o => new
        {
            o.Id, o.OrderNo, o.ServiceTypeId, o.ServiceItemId,
            typeName = typeDict.GetValueOrDefault(o.ServiceTypeId, "-"),
            itemName = itemDict.GetValueOrDefault(o.ServiceItemId, "-"),
            o.WarrantyType, o.Amount,
            o.AppointmentStart, o.AppointmentEnd,
            o.ContactName, o.ContactPhone, o.Area, o.Address, o.Remark,
            o.AssignedUserId, o.Status, o.CreateTime,
            workerName = o.AssignedUserId.HasValue ? workerDict.GetValueOrDefault(o.AssignedUserId.Value, "-") : ""
        });
        return Ok(new { ok = true, list = result });
    }

    [HttpGet("manual-orders/{id:int}")]
    public IActionResult GetManualOrder(int id)
    {
        var user = GetCurrentUser();
        if (user == null || !user.IsAdmin) return Forbidden();

        var o = _db.Db.Queryable<ManualOrder>().First(x => x.Id == id);
        if (o == null) return Ok(new { ok = false, msg = "工单不存在" });

        var typeName = _db.Db.Queryable<ServiceType>().First(t => t.Id == o.ServiceTypeId)?.Name ?? "-";
        var itemName = _db.Db.Queryable<ServiceItem>().First(i => i.Id == o.ServiceItemId)?.Name ?? "-";
        var workerName = o.AssignedUserId.HasValue
            ? (_db.Db.Queryable<User>().First(u => u.Id == o.AssignedUserId)?.RealName ?? "-") : "";
        var creator = _db.Db.Queryable<User>().First(u => u.Id == o.CreatedByUserId);

        return Ok(new
        {
            ok = true,
            order = new
            {
                o.Id, o.OrderNo, o.ServiceTypeId, o.ServiceItemId,
                typeName, itemName,
                o.WarrantyType, o.Amount,
                o.AppointmentStart, o.AppointmentEnd,
                o.ContactName, o.ContactPhone, o.Area, o.Address, o.Remark,
                o.AssignedUserId, workerName, o.Status,
                o.CreateTime, o.AssignTime, o.CompleteTime,
                creatorName = creator?.RealName ?? creator?.UserName ?? "-"
            }
        });
    }

    [HttpPost("manual-orders")]
    public IActionResult CreateManualOrder([FromBody] ManualOrderRequest req)
    {
        var user = GetCurrentUser();
        if (user == null || !user.IsAdmin) return Forbidden();

        if (req.ServiceTypeId <= 0) return Ok(new { ok = false, msg = "请选择订单类型" });
        if (req.ServiceItemId <= 0) return Ok(new { ok = false, msg = "请选择服务项目" });
        if (string.IsNullOrWhiteSpace(req.ContactName)) return Ok(new { ok = false, msg = "请填写联系人" });
        if (string.IsNullOrWhiteSpace(req.ContactPhone)) return Ok(new { ok = false, msg = "请填写联系方式" });

        var order = new ManualOrder
        {
            OrderNo = "SV" + DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(1000, 9999),
            ServiceTypeId = req.ServiceTypeId,
            ServiceItemId = req.ServiceItemId,
            WarrantyType = req.WarrantyType,
            Amount = req.Amount,
            AppointmentStart = req.AppointmentStart,
            AppointmentEnd = req.AppointmentEnd,
            ContactName = req.ContactName!.Trim(),
            ContactPhone = req.ContactPhone!.Trim(),
            Area = req.Area?.Trim(),
            Address = req.Address?.Trim(),
            Remark = req.Remark?.Trim(),
            CreatedByUserId = user.Id,
            CreateTime = DateTime.Now
        };

        if (req.AssignedUserId.HasValue && req.AssignedUserId > 0)
        {
            order.AssignedUserId = req.AssignedUserId;
            order.Status = 1;
            order.AssignTime = DateTime.Now;
        }

        _db.Db.Insertable(order).ExecuteCommand();
        return Ok(new { ok = true, id = order.Id });
    }

    [HttpPut("manual-orders/{id:int}")]
    public IActionResult UpdateManualOrder(int id, [FromBody] ManualOrderRequest req)
    {
        var user = GetCurrentUser();
        if (user == null || !user.IsAdmin) return Forbidden();

        var order = _db.Db.Queryable<ManualOrder>().First(o => o.Id == id);
        if (order == null) return Ok(new { ok = false, msg = "工单不存在" });

        order.ServiceTypeId = req.ServiceTypeId;
        order.ServiceItemId = req.ServiceItemId;
        order.WarrantyType = req.WarrantyType;
        order.Amount = req.Amount;
        order.AppointmentStart = req.AppointmentStart;
        order.AppointmentEnd = req.AppointmentEnd;
        order.ContactName = (req.ContactName ?? "").Trim();
        order.ContactPhone = (req.ContactPhone ?? "").Trim();
        order.Area = req.Area?.Trim();
        order.Address = req.Address?.Trim();
        order.Remark = req.Remark?.Trim();

        if (req.AssignedUserId.HasValue && req.AssignedUserId > 0)
        {
            if (order.AssignedUserId != req.AssignedUserId)
            {
                order.AssignedUserId = req.AssignedUserId;
                order.AssignTime = DateTime.Now;
                if (order.Status == 0) order.Status = 1;
            }
        }
        else
        {
            order.AssignedUserId = null;
        }

        _db.Db.Updateable(order).ExecuteCommand();
        return Ok(new { ok = true });
    }

    [HttpPost("manual-orders/{id:int}/status")]
    public IActionResult UpdateManualOrderStatus(int id, [FromBody] StatusRequest req)
    {
        var user = GetCurrentUser();
        if (user == null || !user.IsAdmin) return Forbidden();

        var order = _db.Db.Queryable<ManualOrder>().First(o => o.Id == id);
        if (order == null) return Ok(new { ok = false, msg = "工单不存在" });

        order.Status = req.Status;
        if (req.Status == 3) order.CompleteTime = DateTime.Now;
        _db.Db.Updateable(order).ExecuteCommand();
        return Ok(new { ok = true });
    }

    [HttpDelete("manual-orders/{id:int}")]
    public IActionResult DeleteManualOrder(int id)
    {
        var user = GetCurrentUser();
        if (user == null || !user.IsAdmin) return Forbidden();

        _db.Db.Deleteable<ManualOrder>().Where(o => o.Id == id).ExecuteCommand();
        return Ok(new { ok = true });
    }

    // ==================== 服务类型 ====================

    [HttpGet("service-types")]
    public IActionResult GetServiceTypes()
    {
        var user = GetCurrentUser();
        if (user == null || !user.IsAdmin) return Forbidden();

        var list = _db.Db.Queryable<ServiceType>().OrderBy(t => t.SortOrder).ToList();
        return Ok(new { ok = true, list });
    }

    [HttpPost("service-types")]
    public IActionResult CreateServiceType([FromBody] ServiceType model)
    {
        var user = GetCurrentUser();
        if (user == null || !user.IsAdmin) return Forbidden();

        model.Name = (model.Name ?? "").Trim();
        if (string.IsNullOrEmpty(model.Name)) return Ok(new { ok = false, msg = "请输入类型名称" });

        model.Id = 0;
        _db.Db.Insertable(model).ExecuteCommand();
        return Ok(new { ok = true });
    }

    [HttpPut("service-types/{id:int}")]
    public IActionResult UpdateServiceType(int id, [FromBody] ServiceType model)
    {
        var user = GetCurrentUser();
        if (user == null || !user.IsAdmin) return Forbidden();

        var exist = _db.Db.Queryable<ServiceType>().First(x => x.Id == id);
        if (exist == null) return Ok(new { ok = false, msg = "不存在" });

        exist.Name = (model.Name ?? "").Trim();
        exist.SortOrder = model.SortOrder;
        exist.IsEnabled = model.IsEnabled;
        _db.Db.Updateable(exist).ExecuteCommand();
        return Ok(new { ok = true });
    }

    [HttpDelete("service-types/{id:int}")]
    public IActionResult DeleteServiceType(int id)
    {
        var user = GetCurrentUser();
        if (user == null || !user.IsAdmin) return Forbidden();

        if (_db.Db.Queryable<ServiceItem>().Any(x => x.ServiceTypeId == id))
            return Ok(new { ok = false, msg = "该类型下有服务项目，请先删除" });

        _db.Db.Deleteable<ServiceType>().Where(x => x.Id == id).ExecuteCommand();
        return Ok(new { ok = true });
    }

    // ==================== 服务项目 ====================

    [HttpGet("service-items")]
    public IActionResult GetServiceItems([FromQuery] int? typeId)
    {
        var user = GetCurrentUser();
        if (user == null || !user.IsAdmin) return Forbidden();

        var query = _db.Db.Queryable<ServiceItem>().OrderBy(i => i.SortOrder);
        if (typeId.HasValue)
            query = query.Where(i => i.ServiceTypeId == typeId.Value);
        var list = query.ToList();

        var typeDict = _db.Db.Queryable<ServiceType>().ToList().ToDictionary(t => t.Id, t => t.Name);
        var result = list.Select(i => new
        {
            i.Id, i.ServiceTypeId, i.Name, i.SortOrder, i.IsEnabled,
            typeName = typeDict.GetValueOrDefault(i.ServiceTypeId, "-")
        });
        return Ok(new { ok = true, list = result });
    }

    [HttpGet("service-items-by-type/{typeId:int}")]
    public IActionResult GetServiceItemsByType(int typeId)
    {
        var list = _db.Db.Queryable<ServiceItem>()
            .Where(i => i.ServiceTypeId == typeId && i.IsEnabled)
            .OrderBy(i => i.SortOrder)
            .Select(i => new { i.Id, i.Name })
            .ToList();
        return Ok(list);
    }

    [HttpPost("service-items")]
    public IActionResult CreateServiceItem([FromBody] ServiceItem model)
    {
        var user = GetCurrentUser();
        if (user == null || !user.IsAdmin) return Forbidden();

        model.Name = (model.Name ?? "").Trim();
        if (string.IsNullOrEmpty(model.Name)) return Ok(new { ok = false, msg = "请输入项目名称" });
        if (model.ServiceTypeId <= 0) return Ok(new { ok = false, msg = "请选择所属类型" });

        model.Id = 0;
        _db.Db.Insertable(model).ExecuteCommand();
        return Ok(new { ok = true });
    }

    [HttpPut("service-items/{id:int}")]
    public IActionResult UpdateServiceItem(int id, [FromBody] ServiceItem model)
    {
        var user = GetCurrentUser();
        if (user == null || !user.IsAdmin) return Forbidden();

        var exist = _db.Db.Queryable<ServiceItem>().First(x => x.Id == id);
        if (exist == null) return Ok(new { ok = false, msg = "不存在" });

        exist.Name = (model.Name ?? "").Trim();
        exist.ServiceTypeId = model.ServiceTypeId;
        exist.SortOrder = model.SortOrder;
        exist.IsEnabled = model.IsEnabled;
        _db.Db.Updateable(exist).ExecuteCommand();
        return Ok(new { ok = true });
    }

    [HttpDelete("service-items/{id:int}")]
    public IActionResult DeleteServiceItem(int id)
    {
        var user = GetCurrentUser();
        if (user == null || !user.IsAdmin) return Forbidden();

        _db.Db.Deleteable<ServiceItem>().Where(x => x.Id == id).ExecuteCommand();
        return Ok(new { ok = true });
    }

    // ==================== 师傅管理 ====================

    [HttpGet("technicians")]
    public IActionResult GetTechnicians()
    {
        var user = GetCurrentUser();
        if (user == null || !user.IsAdmin) return Forbidden();

        var technicians = _db.Db.Queryable<User>().Where(u => u.IsTechnician).OrderByDescending(u => u.CreateTime).ToList()
            .Select(u => new { u.Id, u.UserName, u.RealName, u.Phone, u.CreateTime });
        var nonTech = _db.Db.Queryable<User>().Where(u => !u.IsTechnician).ToList()
            .Select(u => new { u.Id, u.UserName, u.RealName, u.Phone });
        return Ok(new { ok = true, technicians, availableUsers = nonTech });
    }

    [HttpPost("technicians/set/{userId:int}")]
    public IActionResult SetTechnician(int userId)
    {
        var user = GetCurrentUser();
        if (user == null || !user.IsAdmin) return Forbidden();

        var target = _db.Db.Queryable<User>().First(u => u.Id == userId);
        if (target == null) return Ok(new { ok = false, msg = "用户不存在" });

        target.IsTechnician = true;
        _db.Db.Updateable(target).ExecuteCommand();
        return Ok(new { ok = true });
    }

    [HttpPost("technicians/remove/{userId:int}")]
    public IActionResult RemoveTechnician(int userId)
    {
        var user = GetCurrentUser();
        if (user == null || !user.IsAdmin) return Forbidden();

        var target = _db.Db.Queryable<User>().First(u => u.Id == userId);
        if (target == null) return Ok(new { ok = false, msg = "用户不存在" });

        target.IsTechnician = false;
        _db.Db.Updateable(target).ExecuteCommand();
        return Ok(new { ok = true });
    }

    [HttpPost("technicians/create")]
    public IActionResult CreateTechnician([FromBody] CreateTechRequest req)
    {
        var user = GetCurrentUser();
        if (user == null || !user.IsAdmin) return Forbidden();

        var (ok, msg) = _auth.Register(req.UserName ?? "", req.Password ?? "", req.RealName, req.Phone, false);
        if (!ok) return Ok(new { ok = false, msg });

        var newUser = _db.Db.Queryable<User>().First(u => u.UserName == req.UserName!.Trim());
        if (newUser != null)
        {
            newUser.IsTechnician = true;
            _db.Db.Updateable(newUser).ExecuteCommand();
        }
        return Ok(new { ok = true });
    }

    // ==================== 师傅列表（供选择） ====================

    [HttpGet("workers")]
    public IActionResult GetWorkers()
    {
        var list = _db.Db.Queryable<User>().Where(u => u.IsTechnician).ToList()
            .Select(u => new { u.Id, name = u.RealName ?? u.UserName, u.Phone });
        return Ok(list);
    }
}

public class ManualOrderRequest
{
    public int ServiceTypeId { get; set; }
    public int ServiceItemId { get; set; }
    public int WarrantyType { get; set; }
    public decimal Amount { get; set; }
    public DateTime? AppointmentStart { get; set; }
    public DateTime? AppointmentEnd { get; set; }
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public string? Area { get; set; }
    public string? Address { get; set; }
    public string? Remark { get; set; }
    public int? AssignedUserId { get; set; }
}

public class StatusRequest
{
    public int Status { get; set; }
}

public class CreateTechRequest
{
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string? RealName { get; set; }
    public string? Phone { get; set; }
}
