namespace WebGiayOnline.Models
{
    public class DiscountGiay
    {
        public int DiscountId { get; set; }
        public Discount Discount { get; set; }

        public int GiayId { get; set; }
        public Giay Giay { get; set; }
    }

}
