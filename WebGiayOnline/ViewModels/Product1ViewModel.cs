using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace WebGiayOnline.ViewModels
{
    public class Product1ViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm không được để trống.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Giá sản phẩm không được để trống.")]
        public int Price { get; set; }

        public string? Description { get; set; }

        // Tên file ảnh lưu trên Cloudinary hoặc local
        public string? ImageUrl { get; set; }

        // File ảnh được upload từ form
        public IFormFile? ImageFile { get; set; }

        // Bạn có thể thêm các thuộc tính liên quan đến danh mục, trạng thái...
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
    }
}
