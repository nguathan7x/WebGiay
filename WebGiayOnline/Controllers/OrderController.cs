using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebGiayOnline.Data;

namespace WebGiayOnline.Controllers;

public class OrderController : Controller
{
    private readonly ApplicationDbContext _context;

    public OrderController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> MyOrders(string? status, string? searchQuery)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return RedirectToAction("Login", "Account");

        var statusList = new List<string> { "Tất cả", "Chờ xác nhận", "Đã xác nhận", "Hoàn thành" };

        ViewBag.AvailableStatuses = statusList;
        ViewBag.CurrentStatus = status ?? "Tất cả";
        ViewBag.SearchQuery = searchQuery;

        var ordersQuery = _context.Orders
            .Include(o => o.OrderDetails!).ThenInclude(d => d.Giay)
            .Include(o => o.OrderDetails!).ThenInclude(d => d.Size)
            .Where(o => o.UserId == userId);

        // Lọc theo trạng thái nếu có
        if (!string.IsNullOrEmpty(status) && status != "Tất cả")
        {
            ordersQuery = ordersQuery.Where(o => o.Status == status);
        }

        // Lọc theo mã đơn hàng (OrderId, chuyển thành chuỗi để so sánh)
        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            ordersQuery = ordersQuery.Where(o => o.OrderId.ToString().Contains(searchQuery));
        }

        var orders = await ordersQuery
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return View(orders);
    }

    //public async Task<IActionResult> MyOrders(string? status)
    //{
    //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    //    if (userId == null)
    //        return RedirectToAction("Login", "Account");

    //    var statusList = new List<string> { "Tất cả", "Chờ xác nhận", "Đã xác nhận", "Hoàn thành" };

    //    ViewBag.AvailableStatuses = statusList;
    //    ViewBag.CurrentStatus = status ?? "Tất cả";

    //    var ordersQuery = _context.Orders
    //        .Include(o => o.OrderDetails!).ThenInclude(d => d.Giay)
    //        .Include(o => o.OrderDetails!).ThenInclude(d => d.Size)
    //        .Where(o => o.UserId == userId);

    //    if (!string.IsNullOrEmpty(status) && status != "Tất cả")
    //    {
    //        ordersQuery = ordersQuery.Where(o => o.Status == status);
    //    }

    //    var orders = await ordersQuery
    //        .OrderByDescending(o => o.CreatedAt)
    //        .ToListAsync();

    //    return View(orders);
    //}

    //public async Task<IActionResult> MyOrders(string? status)
    //{
    //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    //    if (userId == null)
    //        return RedirectToAction("Login", "Account");

    //    // Danh sách trạng thái cho dropdown
    //    var statusList = new List<string> { "Tất cả", "Chờ xác nhận", "Đã xác nhận", "Hoàn thành" };
    //    ViewBag.StatusList = new SelectList(statusList, status ?? "Tất cả");

    //    var ordersQuery = _context.Orders
    //        .Include(o => o.OrderDetails!).ThenInclude(d => d.Giay)
    //        .Include(o => o.OrderDetails!).ThenInclude(d => d.Size)
    //        .Where(o => o.UserId == userId);

    //    if (!string.IsNullOrEmpty(status) && status != "Tất cả")
    //    {
    //        ordersQuery = ordersQuery.Where(o => o.Status == status);
    //    }

    //    var orders = await ordersQuery
    //        .OrderByDescending(o => o.CreatedAt)
    //        .ToListAsync();

    //    return View(orders);
    //}
}

//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using System.Security.Claims;
//using WebGiayOnline.Data;

//namespace WebGiayOnline.Controllers;

//public class OrderController : Controller
//{
//    private readonly ApplicationDbContext _context;

//    public OrderController(ApplicationDbContext context)
//    {
//        _context = context;
//    }

//    public async Task<IActionResult> MyOrders()
//    {
//        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//        if (userId == null)
//            return RedirectToAction("Login", "Account");

//        var orders = await _context.Orders
//            .Include(o => o.OrderDetails)
//                .ThenInclude(d => d.Giay)
//            .Where(o => o.UserId == userId)
//            .OrderByDescending(o => o.CreatedAt)
//            .ToListAsync();

//        return View(orders);
//    }
//}
