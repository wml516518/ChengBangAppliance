using System.Text.Json;
using ApplianceRepair.Models;

namespace ApplianceRepair.Services;

public class CartService
{
    private const string SessionKey = "Cart";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static List<CartItemDto> GetCart(ISession session)
    {
        var json = session.GetString(SessionKey);
        if (string.IsNullOrEmpty(json)) return new List<CartItemDto>();
        try
        {
            return JsonSerializer.Deserialize<List<CartItemDto>>(json, JsonOptions) ?? new List<CartItemDto>();
        }
        catch
        {
            return new List<CartItemDto>();
        }
    }

    public static void SaveCart(ISession session, List<CartItemDto> cart)
    {
        session.SetString(SessionKey, JsonSerializer.Serialize(cart, JsonOptions));
    }

    public static void ClearCart(ISession session)
    {
        session.Remove(SessionKey);
    }
}
