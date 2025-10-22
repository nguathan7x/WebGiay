using Microsoft.AspNetCore.Mvc;
using WebGiayOnline.Data;
using WebGiayOnline.Models;
using Microsoft.EntityFrameworkCore;
using WebGiayOnline.Models.Orders;

namespace WebGiayOnline.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ShoppingCartController(ApplicationDbContext context)
        {
            _context = context;
        }

     
        public IActionResult Index()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Redirect("/Identity/Account/Login");
                //return RedirectToAction("Login", "Account");
            }

            var shoppingCart = _context.ShoppingCarts
                .Where(s => s.UserId == userId)
                .Select(s => new
                {
                    s.ShoppingCartId,
                    Items = s.Items.Select(i => new
                    {
                        i.ShoppingCartItemId,
                        i.GiayId,
                        i.Quantity,
                        i.Price,
                        ProductName = i.Giay.Name,  // Giả sử thuộc tính tên sản phẩm là TenGiay
                        ProductImage = i.Giay.ImageUrl // Giả sử có ảnh
                    }).ToList()
                }).FirstOrDefault();

            if (shoppingCart == null)
            {
                return View(new List<ShoppingCartItem>()); // Giỏ hàng rỗng
            }

            // Lấy danh sách ShoppingCartItem entity đầy đủ để truyền view
            var items = _context.ShoppingCartItems
                .Where(i => i.ShoppingCartId == shoppingCart.ShoppingCartId)
                .Include(i => i.Giay)
                .ToList();

            return View(items);
        }

        [HttpPost]
        public IActionResult AddToCart(int productId, int sizeId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Redirect("/Identity/Account/Login");
                //return RedirectToAction("Login", "Account");
            }

            // Lấy sản phẩm và giảm giá
            var product = _context.Giays
                .Include(g => g.DiscountGiays)
                    .ThenInclude(dg => dg.Discount)
                .FirstOrDefault(g => g.GiayId == productId);

            if (product == null)
                return RedirectToAction("Index", "Products");

            // Tìm giảm giá đang có hiệu lực
            var now = DateTime.Now;
            var activeDiscount = product.DiscountGiays
                .Select(dg => dg.Discount)
                .Where(d => d.IsActive && d.StartDate <= now && d.EndDate >= now)
                .OrderByDescending(d => d.Percentage)
                .FirstOrDefault();

            int finalPrice = product.Price;
            if (activeDiscount != null)
            {
                 finalPrice = (int)(product.Price * (1 - (decimal)activeDiscount.Percentage / 100));


                //finalPrice = product.Price * (1 - activeDiscount.Percentage / 100);
            }

            // Kiểm tra tồn kho size
            var sizeInfo = _context.GiaySizes               
        .Include(gs => gs.Size)
                .FirstOrDefault(s => s.GiayId == productId && s.SizeId == sizeId);

            if (sizeInfo == null || sizeInfo.Quantity <= 0)
            {
                TempData["Error"] = "Size này đã hết hàng!";
                return RedirectToAction("Detail", new { id = productId });
            }

            // Tìm hoặc tạo giỏ hàng
            var shoppingCart = _context.ShoppingCarts
                .FirstOrDefault(s => s.UserId == userId);

            if (shoppingCart == null)
            {
                shoppingCart = new ShoppingCart { UserId = userId };
                _context.ShoppingCarts.Add(shoppingCart);
                _context.SaveChanges(); // lấy ShoppingCartId
            }

            // Kiểm tra sản phẩm + size trong giỏ hàng
            var cartItem = _context.ShoppingCartItems
                .FirstOrDefault(i => i.ShoppingCartId == shoppingCart.ShoppingCartId
                                  && i.GiayId == productId
                                  && i.SizeId == sizeId);

            if (cartItem != null)
            {
                cartItem.Quantity++;
            }
            else
            {
                cartItem = new ShoppingCartItem
                {
                    ShoppingCartId = shoppingCart.ShoppingCartId,
                    GiayId = productId,
                    SizeId = sizeId,
                    Quantity = 1,
                    Price = finalPrice
                };
                _context.ShoppingCartItems.Add(cartItem);
            }

            _context.SaveChanges();

            return RedirectToAction("Index", "ShoppingCart");
        }


        [HttpPost]
        public IActionResult RemoveFromCart(int cartItemId)
        {
            var cartItem = _context.ShoppingCartItems.Find(cartItemId);
            if (cartItem != null)
            {
                _context.ShoppingCartItems.Remove(cartItem);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult UpdateQuantity(int cartItemId, int quantity)
        {
            if (quantity < 1)
            {
                return BadRequest("Số lượng phải lớn hơn 0");
            }

            var cartItem = _context.ShoppingCartItems.Find(cartItemId);
            if (cartItem != null)
            {
                cartItem.Quantity = quantity;
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ClearCart()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var shoppingCart = _context.ShoppingCarts
                .FirstOrDefault(s => s.UserId == userId);

            if (shoppingCart != null)
            {
                var items = _context.ShoppingCartItems
                    .Where(i => i.ShoppingCartId == shoppingCart.ShoppingCartId);

                _context.ShoppingCartItems.RemoveRange(items);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }


        
        public async Task<IActionResult> Checkout()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Redirect("/Identity/Account/Login");
            }

            var cart = await _context.ShoppingCarts
                .Include(c => c.Items)
                .ThenInclude(i => i.Giay)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.Items.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index");
            }

            var orderInfo = new OrderInfoModel
            {
                OrderId = Guid.NewGuid().ToString("N"),
                FullName = User.Identity.Name ?? "Khách hàng",
                Amount = cart.Items.Sum(i => i.Price * i.Quantity),
                OrderInfo = $"Thanh toán đơn hàng của {User.Identity.Name}"
                //ReturnUrl = "https://localhost:7181/Payment/PaymentCallBack",
                //NotifyUrl = "https://localhost:7181/Payment/Notify",
            };
           // return RedirectToAction("CreatePaymentUrl", "Payment", orderInfo);


            return View("~/Views/ShoppingCart/ConfirmPayment.cshtml", orderInfo);


            //return View("ConfirmPayment", orderInfo); // View có form ẩn và tự submit
        }

      


    }

}
