using SqlSugar;

namespace ApplianceRepair.Models;

[SugarTable("Products")]
public class Product
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    [SugarColumn(Length = 100)]
    public string Name { get; set; } = string.Empty;

    [SugarColumn(Length = 500, IsNullable = true)]
    public string? Description { get; set; }

    /// <summary>true=上架，false=下架</summary>
    public bool IsOnSale { get; set; } = true;

    public decimal Price { get; set; }

    [SugarColumn(Length = 50, IsNullable = true)]
    public string? Category { get; set; }

    /// <summary>商品图片相对路径，如 /uploads/xxx.jpg</summary>
    [SugarColumn(Length = 200, IsNullable = true)]
    public string? ImagePath { get; set; }

    public DateTime CreateTime { get; set; } = DateTime.Now;
    public DateTime? UpdateTime { get; set; }
}
