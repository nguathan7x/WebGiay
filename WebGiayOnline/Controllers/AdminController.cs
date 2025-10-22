using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebGiayOnline.Data;
using WebGiayOnline.Models;

namespace WebGiayOnline.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OrderList()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails!)
                    .ThenInclude(d => d.Giay)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int orderId, string newStatus)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                return NotFound();

            if (order.Status == newStatus)
                return RedirectToAction("OrderList");

            // Ghi lại lịch sử
            var history = new OrderStatusHistory
            {
                OrderId = orderId,
                OldStatus = order.Status,
                NewStatus = newStatus,
                ChangedBy = User.Identity.Name ?? "Admin" // hoặc lấy userId/email
            };

            order.Status = newStatus;

            _context.OrderStatusHistories.Add(history);
            await _context.SaveChangesAsync();

            return RedirectToAction("OrderList");
        }

        [HttpGet("OrderDetails/{orderId}")]
        public async Task<IActionResult> OrderDetails(int orderId)

        {
            var order = await _context.Orders
                  .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Giay)
                .Include(o => o.StatusHistories) // ✅ Load lịch sử trạng thái
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
                return NotFound();

            return View(order);
        }

        //[HttpPost]
        //public async Task<IActionResult> UpdateStatus(int orderId, string newStatus)
        //{
        //    var order = await _context.Orders.FindAsync(orderId);
        //    if (order == null)
        //        return NotFound();

        //    order.Status = newStatus;
        //    await _context.SaveChangesAsync();

        //    return RedirectToAction("OrderList");
        //}
    }
}
