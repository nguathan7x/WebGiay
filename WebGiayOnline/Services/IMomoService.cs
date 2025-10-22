using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using WebGiayOnline.Models.Momo;
using WebGiayOnline.Models.Orders;

namespace WebGiayOnline.Services
{
    public interface IMomoService
    {
        // T?o URL thanh toán
        Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(OrderInfoModel model);

        // X? lý ph?n h?i khi thanh toán xong (redirect t? MoMo)
        Task<MomoExecuteResponseModel> PaymentExecuteAsync(IQueryCollection collection);
    }
}
