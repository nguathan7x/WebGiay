using WebGiayOnline.Models;

    public class DiscountCreateViewModel
    {
        // Thông tin giảm giá
        public string Name { get; set; }
        public decimal Percentage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }

        // Danh sách ID các giày được chọn
        public List<int> SelectedGiayIds { get; set; } = new List<int>();

        // Danh sách giày cho người dùng chọn (hiển thị trong View)
        public List<Giay> AllGiays { get; set; } = new List<Giay>();
    }
