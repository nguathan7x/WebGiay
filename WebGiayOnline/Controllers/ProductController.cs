using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using WebGiayOnline.Data;
using WebGiayOnline.Models;
using Microsoft.AspNetCore.Identity;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace WebGiayOnline.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;


        public ProductController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,IWebHostEnvironment hostingEnvironment, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }
 
   
        public async Task<IActionResult> Index(string searchQuery, string gender)
        {
            var query = _context.Giays.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                query = query.Where(p => p.Name.Contains(searchQuery));
            }

            if (!string.IsNullOrWhiteSpace(gender))
            {
                gender = gender.Trim().ToLower();
                query = query.Where(p => p.Gender.ToLower() == gender);
            }

            // Gửi danh sách giới tính về View để vẽ dropdown lọc
            ViewBag.AvailableGenders = new List<string> { "Men", "Women", "Unisex" };
            ViewBag.CurrentGender = gender;
            ViewBag.SearchQuery = searchQuery;

            return View(await query.ToListAsync());
        }

      
        public async Task<IActionResult> DisplayGroup(int productGroupId)
        {
            var productGroup = await _context.ProductGroups
                .Include(pg => pg.Variants)
                    .ThenInclude(g => g.ProductImages)
                .FirstOrDefaultAsync(pg => pg.ProductGroupId == productGroupId);

            if (productGroup == null)
                return NotFound();

            return View(productGroup);
        }

        public async Task<IActionResult> Details(int id)
        {
            // Lấy thông tin chi tiết sản phẩm từ DB
            var product = await _context.Giays
                .Include(g => g.Reviews)
                .ThenInclude(r => r.User)
                .Include(g => g.Brand)
                .Include(g => g.Category)
                .Include(g => g.ProductGroup)
                    .ThenInclude(pg => pg.Variants)
                        .ThenInclude(v => v.ProductImages)
                .Include(g => g.ProductImages)
                .Include(g => g.DiscountGiays)
                    .ThenInclude(dg => dg.Discount)
                .FirstOrDefaultAsync(m => m.GiayId == id);

            if (product == null)
            {
                return NotFound();
            }


            string baseUrl = "http://localhost:5555/";
            List<SanPhamGoiy> sanPhamGoiy = new List<SanPhamGoiy>();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync($"api?id={id}");
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    var goiyResponse = JsonConvert.DeserializeObject<SanPhamGoiyResponse>(json);
                    sanPhamGoiy = goiyResponse.san_pham_gui_y;
                }
            }

            ViewBag.SanPhamGoiy = sanPhamGoiy;

            //// Gọi API AI gợi ý sản phẩm
            //List<string> sanPhamGoiy = new List<string>();
            //string baseUrl = "http://localhost:5555/";
            //using (var client = new HttpClient())
            //{
            //    client.BaseAddress = new Uri(baseUrl);
            //    client.DefaultRequestHeaders.Accept.Clear();
            //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //    HttpResponseMessage response = await client.GetAsync($"api?id={id}");
            //    if (response.IsSuccessStatusCode)
            //    {
            //        string json = await response.Content.ReadAsStringAsync();
            //        var result = JsonConvert.DeserializeObject<SanPhamGoiyResponse>(json);
            //        sanPhamGoiy = result.san_pham_gui_y; // tên key json trả về từ Flask là "san pham gui y"
            //    }
            //}

            //ViewBag.SanPhamGoiy = sanPhamGoiy;

            // Tính giá giảm
            var discount = _context.DiscountGiays
                .Where(dg =>
                            dg.GiayId == id &&
                            dg.Discount != null &&
                            dg.Discount.IsActive &&
                            dg.Discount.StartDate <= DateTime.Now &&
                            dg.Discount.EndDate >= DateTime.Now)
                .OrderByDescending(dg => dg.Discount.Percentage)
                .FirstOrDefault();

            decimal discountPercent = discount?.Discount.Percentage ?? 0;
            decimal finalPrice = product.Price * (1 - discountPercent / 100);
            ViewBag.DiscountPercent = discountPercent;
            ViewBag.FinalPrice = finalPrice;

            // Lấy size + tồn kho
            var allSizes = await _context.Sizes.ToListAsync();
            var giaySizes = await _context.GiaySizes.Where(gs => gs.GiayId == id).ToListAsync();

            var shoeSizes = allSizes.Select(s => new ShoeSize
            {
                SizeLabel = s.Label,
                IsAvailable = giaySizes.Any(gs => gs.SizeId == s.SizeId && gs.Quantity > 0)
            }).ToList();

            ViewBag.Sizes = shoeSizes;

            // Kiểm tra favorite
            var userId = _userManager.GetUserId(User);
            if (userId != null)
            {
                var isFavorite = await _context.Favorites.AnyAsync(f => f.GiayId == id && f.UserId == userId);
                ViewBag.IsFavorite = isFavorite;
            }
            else
            {
                ViewBag.IsFavorite = false;
            }

            if (product.Reviews != null && product.Reviews.Any())
            {
                ViewBag.TotalReviews = product.Reviews.Count;
                ViewBag.AvgRating = product.Reviews.Average(r => r.Rating);
            }
            else
            {
                ViewBag.TotalReviews = 0;
                ViewBag.AvgRating = 0;
            }

            var ratingSummary = product.Reviews
    .GroupBy(r => r.Rating)
    .Select(g => new
    {
        Rating = g.Key,
        Count = g.Count()
    })
    .ToDictionary(g => g.Rating, g => g.Count);

            ViewBag.RatingSummary = ratingSummary;

            // Tìm OrderDetailId tương ứng nếu người dùng đã mua và đơn hoàn thành
            if (userId != null)
            {
                var orderDetail = await _context.OrderDetails
                    .Include(od => od.Order)
                    .Where(od => od.GiayId == id
                              && od.Order.UserId == userId
                              && od.Order.Status == "Hoàn thành")
                    .OrderByDescending(od => od.Order.CreatedAt)
                    .FirstOrDefaultAsync();

                ViewBag.OrderDetailId = orderDetail?.OrderDetailId;
            }
            else
            {
                ViewBag.OrderDetailId = null;
            }

            return View(product);
        }

        // public async Task<IActionResult> Details(int id)
        // {
        //     var product = await _context.Giays             
        //          .Include(g => g.Brand)
        //             .Include(g => g.Category)
        //             .Include(g => g.ProductGroup)
        //     .ThenInclude(pg => pg.Variants)              // Load tất cả biến thể
        //         .ThenInclude(v => v.ProductImages)       // Load ảnh từng biến thể
        //          .Include(g => g.ProductImages)
        //           .Include(g => g.DiscountGiays)
        //     .ThenInclude(dg => dg.Discount)
        //          .FirstOrDefaultAsync(m => m.GiayId == id);
        //     if (product == null)
        //     {
        //         return NotFound();
        //     }

        //     // Lấy dữ liệu size + tồn kho
        //     var allSizes = await _context.Sizes.ToListAsync();
        //     var giaySizes = await _context.GiaySizes.Where(gs => gs.GiayId == id).ToListAsync();


        //     var discount = _context.DiscountGiays
        //.Where(dg => dg.Discount.IsActive &&
        //             dg.Discount.StartDate <= DateTime.Now &&
        //             dg.Discount.EndDate >= DateTime.Now)
        //.OrderByDescending(dg => dg.Discount.Percentage)
        //.FirstOrDefault();

        //     // Tính giá sau giảm
        //     decimal discountPercent = discount?.Discount.Percentage ?? 0;
        //     decimal finalPrice = product.Price * (1 - discountPercent / 100);

        //     // Gửi qua ViewBag hoặc ViewModel
        //     ViewBag.DiscountPercent = discountPercent;
        //     ViewBag.FinalPrice = finalPrice;


        //     var shoeSizes = allSizes.Select(s => new ShoeSize
        //     {
        //         SizeLabel = s.Label,
        //         IsAvailable = giaySizes.Any(gs => gs.SizeId == s.SizeId && gs.Quantity > 0)
        //     }).ToList();

        //     ViewBag.Sizes = shoeSizes;

        //     // Lấy UserId hiện tại
        //     var userId = _userManager.GetUserId(User);
        //     if (userId != null)
        //     {
        //         // Kiểm tra xem sản phẩm có trong danh sách yêu thích của user hay không
        //         var isFavorite = await _context.Favorites.AnyAsync(f => f.GiayId == id && f.UserId == userId);
        //         ViewBag.IsFavorite = isFavorite;
        //     }
        //     else
        //     {
        //         ViewBag.IsFavorite = false; // Nếu chưa đăng nhập thì không phải favorite
        //     }

        //     return View(product);
        // }

        public async Task<IActionResult> Create()
        {
            ViewBag.AllSizes = _context.Sizes
                .AsEnumerable()
                .OrderBy(s => ConvertLabelToNumber(s.Label))
                .ToList();

            ViewBag.ProductGroups = new SelectList(await _context.ProductGroups.ToListAsync(), "ProductGroupId", "GroupName");

            //ViewData["Brands"] = new SelectList(await _context.Brands.ToListAsync(), "BrandId", "Name");
            ViewBag.Brands = new SelectList(await _context.Brands.ToListAsync(), "BrandId", "Name");
            //ViewData["Categories"] = new SelectList(await _context.Categories.ToListAsync(), "CategoryId", "Name");
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "CategoryId", "Name");
            return View();
        }

        // Xử lý form tạo sản phẩm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("GiayId,Gender,Name,Price,Content,Description,Color,BrandId,CategoryId,ProductGroupId")] Giay giay, List<IFormFile> ImageFiles)
        {
            if (ModelState.IsValid)
            {
                var brandId = giay.BrandId;
                var categoryId = giay.CategoryId;
                if (ImageFiles != null && ImageFiles.Count > 0)
                {
                    var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    giay.ProductImages = new List<ProductImage>();

                    foreach (var file in ImageFiles)
                    {
                        if (file.Length > 0)
                        {
                            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                            var filePath = Path.Combine(uploadsFolder, fileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            var productImage = new ProductImage
                            {
                                ImageUrl = "/uploads/" + fileName
                            };
                            giay.ProductImages.Add(productImage);
                        }
                    }

                    // Lấy ảnh đầu tiên làm ảnh đại diện (ImageUrl)
                    giay.ImageUrl = giay.ProductImages.First().ImageUrl;
                }

                _context.Add(giay);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.AllSizes = _context.Sizes
                .AsEnumerable()
                .OrderBy(s => ConvertLabelToNumber(s.Label))
                .ToList();
            ViewBag.ProductGroups = new SelectList(await _context.ProductGroups.ToListAsync(), "ProductGroupId", "GroupName", giay.ProductGroupId);
            ViewBag.Brands = new SelectList(await _context.Brands.ToListAsync(), "BrandId", "Name", giay.BrandId);
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "CategoryId", "Name", giay.CategoryId);


            return View(giay);
        }

        private double ConvertLabelToNumber(string label)
        {
            // Loại bỏ tiền tố như "EU ", "US ", v.v.
            var numberPart = new string(label.Where(c => char.IsDigit(c) || c == '.').ToArray());

            if (double.TryParse(numberPart, out var number))
            {
                return number;
            }

            return double.MaxValue; // Đưa các label không rõ định dạng xuống cuối danh sách
        }



        // Chỉnh sửa giày (Edit)
        public async Task<IActionResult> Edit(int id)
        {
            var giay = await _context.Giays.FindAsync(id);
            if (giay == null)
            {
                return NotFound();
            }
            ViewBag.AllSizes = _context.Sizes
           .AsEnumerable()
          .OrderBy(s => ConvertLabelToNumber(s.Label))
          .ToList();
            ViewBag.ProductGroups = new SelectList(await _context.ProductGroups.ToListAsync(), "ProductGroupId", "GroupName");
            // Truyền dữ liệu Brands và Categories sang View để dropdown hiển thị đúng
            ViewBag.Brands = new SelectList(_context.Brands, "BrandId", "Name", giay.BrandId);
            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name", giay.CategoryId);


            return View(giay);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("GiayId,Gender,Name,Price,Content,Description,Color,ImageUrl,BrandId,ProductGroupId,CategoryId")] Giay giay, List<IFormFile> ImageFiles)
        {
            if (id != giay.GiayId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingGiay = await _context.Giays
                        .Include(g => g.ProductImages)
                        .FirstOrDefaultAsync(g => g.GiayId == id);

                    if (existingGiay == null)
                    {
                        return NotFound();
                    }

                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    if (ImageFiles != null && ImageFiles.Count > 0) // Có ảnh mới
                    {
                        // Xóa ảnh cũ trên ổ cứng
                        if (existingGiay.ProductImages != null && existingGiay.ProductImages.Any())
                        {
                            foreach (var oldImage in existingGiay.ProductImages)
                            {
                                var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, oldImage.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                                if (System.IO.File.Exists(oldFilePath))
                                {
                                    System.IO.File.Delete(oldFilePath);
                                }
                            }

                            // Xóa ảnh cũ trong database
                            _context.ProductImages.RemoveRange(existingGiay.ProductImages);
                            existingGiay.ProductImages.Clear();
                        }

                        existingGiay.ProductImages = new List<ProductImage>();

                        foreach (var file in ImageFiles)
                        {
                            if (file.Length > 0)
                            {
                                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                                var filePath = Path.Combine(uploadsFolder, fileName);

                                using (var stream = new FileStream(filePath, FileMode.Create))
                                {
                                    await file.CopyToAsync(stream);
                                }

                                var productImage = new ProductImage
                                {
                                    ImageUrl = "/uploads/" + fileName,
                                    GiayId = giay.GiayId
                                };

                                existingGiay.ProductImages.Add(productImage);
                            }
                        }

                        // Cập nhật ảnh đại diện mới (ảnh đầu tiên)
                        existingGiay.ImageUrl = existingGiay.ProductImages.First().ImageUrl;
                    }
                    else
                    {
                        // Không có ảnh mới: giữ nguyên ảnh cũ, không làm gì cả
                        // Nếu muốn xóa ảnh đại diện khi không upload ảnh mới, bạn có thể bổ sung logic ở đây
                        // Ví dụ:
                        // existingGiay.ImageUrl = existingGiay.ImageUrl; // giữ nguyên
                    }

                    // Cập nhật các thông tin khác
                    existingGiay.Name = giay.Name;
                    existingGiay.Gender = giay.Gender;
                    existingGiay.Price = giay.Price;
                    existingGiay.Color = giay.Color;
                    existingGiay.Content = giay.Content;
                    existingGiay.Description = giay.Description;
                    existingGiay.BrandId = giay.BrandId;
                    existingGiay.ProductGroupId = giay.ProductGroupId;
                    existingGiay.CategoryId = giay.CategoryId;

                    _context.Update(existingGiay);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GiayExists(giay.GiayId))
                        return NotFound();
                    else
                        throw;
                }
            }

            // Load lại ViewBag nếu ModelState không hợp lệ
            ViewBag.AllSizes = _context.Sizes
                .AsEnumerable()
                .OrderBy(s => ConvertLabelToNumber(s.Label))
                .ToList();
            ViewBag.ProductGroups = new SelectList(await _context.ProductGroups.ToListAsync(), "ProductGroupId", "GroupName", giay.ProductGroupId);
            ViewBag.Brands = new SelectList(_context.Brands, "BrandId", "Name", giay.BrandId);
            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name", giay.CategoryId);

            return View(giay);
        }



        // Xóa giày (Delete)
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Giays
                .FirstOrDefaultAsync(m => m.GiayId == id);
            if (product == null)
            {
                return NotFound();
            }
         
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Giays
                .Include(g => g.ProductImages)
                .FirstOrDefaultAsync(g => g.GiayId == id);

            if (product != null)
            {
                // Xóa ảnh trên ổ cứng
                if (product.ProductImages != null && product.ProductImages.Any())
                {
                    foreach (var img in product.ProductImages)
                    {
                        var filePath = Path.Combine(_webHostEnvironment.WebRootPath, img.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }

                    // Xóa ảnh trong database
                    _context.ProductImages.RemoveRange(product.ProductImages);
                }

                // Xóa sản phẩm
                _context.Giays.Remove(product);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }


        private bool GiayExists(int id)
        {
            return _context.Giays.Any(e => e.GiayId == id);
        }
    }
}
