namespace WebGiayOnline.Models
{
    public class News
    {
        public int NewsId { get; set; }  // Khóa chính
        public string Title { get; set; }  // Tiêu đề tin tức
        public string Content { get; set; }  // Nội dung tin tức
        public DateTime PublishDate { get; set; }  // Ngày đăng tin
    }

}
