namespace WebGiayOnline.Models
{
    public class Size
    {
        public int SizeId { get; set; }               // Khóa chính
        public string Label { get; set; }             // Ví dụ: "EU 38", "US 7"

        public ICollection<GiaySize> GiaySizes { get; set; } = new List<GiaySize>();
    }
}
