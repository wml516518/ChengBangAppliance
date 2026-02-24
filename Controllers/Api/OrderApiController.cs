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
public class OrderApiController : ControllerBase
{
    private readonly SqlSugarService _db;

    public OrderApiController(SqlSugarService db)
    {
        _db = db;
    }

    private int? UserId
    {
        get
        {
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(id, out var uid) ? uid : null;
        }
    }

    [HttpPost("submit")]
    public IActionResult Submit([FromBody] SubmitOrderRequest req)
    {
        var userId = UserId;
        if (userId == null) return Ok(new { ok = false, msg = "请先登录" });

        var p = _db.Db.Queryable<Product>().First(x => x.Id == req.ProductId);
        if (p == null || !p.IsOnSale) return Ok(new { ok = false, msg = "商品不存在或已下架" });
        var quantity = req.Quantity < 1 ? 1 : req.Quantity;
        if (req.PaymentMethod != 1 && req.PaymentMethod != 2) return Ok(new { ok = false, msg = "请选择支付方式" });
        var contactName = (req.ContactName ?? "").Trim();
        var contactPhone = (req.ContactPhone ?? "").Trim();
        if (string.IsNullOrEmpty(contactName)) return Ok(new { ok = false, msg = "请填写联系人" });
        if (string.IsNullOrEmpty(contactPhone)) return Ok(new { ok = false, msg = "请填写联系电话" });

        var amount = p.Price * quantity;
        var orderNo = DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(1000, 9999);
        var now = DateTime.Now;
        var order = new Order
        {
            UserId = userId.Value,
            OrderNo = orderNo,
            Status = 0,
            TotalAmount = amount,
            ContactName = contactName,
            ContactPhone = contactPhone,
            Address = req.Address?.Trim(),
            Remark = req.Remark?.Trim(),
            PaymentMethod = req.PaymentMethod,
            CreateTime = now,
            PayTime = DateTime.MinValue
        };
        _db.Db.Insertable(order).ExecuteCommand();
        var orderId = order.Id;
        _db.Db.Insertable(new OrderItem
        {
            OrderId = orderId,
            ProductId = p.Id,
            ProductName = p.Name,
            ImagePath = p.ImagePath,
            Price = p.Price,
            Quantity = quantity,
            Amount = amount
        }).ExecuteCommand();
        return Ok(new { ok = true, orderId, orderNo });
    }

    [HttpGet("list")]
    public IActionResult MyOrders()
    {
        var userId = UserId;
        if (userId == null) return Ok(new { ok = false, msg = "请先登录" });
        var list = _db.Db.Queryable<Order>().Where(o => o.UserId == userId).OrderByDescending(o => o.CreateTime).ToList();
        return Ok(list);
    }

    [HttpGet("{id:int}")]
    public IActionResult OrderDetail(int id)
    {
        var userId = UserId;
        var order = _db.Db.Queryable<Order>().First(o => o.Id == id && o.UserId == userId);
        if (order == null) return NotFound();
        var items = _db.Db.Queryable<OrderItem>().Where(i => i.OrderId == id).ToList();
        return Ok(new { order, items });
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var userId = UserId;
        var order = _db.Db.Queryable<Order>().First(o => o.Id == id && o.UserId == userId);
        if (order == null) return Ok(new { ok = false, msg = "订单不存在" });
        _db.Db.Deleteable<OrderItem>().Where(i => i.OrderId == id).ExecuteCommand();
        _db.Db.Deleteable<Order>().Where(o => o.Id == id).ExecuteCommand();
        return Ok(new { ok = true });
    }
}

public class SubmitOrderRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public int PaymentMethod { get; set; }
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
    public string? Remark { get; set; }
}
