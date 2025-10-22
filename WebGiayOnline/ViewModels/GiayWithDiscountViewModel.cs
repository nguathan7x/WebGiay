using WebGiayOnline.Models;
namespace WebGiayOnline.ViewModels
{
    public class GiayWithDiscountViewModel
    {
        public Giay Giay { get; set; }
        public Discount? ActiveDiscount { get; set; }
        public double AvgRating { get; set; } // Trung bình đánh giá
        public int TotalReviews { get; set; } // Số lượng đánh giá

    }
}