namespace WebGiayOnline.Models
{
    public class Brand
    {
        public int BrandId { get; set; }  // Khóa chính
        public string Name { get; set; }
        public string? Description { get; set; } // Mô tả thương hiệu
        public ICollection<Giay>? Giays { get; set; } // Mối quan hệ với giày
    }

}
