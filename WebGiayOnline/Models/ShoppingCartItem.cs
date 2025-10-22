using WebGiayOnline.Models;
namespace WebGiayOnline.Models;
public class ShoppingCartItem
{
    public int ShoppingCartItemId { get; set; }
    public int ShoppingCartId { get; set; }
    public int GiayId { get; set; }
    public int SizeId { get; set; } // ← dùng SizeId thay vì Size
    public int Quantity { get; set; }
    public int Price { get; set; }

    public Giay Giay { get; set; }
    public Size Size { get; set; }
    public ShoppingCart ShoppingCart { get; set; }
}
