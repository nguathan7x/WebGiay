using Newtonsoft.Json;

namespace WebGiayOnline.Models
{
    public class SanPhamGoiyResponse
    {
        [JsonProperty("san pham gui y")]
        public List<SanPhamGoiy> san_pham_gui_y { get; set; }
    }

    public class SanPhamGoiy
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }
    }
}