using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WebGiayOnline.Models.Momo;
using WebGiayOnline.Models.Orders;
using RestSharp;
using Microsoft.AspNetCore.Http;

namespace WebGiayOnline.Services
{
    public class MomoService : IMomoService
    {
        private readonly IOptions<MomoOptionModel> _options;

        public MomoService(IOptions<MomoOptionModel> options)
        {
            _options = options;
        }

        public async Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(OrderInfoModel model)
        {
            model.OrderId = Guid.NewGuid().ToString("N");
            model.OrderInfo = $"Khách hàng: {model.FullName}. Nội dung: {model.OrderInfo}";
            string requestId = model.OrderId;

            // Tạo raw data đúng format V2
            string rawHash =
                $"accessKey={_options.Value.AccessKey}&amount={model.Amount}&extraData=&ipnUrl={_options.Value.NotifyUrl}&orderId={model.OrderId}&orderInfo={model.OrderInfo}&partnerCode={_options.Value.PartnerCode}&redirectUrl={_options.Value.ReturnUrl}&requestId={requestId}&requestType={_options.Value.RequestType}";

            string signature = ComputeHmacSha256(rawHash, _options.Value.SecretKey);

            var requestData = new
            {
                partnerCode = _options.Value.PartnerCode,
                accessKey = _options.Value.AccessKey,
                requestId = requestId,
                amount = model.Amount.ToString(),
                orderId = model.OrderId,
                orderInfo = model.OrderInfo,
                redirectUrl = _options.Value.ReturnUrl,
                ipnUrl = _options.Value.NotifyUrl,
                extraData = "",
                requestType = _options.Value.RequestType,
                signature = signature,
                lang = "vi"
            };

            var client = new RestClient(_options.Value.MomoApiUrl);
            var request = new RestRequest("", Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddJsonBody(requestData);

            var response = await client.ExecuteAsync(request);

            Console.WriteLine("MoMo response:");
            Console.WriteLine(response.Content);

            return JsonConvert.DeserializeObject<MomoCreatePaymentResponseModel>(response.Content);
        }

        public Task<MomoExecuteResponseModel> PaymentExecuteAsync(IQueryCollection collection)
        {
            var amount = collection["amount"].ToString();
            var orderInfo = collection["orderInfo"].ToString();
            var orderId = collection["orderId"].ToString();
            var resultCode = collection["resultCode"].ToString();

            var result = new MomoExecuteResponseModel()
            {
                Amount = amount,
                OrderId = orderId,
                OrderInfo = orderInfo,
                ResultCode = resultCode // 👈 gán vào đây
            };

            return Task.FromResult(result);
        }

        //public Task<MomoExecuteResponseModel> PaymentExecuteAsync(IQueryCollection collection)
        //{
        //    var amount = collection.FirstOrDefault(s => s.Key == "amount").Value.ToString();
        //    var orderInfo = collection.FirstOrDefault(s => s.Key == "orderInfo").Value.ToString();
        //    var orderId = collection.FirstOrDefault(s => s.Key == "orderId").Value.ToString();

        //    var result = new MomoExecuteResponseModel()
        //    {
        //        Amount = amount,
        //        OrderId = orderId,
        //        OrderInfo = orderInfo
        //    };

        //    return Task.FromResult(result);
        //}

        private string ComputeHmacSha256(string message, string secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var messageBytes = Encoding.UTF8.GetBytes(message);

            using (var hmac = new HMACSHA256(keyBytes))
            {
                byte[] hashBytes = hmac.ComputeHash(messageBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}
