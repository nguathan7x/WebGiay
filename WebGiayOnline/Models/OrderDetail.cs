using WebGiayOnline.Models;

namespace WebGiayOnline.Models
{
    public class OrderDetail
    {
        public int OrderDetailId { get; set; }
        public int OrderId { get; set; }
        public int GiayId { get; set; }
        public int SizeId { get; set; } // THÊM SizeId để lưu đúng size đã chọn
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        public Order Order { get; set; }
        public Giay Giay { get; set; }
        public Size Size { get; set; } // Navigation property (nếu bạn có class Size)
        public ICollection<Review> Reviews { get; set; }
    }


}

