namespace WebGiayOnline.Models
{
    public class GiaySize
    {
        public int GiayId { get; set; }
        public Giay Giay { get; set; }

        public int SizeId { get; set; }
        public Size Size { get; set; }

        public int Quantity { get; set; }             // Số lượng tồn kho cho size này
    }
}
