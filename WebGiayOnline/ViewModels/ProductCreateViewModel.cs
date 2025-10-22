using WebGiayOnline.Models;

namespace WebGiayOnline.ViewModels
{
    public class ProductCreateViewModel
    {
        public Giay Giay { get; set; } = new Giay();
        public List<IFormFile>? ImageFiles { get; set; }
    }
}
