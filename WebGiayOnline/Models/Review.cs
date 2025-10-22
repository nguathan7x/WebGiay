namespace WebGiayOnline.Models
{
    public class Review
    {
        public int ReviewId { get; set; }  // Khóa chính
        public int GiayId { get; set; }  // Khóa ngoại liên kết với Giay
        public string UserId { get; set; }  // Khóa ngoại liên kết với User (Identity)
        public string Content { get; set; }  // Nội dung đánh giá
        public int Rating { get; set; }  // Đánh giá (1-5 sao)
        public DateTime ReviewDate { get; set; }  // Ngày đánh giá
        public Giay Giay { get; set; }  // Liên kết với Giay
        public ApplicationUser User { get; set; }
        public int OrderDetailId { get; set; }  // chỉ là khóa ngoại
        public OrderDetail OrderDetail { get; set; }  // navigation property

    }

}
