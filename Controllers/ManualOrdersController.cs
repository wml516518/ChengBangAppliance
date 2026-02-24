using ApplianceRepair.Models;
using ApplianceRepair.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApplianceRepair.Controllers;

[Authorize(Roles = "Admin")]
public class ManualOrdersController : Controller
{
    private readonly SqlSugarService _db;

    public ManualOrdersController(SqlSugarService db)
    {
        _db = db;
    }

    public IActionResult Index(int? status)
    {
        ViewBag.CurrentStatus = status;
        var query = _db.Db.Queryable<ManualOrder>().OrderByDescending(o => o.CreateTime);
        if (status.HasValue)
            query = query.Where(o => o.Status == status.Value);
        var list = query.ToList();

        var typeIds = list.Select(o => o.ServiceTypeId).Distinct().ToList();
        var itemIds = list.Select(o => o.ServiceItemId).Distinct().ToList();
        var workerIds = list.Where(o => o.AssignedUserId.HasValue).Select(o => o.AssignedUserId!.Value).Distinct().ToList();

        ViewBag.TypeDict = _db.Db.Queryable<ServiceType>().Where(t => typeIds.Contains(t.Id)).ToList().ToDictionary(t => t.Id, t => t.Name);
        ViewBag.ItemDict = _db.Db.Queryable<ServiceItem>().Where(i => itemIds.Contains(i.Id)).ToList().ToDictionary(i => i.Id, i => i.Name);
        ViewBag.WorkerDict = workerIds.Count > 0
            ? _db.Db.Queryable<User>().Where(u => workerIds.Contains(u.Id)).ToList().ToDictionary(u => u.Id, u => u.RealName ?? u.UserName)
            : new Dictionary<int, string>();

        return View(list);
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.Types = _db.Db.Queryable<ServiceType>().Where(t => t.IsEnabled).OrderBy(t => t.SortOrder).ToList();
        ViewBag.Workers = _db.Db.Queryable<User>().Where(u => u.IsTechnician).ToList();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(ManualOrder model)
    {
        model.ContactName = (model.ContactName ?? "").Trim();
        model.ContactPhone = (model.ContactPhone ?? "").Trim();

        if (model.ServiceTypeId <= 0)
            ModelState.AddModelError("ServiceTypeId", "请选择订单类型");
        if (model.ServiceItemId <= 0)
            ModelState.AddModelError("ServiceItemId", "请选择服务项目");
        if (string.IsNullOrEmpty(model.ContactName))
            ModelState.AddModelError("ContactName", "请填写联系人");
        if (string.IsNullOrEmpty(model.ContactPhone))
            ModelState.AddModelError("ContactPhone", "请填写联系方式");

        if (!ModelState.IsValid)
        {
            ViewBag.Types = _db.Db.Queryable<ServiceType>().Where(t => t.IsEnabled).OrderBy(t => t.SortOrder).ToList();
            ViewBag.Workers = _db.Db.Queryable<User>().Where(u => u.IsTechnician).ToList();
            return View(model);
        }

        var adminName = User.Identity?.Name;
        var adminUser = _db.Db.Queryable<User>().First(u => u.UserName == adminName);

        model.OrderNo = "SV" + DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(1000, 9999);
        model.CreatedByUserId = adminUser?.Id ?? 0;
        model.CreateTime = DateTime.Now;
        model.Area = model.Area?.Trim();
        model.Address = model.Address?.Trim();
        model.Remark = model.Remark?.Trim();

        if (model.AssignedUserId.HasValue && model.AssignedUserId > 0)
        {
            model.Status = 1;
            model.AssignTime = DateTime.Now;
        }
        else
        {
            model.AssignedUserId = null;
            model.Status = 0;
        }

        _db.Db.Insertable(model).ExecuteCommand();
        TempData["Success"] = "工单已创建";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Detail(int id)
    {
        var order = _db.Db.Queryable<ManualOrder>().First(o => o.Id == id);
        if (order == null) return NotFound();

        PrepareDetailViewBag(order);
        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(ManualOrder model)
    {
        var order = _db.Db.Queryable<ManualOrder>().First(o => o.Id == model.Id);
        if (order == null) return NotFound();

        order.ServiceTypeId = model.ServiceTypeId;
        order.ServiceItemId = model.ServiceItemId;
        order.WarrantyType = model.WarrantyType;
        order.Amount = model.Amount;
        order.AppointmentStart = model.AppointmentStart;
        order.AppointmentEnd = model.AppointmentEnd;
        order.ContactName = (model.ContactName ?? "").Trim();
        order.ContactPhone = (model.ContactPhone ?? "").Trim();
        order.Area = model.Area?.Trim();
        order.Address = model.Address?.Trim();
        order.Remark = model.Remark?.Trim();

        if (model.AssignedUserId.HasValue && model.AssignedUserId > 0)
        {
            if (order.AssignedUserId != model.AssignedUserId)
            {
                order.AssignedUserId = model.AssignedUserId;
                order.AssignTime = DateTime.Now;
                if (order.Status == 0) order.Status = 1;
            }
        }
        else
        {
            order.AssignedUserId = null;
        }

        _db.Db.Updateable(order).ExecuteCommand();
        TempData["Success"] = "工单已保存";
        return RedirectToAction(nameof(Detail), new { id = order.Id });
    }

    private void PrepareDetailViewBag(ManualOrder order)
    {
        ViewBag.Types = _db.Db.Queryable<ServiceType>().Where(t => t.IsEnabled).OrderBy(t => t.SortOrder).ToList();
        ViewBag.Items = _db.Db.Queryable<ServiceItem>().Where(i => i.ServiceTypeId == order.ServiceTypeId && i.IsEnabled).OrderBy(i => i.SortOrder).ToList();
        ViewBag.Workers = _db.Db.Queryable<User>().Where(u => u.IsTechnician).ToList();
        var creator = _db.Db.Queryable<User>().First(u => u.Id == order.CreatedByUserId);
        ViewBag.CreatorName = creator?.RealName ?? creator?.UserName ?? "-";
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Assign(int id, int assignedUserId)
    {
        var order = _db.Db.Queryable<ManualOrder>().First(o => o.Id == id);
        if (order == null) return NotFound();
        if (assignedUserId <= 0)
        {
            TempData["Success"] = "请选择师傅";
            return RedirectToAction(nameof(Detail), new { id });
        }

        order.AssignedUserId = assignedUserId;
        order.Status = 1;
        order.AssignTime = DateTime.Now;
        _db.Db.Updateable(order).ExecuteCommand();
        TempData["Success"] = "已指派师傅";
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateStatus(int id, int status)
    {
        var order = _db.Db.Queryable<ManualOrder>().First(o => o.Id == id);
        if (order == null) return NotFound();

        order.Status = status;
        if (status == 3)
            order.CompleteTime = DateTime.Now;
        _db.Db.Updateable(order).ExecuteCommand();

        var statusName = status switch { 0 => "待指派", 1 => "已指派", 2 => "进行中", 3 => "已完成", 4 => "已取消", _ => "" };
        TempData["Success"] = $"工单状态已更新为「{statusName}」";
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        _db.Db.Deleteable<ManualOrder>().Where(o => o.Id == id).ExecuteCommand();
        TempData["Success"] = "工单已删除";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>根据服务类型获取服务项目 (AJAX)</summary>
    [HttpGet]
    public IActionResult GetServiceItems(int typeId)
    {
        var items = _db.Db.Queryable<ServiceItem>()
            .Where(i => i.ServiceTypeId == typeId && i.IsEnabled)
            .OrderBy(i => i.SortOrder)
            .Select(i => new { i.Id, i.Name })
            .ToList();
        return Json(items);
    }
}
