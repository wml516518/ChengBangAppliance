using ApplianceRepair.Extensions;
using ApplianceRepair.Models;
using ApplianceRepair.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApplianceRepair.Controllers;

/// <summary>客户端下单：商品列表、直接下单、我的订单（与后台管理分开）</summary>
public class MobileController : Controller
{
    private readonly SqlSugarService _db;

    public MobileController(SqlSugarService db)
    {
        _db = db;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Index(string? category)
    {
        var query = _db.Db.Queryable<Product>().Where(p => p.IsOnSale);
        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.Category == category.Trim());
        var list = query.OrderByDescending(p => p.CreateTime).ToList();

        var categoryList = _db.Db.Queryable<Category>().OrderBy(c => c.SortOrder).ToList();
        var categories = categoryList.OrderBy(c => c.SortOrder).ThenBy(c => c.Name).Select(c => c.Name).ToList();

        ViewBag.Categories = categories;
        ViewBag.CurrentCategory = category?.Trim() ?? "";
        return View(list);
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Detail(int id)
    {
        var p = _db.Db.Queryable<Product>().First(x => x.Id == id);
        if (p == null || !p.IsOnSale) return NotFound();
        return View(p);
    }

    [Authorize]
    [HttpGet]
    public IActionResult Checkout(int productId, int quantity = 1)
    {
        if (productId <= 0) return RedirectToAction(nameof(Index));
        if (quantity < 1) quantity = 1;

        var p = _db.Db.Queryable<Product>().First(x => x.Id == productId);
        if (p == null || !p.IsOnSale) return RedirectToAction(nameof(Index));

        var userId = User.GetUserId()!.Value;
        var user = _db.Db.Queryable<User>().First(u => u.Id == userId);

        ViewBag.Product = p;
        ViewBag.Quantity = quantity;
        ViewBag.Total = p.Price * quantity;
        ViewBag.ContactName = user.RealName ?? user.UserName;
        ViewBag.ContactPhone = user.Phone ?? "";
        return View();
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SubmitOrder(int productId, int quantity, int paymentMethod, string contactName, string contactPhone, string? address, string? remark)
    {
        var userId = User.GetUserId();
        if (userId == null) return RedirectToAction("Login", "Account", new { returnUrl = "/Mobile" });

        var p = _db.Db.Queryable<Product>().First(x => x.Id == productId);
        if (p == null || !p.IsOnSale) { TempData["Error"] = "商品不存在或已下架"; return RedirectToAction(nameof(Index)); }
        if (quantity < 1) quantity = 1;
        if (paymentMethod != 1 && paymentMethod != 2) { TempData["Error"] = "请选择支付方式（微信或支付宝）"; return RedirectToAction(nameof(Checkout), new { productId, quantity }); }

        contactName = (contactName ?? "").Trim();
        contactPhone = (contactPhone ?? "").Trim();
        if (string.IsNullOrEmpty(contactName)) { TempData["Error"] = "请填写联系人"; return RedirectToAction(nameof(Checkout), new { productId, quantity }); }
        if (string.IsNullOrEmpty(contactPhone)) { TempData["Error"] = "请填写联系电话"; return RedirectToAction(nameof(Checkout), new { productId, quantity }); }

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
            Address = address?.Trim(),
            Remark = remark?.Trim(),
            PaymentMethod = paymentMethod,
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

        TempData["OrderId"] = orderId;
        TempData["OrderNo"] = orderNo;
        return RedirectToAction(nameof(OrderResult), new { id = orderId });
    }

    [Authorize]
    [HttpGet]
    public IActionResult OrderResult(int id)
    {
        var userId = User.GetUserId();
        var order = _db.Db.Queryable<Order>().First(o => o.Id == id && o.UserId == userId);
        if (order == null) return RedirectToAction(nameof(MyOrders));
        ViewBag.Order = order;
        return View();
    }

    [Authorize]
    [HttpGet]
    public IActionResult MyOrders()
    {
        var userId = User.GetUserId();
        if (userId == null) return RedirectToAction("Login", "Account");
        var list = _db.Db.Queryable<Order>().Where(o => o.UserId == userId).OrderByDescending(o => o.CreateTime).ToList();
        return View(list);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteOrder(int id)
    {
        var userId = User.GetUserId();
        if (userId == null) return RedirectToAction("Login", "Account");
        var order = _db.Db.Queryable<Order>().First(o => o.Id == id && o.UserId == userId);
        if (order == null) return RedirectToAction(nameof(MyOrders));
        _db.Db.Deleteable<OrderItem>().Where(i => i.OrderId == id).ExecuteCommand();
        _db.Db.Deleteable<Order>().Where(o => o.Id == id).ExecuteCommand();
        TempData["Success"] = "订单已删除";
        return RedirectToAction(nameof(MyOrders));
    }

    [Authorize]
    [HttpGet]
    public IActionResult OrderDetail(int id)
    {
        var userId = User.GetUserId();
        var order = _db.Db.Queryable<Order>().First(o => o.Id == id && o.UserId == userId);
        if (order == null) return NotFound();
        var items = _db.Db.Queryable<OrderItem>().Where(i => i.OrderId == id).ToList();
        ViewBag.Order = order;
        return View(items);
    }
}
