namespace WebGiayOnline.Models
{
    public class Favorite
    {
        public int FavoriteId { get; set; }
        public string UserId { get; set; }  // liên kết với Identity User
        public int GiayId { get; set; }     // sản phẩm được yêu thích

        public Giay Giay { get; set; }
    }

}
