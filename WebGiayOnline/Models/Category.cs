namespace WebGiayOnline.Models
{
    public class Category
    {
        public int CategoryId { get; set; }  // Khóa chính
        public string Name { get; set; }
        public string? Description { get; set; } // Mô tả danh mục
        public ICollection<Giay>? Giays { get; set; }  // Mối quan hệ với giày
    }

}
