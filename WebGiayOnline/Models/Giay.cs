using System.ComponentModel.DataAnnotations.Schema;
using WebGiayOnline.Models;

namespace WebGiayOnline.Models
{
    public class Giay
    {
        public int GiayId { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public string Color { get; set; }
        public string? ImageUrl { get; set; }

        public int? BrandId { get; set; }
        public Brand? Brand { get; set; }
        public string? Gender { get; set; } // "Men", "Women", "Unisex"

        public int? CategoryId { get; set; }
        public Category? Category { get; set; }
        [NotMapped]
        public IFormFile? ImageFile { get; set; }
        public string? Description { get; set; }
        public string? Content { get; set; }
        public int? ProductGroupId { get; set; }    // Khóa ngoại liên kết mẫu giày chung
        public ProductGroup? ProductGroup { get; set; }
        // Quan hệ nhiều-nhiều với Size qua GiaySize
        public ICollection<GiaySize> GiaySizes { get; set; } = new List<GiaySize>();
         
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
        public ICollection<DiscountGiay> DiscountGiays { get; set; } = new List<DiscountGiay>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }


}


