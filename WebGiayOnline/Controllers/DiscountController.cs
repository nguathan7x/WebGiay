using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebGiayOnline.Data;
using WebGiayOnline.Models;
using WebGiayOnline.ViewModels;
public class DiscountController : Controller
{
    private readonly ApplicationDbContext _context;

    public DiscountController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.Discounts.ToListAsync());
    }

    public IActionResult Create()
    {
        var model = new DiscountCreateViewModel
        {
            AllGiays = _context.Giays.ToList()
        };
        return View(model);
    }

    [HttpPost]
    public IActionResult Create(DiscountCreateViewModel model)
    {
        if (ModelState.IsValid)
        {
            var discount = new Discount
            {
                Name = model.Name,
                Percentage = model.Percentage,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                IsActive = model.IsActive
            };

            _context.Discounts.Add(discount);
            _context.SaveChanges();

            // Sau khi lưu discount, thêm liên kết DiscountGiay
            foreach (var giayId in model.SelectedGiayIds)
            {
                var dg = new DiscountGiay
                {
                    DiscountId = discount.DiscountId,
                    GiayId = giayId
                };
                _context.DiscountGiays.Add(dg);
            }

            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        // Nếu có lỗi thì load lại danh sách giày
        model.AllGiays = _context.Giays.ToList();
        return View(model);
    }


    public async Task<IActionResult> Edit(int id)
    {
        var discount = await _context.Discounts
            .Include(d => d.DiscountGiays)
            .FirstOrDefaultAsync(d => d.DiscountId == id);
        if (discount == null) return NotFound();

        var model = new DiscountCreateViewModel
        {
            Name = discount.Name,
            Percentage = discount.Percentage,
            StartDate = discount.StartDate,
            EndDate = discount.EndDate,
            IsActive = discount.IsActive,
            SelectedGiayIds = discount.DiscountGiays.Select(dg => dg.GiayId).ToList(),
            AllGiays = await _context.Giays.ToListAsync()
        };

        ViewBag.DiscountId = id;
        return View(model);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DiscountCreateViewModel model)
    {
        if (ModelState.IsValid)
        {
            var discount = await _context.Discounts
                .Include(d => d.DiscountGiays)
                .FirstOrDefaultAsync(d => d.DiscountId == id);
            if (discount == null) return NotFound();

            // Cập nhật thông tin Discount
            discount.Name = model.Name;
            discount.Percentage = model.Percentage;
            discount.StartDate = model.StartDate;
            discount.EndDate = model.EndDate;
            discount.IsActive = model.IsActive;

            // Xóa các liên kết cũ
            _context.DiscountGiays.RemoveRange(discount.DiscountGiays);

            // Tạo các liên kết mới
            foreach (var giayId in model.SelectedGiayIds)
            {
                _context.DiscountGiays.Add(new DiscountGiay
                {
                    DiscountId = discount.DiscountId,
                    GiayId = giayId
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        model.AllGiays = await _context.Giays.ToListAsync();
        ViewBag.DiscountId = id;
        return View(model);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var discount = await _context.Discounts
            .Include(d => d.DiscountGiays)
            .FirstOrDefaultAsync(d => d.DiscountId == id);

        if (discount == null) return NotFound();

        // Xóa liên kết trước
        _context.DiscountGiays.RemoveRange(discount.DiscountGiays);

        // Sau đó xóa Discount
        _context.Discounts.Remove(discount);

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

}
