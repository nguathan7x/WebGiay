using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebGiayOnline.Data;
using WebGiayOnline.Models;

namespace WebGiayOnline.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            int totalOrders = _context.Orders.Count();


            // Chỉ lấy đơn hàng đã hoàn thành trong ngày
            var deliveredOrders = _context.Orders
                .Where(o => o.CreatedAt >= today && o.CreatedAt < tomorrow && o.Status == "Hoàn thành")
                .Select(o => o.OrderId)
                .ToList();

            var totalQuantitySoldToday = _context.OrderDetails
                .Where(od => deliveredOrders.Contains(od.OrderId))
                .Sum(od => (int?)od.Quantity) ?? 0;

            var totalRevenueToday = _context.OrderDetails
                .Where(od => deliveredOrders.Contains(od.OrderId))
                .Sum(od => (decimal?)od.Price * od.Quantity) ?? 0;

            // Thống kê số đơn hàng theo trạng thái
            var pendingCount = _context.Orders.Count(o => o.Status == "Chờ xác nhận");
            var confirmedCount = _context.Orders.Count(o => o.Status == "Đã xác nhận");
            var completedCount = _context.Orders.Count(o => o.Status == "Hoàn thành");

            ViewBag.OrderStatusData = new
            {
                Pending = pendingCount,
                Confirmed = confirmedCount,
                Completed = completedCount
            };

            // Doanh thu theo ngày (7 ngày gần nhất)
            var revenueByDay = _context.Orders
        .Where(o => o.Status == "Hoàn thành")
        .ToList() // chạy SQL trước, sau đó xử lý trong bộ nhớ
        .GroupBy(o => o.CreatedAt.Date)
        .OrderByDescending(g => g.Key)
        .Take(7)
        .Select(g => new
        {
            Date = g.Key.ToString("dd/MM"),
            Total = g.Sum(o => o.Total)
        })
        .OrderBy(g => g.Date) // sắp lại theo ngày tăng dần để vẽ biểu đồ
        .ToList();


            ViewBag.RevenueByDayLabels = revenueByDay.Select(r => r.Date).ToArray();
            ViewBag.RevenueByDayValues = revenueByDay.Select(r => r.Total).ToArray();

            // Top 5 sản phẩm bán chạy hôm nay
            var topProducts = _context.OrderDetails
                .Where(od => deliveredOrders.Contains(od.OrderId))
                .Include(od => od.Giay)
                .GroupBy(od => od.Giay.Name)
                .Select(g => new
                {
                    ProductName = g.Key,
                    TotalSold = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(g => g.TotalSold)
                .Take(5)
                .ToList();

            // Truyền dữ liệu ra View
            ViewBag.AllOrders = totalOrders;
            ViewBag.TotalSoldToday = totalQuantitySoldToday;
            ViewBag.TotalRevenueToday = totalRevenueToday;
            ViewBag.PendingCount = pendingCount;
            ViewBag.ConfirmedCount = confirmedCount;
            ViewBag.CompletedCount = completedCount;
            ViewBag.TopProducts = topProducts;


            return View();
        }
    }

}
