using Microsoft.AspNetCore.Identity;

namespace WebGiayOnline.Models
{
    public class ShoppingCart
    {
        public int ShoppingCartId { get; set; } // Primary Key

        public string UserId { get; set; }      // Foreign Key
        public ApplicationUser User { get; set; }  // Navigation Property

        public ICollection<ShoppingCartItem> Items { get; set; } = new List<ShoppingCartItem>();
    }
}
