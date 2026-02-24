namespace ApplianceRepair.Models;

/// <summary>购物车项（存 Session）</summary>
public class CartItemDto
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImagePath { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal Amount => Price * Quantity;
}
