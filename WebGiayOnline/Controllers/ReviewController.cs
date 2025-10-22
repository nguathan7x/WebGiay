using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebGiayOnline.Data;
using WebGiayOnline.Models;
using WebGiayOnline.ViewModels;

namespace WebGiayOnline.Controllers
{
    [Authorize(Roles = SD.Role_Customer)]
    public class ReviewController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReviewController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> Submit(ReviewViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // Kiểm tra đơn hàng chứa sản phẩm (theo OrderDetailId)
            var orderDetail = await _context.OrderDetails
                .Include(od => od.Order)
                .FirstOrDefaultAsync(od =>
                    od.OrderDetailId == model.OrderDetailId &&
                    od.GiayId == model.GiayId &&
                    od.Order.UserId == user.Id &&
                    od.Order.Status == "Hoàn thành");

            if (orderDetail == null)
            {
                TempData["Message"] = "Bạn không thể đánh giá sản phẩm này.";
                return RedirectToAction("Details", "Product", new { id = model.GiayId });
            }

            // Chỉ cho phép đánh giá trong 7 ngày
            if ((DateTime.Now - orderDetail.Order.CreatedAt).TotalDays > 7)
            {
                TempData["Message"] = "Thời gian đánh giá đã hết (chỉ cho phép đánh giá trong vòng 7 ngày sau khi mua).";
                return RedirectToAction("Details", "Product", new { id = model.GiayId });
            }

            // Kiểm tra nếu đã đánh giá OrderDetail này rồi
            var alreadyRated = await _context.Reviews.AnyAsync(r => r.OrderDetailId == model.OrderDetailId);
            if (alreadyRated)
            {
                TempData["Message"] = "Bạn đã đánh giá sản phẩm này cho đơn hàng này rồi.";
                return RedirectToAction("Details", "Product", new { id = model.GiayId });
            }

            // Ghi nhận đánh giá mới
            var review = new Review
            {
                GiayId = model.GiayId,
                UserId = user.Id,
                Content = model.Content,
                Rating = model.Rating,
                ReviewDate = DateTime.Now,
                OrderDetailId = model.OrderDetailId // Gắn vào OrderDetail cụ thể
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Đánh giá của bạn đã được ghi nhận!";
            return RedirectToAction("Details", "Product", new { id = model.GiayId });
        }

        [AllowAnonymous]
        public async Task<IActionResult> ProductReviews(int giayId)
        {
            var product = await _context.Giays
                .Include(g => g.Reviews)
                .FirstOrDefaultAsync(g => g.GiayId == giayId);

            if (product == null) return NotFound();

            var reviews = product.Reviews.OrderByDescending(r => r.ReviewDate).ToList();

            var reviewVMs = new List<ReviewDisplayViewModel>();

            foreach (var review in reviews)
            {
                var user = await _userManager.FindByIdAsync(review.UserId);

                reviewVMs.Add(new ReviewDisplayViewModel
                {
                    FullName = user?.FullName ?? "Người dùng ẩn danh",
                    AvatarUrl = !string.IsNullOrEmpty(user?.AvatarUrl) ? user.AvatarUrl : "/images/default-avatar.png",
                    Content = review.Content,
                    Rating = review.Rating,
                    ReviewDate = review.ReviewDate
                });
            }

            ViewBag.GiayName = product.Name;
            ViewBag.GiayId = giayId;
            ViewBag.TotalReviews = reviews.Count;

            var summary = reviews
                .GroupBy(r => r.Rating)
                .Select(g => new { Rating = g.Key, Count = g.Count() })
                .ToDictionary(g => g.Rating, g => g.Count);

            ViewBag.RatingSummary = summary;
            ViewBag.AvgRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;

            return View(reviewVMs);
        }

      
    }
}
