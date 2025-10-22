using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using WebGiayOnline.Models.Momo;
using WebGiayOnline.Models.Orders;

namespace WebGiayOnline.Services
{
    public interface IMomoService
    {
        // T?o URL thanh to�n
        Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(OrderInfoModel model);

        // X? l� ph?n h?i khi thanh to�n xong (redirect t? MoMo)
        Task<MomoExecuteResponseModel> PaymentExecuteAsync(IQueryCollection collection);
    }
}
