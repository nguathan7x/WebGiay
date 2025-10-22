using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using WebGiayOnline.Data;
using WebGiayOnline.Models;
using WebGiayOnline.Models.Orders;
using WebGiayOnline.Services;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net.Mail;
using System.Net;



namespace WebGiayOnline.Controllers;

[Route("Payment")]
public class PaymentController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IMomoService _momoService;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _configuration;

    public PaymentController(ApplicationDbContext context, IMomoService momoService, IEmailSender emailSender, IConfiguration configuration)
    {
        _context = context;
        _momoService = momoService;
        _emailSender = emailSender;
        _configuration = configuration;
    }

    [HttpPost("Create")]
    public async Task<IActionResult> CreatePaymentUrl(OrderInfoModel model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        model.OrderInfo += $" | UserId={userId}";

        // Lấy giỏ hàng
        var cart = await _context.ShoppingCarts
            .Include(c => c.Items).ThenInclude(i => i.Giay)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null || !cart.Items.Any())
        {
            TempData["Error"] = "Giỏ hàng trống.";
            return RedirectToAction("Index", "ShoppingCart");
        }

        // Tạo đơn hàng (chưa thanh toán)
        var order = new Order
        {
            UserId = userId,
            CreatedAt = DateTime.Now,
            Total = model.Amount,
            Status = "Chờ xác nhận", // Trạng thái tạm thời
            OrderDetails = new List<OrderDetail>()
        };

        foreach (var item in cart.Items)
        {
            order.OrderDetails.Add(new OrderDetail
            {
                GiayId = item.GiayId,
                SizeId = item.SizeId,
                Quantity = item.Quantity,
                Price = item.Price
            });
        }

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Lấy thông tin user
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            var pdfBytes = GenerateInvoicePdf(
                customerName: user.FullName ?? user.Email,
                orderId: order.OrderId.ToString(),
                amount: order.Total,
                date: order.CreatedAt
            );

            var subject = $"Xác nhận đơn hàng #{order.OrderId}";
            var htmlMessage = $@"
        <p>Chào {user.FullName ?? user.Email},</p>
        <p>Chúng tôi đã tạo đơn hàng của bạn và đang chờ thanh toán.</p>
        <p><strong>Mã đơn:</strong> {order.OrderId}</p>
        <p><strong>Số tiền:</strong> {order.Total:N0} VND</p>
        <p>Hóa đơn được đính kèm trong email này.</p>
        <p>Xin cảm ơn quý khách!</p>
    ";

            await SendEmailWithAttachment(user.Email, subject, htmlMessage, pdfBytes, $"Invoice_{order.OrderId}.pdf");
        }

        // Sau đó gọi Momo
        var response = await _momoService.CreatePaymentAsync(model);
        if (response != null && response.ErrorCode == 0 && !string.IsNullOrEmpty(response.PayUrl))
        {
            // Xoá giỏ hàng vì đã đặt đơn
            _context.ShoppingCartItems.RemoveRange(cart.Items);
            _context.ShoppingCarts.Remove(cart);
            await _context.SaveChangesAsync();

            return Redirect(response.PayUrl);
        }

        return Content("Không tạo được liên kết thanh toán: " + (response?.Message ?? "Lỗi không xác định"));
    }


    [HttpGet("PaymentCallBack")]
    public async Task<IActionResult> PaymentCallBack()
    {
        var paymentResult = await _momoService.PaymentExecuteAsync(HttpContext.Request.Query);

        // Giả sử bạn thêm thuộc tính IsSuccess trong MomoExecuteResponseModel
        if (paymentResult != null && paymentResult.IsSuccess)
        {
            return View("PaymentCallBack", paymentResult);
        }

        // Nếu thanh toán bị hủy hoặc thất bại
        return View("PaymentCallBack", paymentResult);
    }



    //[HttpGet("PaymentCallBack")]
    //public async Task<IActionResult> PaymentCallBack()
    //{
    //    var paymentResult = await _momoService.PaymentExecuteAsync(HttpContext.Request.Query);

    //    // Giả sử bạn thêm thuộc tính IsSuccess trong MomoExecuteResponseModel
    //    if (paymentResult != null && paymentResult.IsSuccess)
    //    {
    //        return View("PaymentCallBack", paymentResult);
    //    }

    //    // Nếu thanh toán bị hủy hoặc thất bại
    //    return View("PaymentCallBack", paymentResult);
    //}



    /// ✅ Nhận callback từ Momo (IPN), không cần đăng nhập
    [HttpPost("MomoNotify")]
    public async Task<IActionResult> MomoNotify([FromBody] JObject data)
    {
        var errorCode = data["errorCode"]?.ToString();
        var orderId = data["orderId"]?.ToString();
        var amountStr = data["amount"]?.ToString();
        var orderInfo = data["orderInfo"]?.ToString();

        if (errorCode == "0" && !string.IsNullOrEmpty(orderId))
        {
            var userId = GetUserIdFromOrderInfo(orderInfo);

            if (string.IsNullOrWhiteSpace(userId))
                return Ok("Không tìm thấy UserId.");

            var cart = await _context.ShoppingCarts
                .Include(c => c.Items).ThenInclude(i => i.Giay)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart != null && cart.Items.Any())
            {
                var order = new Order
                {
                    UserId = userId,
                    Total = decimal.Parse(amountStr),
                    Status = "Đã thanh toán",
                    CreatedAt = DateTime.Now,
                    OrderDetails = new List<OrderDetail>()
                };

                foreach (var item in cart.Items)
                {
                    var orderDetail = new OrderDetail
                    {
                        GiayId = item.GiayId,
                        SizeId = item.SizeId,
                        Quantity = item.Quantity,
                        Price = item.Price
                    };
                    order.OrderDetails.Add(orderDetail);

                    // Giảm tồn kho
                    var giaySize = await _context.GiaySizes
                        .FirstOrDefaultAsync(gs => gs.GiayId == item.GiayId && gs.SizeId == item.SizeId);

                    if (giaySize != null)
                        giaySize.Quantity -= item.Quantity;
                }

                _context.Orders.Add(order);
                _context.ShoppingCartItems.RemoveRange(cart.Items);
                _context.ShoppingCarts.Remove(cart);

                await _context.SaveChangesAsync();
            }
        }

        return Ok();
    }

    /// ✅ Tách UserId ra từ OrderInfo
    private string GetUserIdFromOrderInfo(string orderInfo)
    {
        if (orderInfo.Contains("UserId="))
        {
            var parts = orderInfo.Split("UserId=");
            return parts.Last().Trim();
        }
        return "";
    }

    private byte[] GenerateInvoicePdf(string customerName, string orderId, decimal amount, DateTime date)
    {
        using var stream = new MemoryStream();
        var document = new PdfDocument();
        var page = document.AddPage();
        var gfx = XGraphics.FromPdfPage(page);
        var font = new XFont("Arial", 14, XFontStyle.Regular);

        gfx.DrawString("HÓA ĐƠN THANH TOÁN", new XFont("Arial", 18, XFontStyle.Bold), XBrushes.Black, new XRect(0, 30, page.Width, 40), XStringFormats.TopCenter);
        gfx.DrawString($"Khách hàng: {customerName}", font, XBrushes.Black, 50, 100);
        gfx.DrawString($"Mã đơn hàng: {orderId}", font, XBrushes.Black, 50, 130);
        gfx.DrawString($"Tổng tiền: {amount:N0} VND", font, XBrushes.Black, 50, 160);
        gfx.DrawString($"Thời gian: {date:dd/MM/yyyy HH:mm}", font, XBrushes.Black, 50, 190);

        document.Save(stream, false);
        return stream.ToArray();
    }

    private async Task SendEmailWithAttachment(string toEmail, string subject, string htmlMessage, byte[] attachmentBytes, string fileName)
    {
        var fromEmail = _configuration["EmailSettings:Gmail:Email"];
        var password = _configuration["EmailSettings:Gmail:AppPassword"];

        var smtpClient = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new NetworkCredential(fromEmail, password),
            EnableSsl = true
        };

        var message = new MailMessage
        {
            From = new MailAddress(fromEmail),
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true
        };
        message.To.Add(toEmail);

        if (attachmentBytes != null)
        {
            var stream = new MemoryStream(attachmentBytes);
            var attachment = new Attachment(stream, fileName, "application/pdf");
            message.Attachments.Add(attachment);
        }

        await smtpClient.SendMailAsync(message);
    }

}
