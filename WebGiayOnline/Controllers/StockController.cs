using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebGiayOnline.Data;
using WebGiayOnline.Models;
using Microsoft.EntityFrameworkCore;

namespace WebGiayOnline.Controllers
{
    public class StockController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StockController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Form nhập kho
        public IActionResult Index()
        {
            ViewBag.Products = new SelectList(_context.Giays, "GiayId", "Name");
            ViewBag.Sizes = _context.Sizes.OrderBy(s => s.Label).ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(int productId, int sizeId, int quantity)
        {
            var giaySize = await _context.GiaySizes
                .FirstOrDefaultAsync(gs => gs.GiayId == productId && gs.SizeId == sizeId);

            if (giaySize == null)
            {
                // Thêm mới nếu chưa có
                giaySize = new GiaySize
                {
                    GiayId = productId,
                    SizeId = sizeId,
                    Quantity = quantity
                };
                _context.GiaySizes.Add(giaySize);
            }
            else
            {
                // Cập nhật số lượng mới
                giaySize.Quantity = quantity;
                _context.GiaySizes.Update(giaySize);
            }

            await _context.SaveChangesAsync();

            ViewBag.Products = new SelectList(_context.Giays, "GiayId", "Name");
            ViewBag.Sizes = _context.Sizes.OrderBy(s => s.Label).ToList();
            ViewBag.Message = "Updated stock successfully!";

            return View();
        }
        public IActionResult ChooseProduct()
        {
            var products = _context.Giays.OrderBy(p => p.Name).ToList();
            return View(products);
        }

        public async Task<IActionResult> EditBulk(int giayId)
        {
            var giay = await _context.Giays.FindAsync(giayId);
            if (giay == null) return NotFound();

            ViewBag.AllSizes = await _context.Sizes.OrderBy(s => s.Label).ToListAsync();

            // Lấy tồn kho hiện tại cho sản phẩm
            var stocks = await _context.GiaySizes
                .Where(gs => gs.GiayId == giayId)
                .ToListAsync();

            // Đưa tồn kho về Dictionary<SizeId, Quantity>
            ViewBag.CurrentStocks = stocks.ToDictionary(s => s.SizeId, s => s.Quantity);

            return View(giay);
        }

        // POST: Xử lý cập nhật tồn kho hàng loạt
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBulk(int GiayId, Dictionary<int, int> Quantities)
        {
            var giay = await _context.Giays.FindAsync(GiayId);
            if (giay == null) return NotFound();

            foreach (var entry in Quantities)
            {
                int sizeId = entry.Key;
                int qty = entry.Value;

                var giaySize = await _context.GiaySizes
                    .FirstOrDefaultAsync(gs => gs.GiayId == GiayId && gs.SizeId == sizeId);

                if (giaySize == null)
                {
                    // Thêm mới
                    giaySize = new GiaySize
                    {
                        GiayId = GiayId,
                        SizeId = sizeId,
                        Quantity = qty
                    };
                    _context.GiaySizes.Add(giaySize);
                }
                else
                {
                    // Cập nhật số lượng
                    giaySize.Quantity = qty;
                    _context.GiaySizes.Update(giaySize);
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Stock quantities updated successfully.";
            return RedirectToAction(nameof(EditBulk), new { giayId = GiayId });
        }
    }
}


