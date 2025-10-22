using Microsoft.AspNetCore.Mvc;
using WebGiayOnline.Models;
using Microsoft.EntityFrameworkCore;
using WebGiayOnline.Data;

namespace WebGiayOnline.ViewComponents
{
    public class GiayCardViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public GiayCardViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(Giay giay)
        {
            // Nạp Discount đang có hiệu lực
            var discount = await _context.DiscountGiays
                .Where(dg => dg.GiayId == giay.GiayId)
                .Select(dg => dg.Discount)
                .FirstOrDefaultAsync(d =>
                    d.IsActive &&
                    d.StartDate <= DateTime.Now &&
                    d.EndDate >= DateTime.Now);

            ViewData["Discount"] = discount;

            return View(giay); // truyền đối tượng giày vào view
        }
    }
}
