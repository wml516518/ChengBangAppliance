using SqlSugar;

namespace ApplianceRepair.Models;

[SugarTable("Orders")]
public class Order
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    public int UserId { get; set; }

    /// <summary>订单号，便于展示与查询</summary>
    [SugarColumn(Length = 32)]
    public string OrderNo { get; set; } = string.Empty;

    /// <summary>0待支付 1已支付 2已完成 3已取消</summary>
    public int Status { get; set; }

    public decimal TotalAmount { get; set; }

    [SugarColumn(Length = 50)]
    public string ContactName { get; set; } = string.Empty;

    [SugarColumn(Length = 20)]
    public string ContactPhone { get; set; } = string.Empty;

    [SugarColumn(Length = 200, IsNullable = true)]
    public string? Address { get; set; }

    [SugarColumn(Length = 500, IsNullable = true)]
    public string? Remark { get; set; }

    /// <summary>支付方式：0未选 1微信 2支付宝</summary>
    public int PaymentMethod { get; set; }

    public DateTime CreateTime { get; set; } = DateTime.Now;
    public DateTime? PayTime { get; set; }
}
