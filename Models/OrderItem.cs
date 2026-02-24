using SqlSugar;

namespace ApplianceRepair.Models;

[SugarTable("OrderItems")]
public class OrderItem
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int ProductId { get; set; }

    [SugarColumn(Length = 100)]
    public string ProductName { get; set; } = string.Empty;

    [SugarColumn(Length = 200, IsNullable = true)]
    public string? ImagePath { get; set; }

    public decimal Price { get; set; }

    public int Quantity { get; set; }

    public decimal Amount { get; set; }
}
