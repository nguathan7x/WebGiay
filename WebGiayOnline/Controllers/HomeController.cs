using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebGiayOnline.Data;
using WebGiayOnline.Models;
using WebGiayOnline.ViewModels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
namespace WebGiayOnline.Controllers
{
    //[AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index(string? searchTerm, string gender, int? brandId, string? size, string priceRange, bool? hasDiscount)
        {
            var today = DateTime.Now;

            var giaysQuery = _context.Giays
                .Include(g => g.GiaySizes)
                .Include(g => g.ProductImages)
                .AsQueryable();

            //ViewBag.TotalShoes = giaysQuery.Count();


            if (!string.IsNullOrEmpty(searchTerm))
                giaysQuery = giaysQuery.Where(g => g.Name.Contains(searchTerm));

            if (!string.IsNullOrEmpty(gender))
                giaysQuery = giaysQuery.Where(g => g.Gender.ToLower() == gender.ToLower());

            if (brandId.HasValue)
                giaysQuery = giaysQuery.Where(g => g.BrandId == brandId.Value);

            if (!string.IsNullOrEmpty(size))
            {
                giaysQuery = giaysQuery.Where(g => g.GiaySizes.Any(s => s.Size.Label == size));
            }

            // L?c theo kho?ng giá
            if (!string.IsNullOrEmpty(priceRange))
            {
                var parts = priceRange.Split("-");
                if (parts.Length == 2 &&
                    decimal.TryParse(parts[0], out decimal min) &&
                    decimal.TryParse(parts[1], out decimal max))
                {
                    giaysQuery = giaysQuery.Where(g => g.Price >= min && g.Price <= max);
                }
            }

            var allGiays = giaysQuery.ToList();

            var discounts = _context.DiscountGiays
                .Include(dg => dg.Discount)
                .Where(dg => dg.Discount.IsActive == true
                          && dg.Discount.StartDate <= today
                          && dg.Discount.EndDate >= today)
                .ToList();

            var viewModel = allGiays.Select(g => new GiayWithDiscountViewModel
            {
                Giay = g,
                ActiveDiscount = discounts.FirstOrDefault(dg => dg.GiayId == g.GiayId)?.Discount,
                AvgRating = _context.Reviews.Where(r => r.GiayId == g.GiayId).Any()
                 ? _context.Reviews.Where(r => r.GiayId == g.GiayId).Average(r => r.Rating)
                 : 0,
                TotalReviews = _context.Reviews.Count(r => r.GiayId == g.GiayId)
            });


            // N?u ng??i dùng ch?n ch? hi?n th? s?n ph?m có khuy?n mãi
            if (hasDiscount == true)
            {
                viewModel = viewModel.Where(v => v.ActiveDiscount != null);
            }

            var finalList = viewModel.ToList();
            //ViewBag.TotalShoes = giaysQuery.Count();
            ViewBag.TotalShoes = finalList.Count();

            // Truy?n l?i các ViewBag ?? gi? tr?ng thái l?c
            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentGender = gender;
            ViewBag.CurrentBrandId = brandId;
            ViewBag.CurrentSize = size;
            ViewBag.CurrentPriceRange = priceRange;
            ViewBag.HasDiscount = hasDiscount;

            ViewBag.Brands = _context.Brands.ToList();
            ViewBag.AvailableGenders = new List<string> { "Men", "Women", "Unisex" };
            ViewBag.AvailableSizes = _context.GiaySizes.Select(s => s.Size.Label).Distinct().ToList();
         

            return View(finalList);
        }

        [HttpGet]
        public IActionResult SearchSuggestions(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new List<string>());

            var suggestions = _context.Giays
                .Where(g => g.Name.Contains(term))
                .Select(g => g.Name)
                .Distinct()
                .Take(10)
                .ToList();

            return Json(suggestions);
        }
        public IActionResult About()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
