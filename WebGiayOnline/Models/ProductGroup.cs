using System.ComponentModel.DataAnnotations;

namespace WebGiayOnline.Models
{
    public class ProductGroup
    {
        public int ProductGroupId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên nhóm sản phẩm")]
        public string GroupName { get; set; }    // Tên mẫu giày, ví dụ "Nike Air Max Dn8"

        public ICollection<Giay> Variants { get; set; } = new List<Giay>();
    }

}
