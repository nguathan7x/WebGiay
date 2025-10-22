namespace WebGiayOnline.Models
{
    public class ProductImage
    {
        public int ProductImageId { get; set; }
        public int GiayId { get; set; }         // FK đến Giay
        public string ImageUrl { get; set; } = "";

        public Giay? Giay { get; set; }
    }

}
