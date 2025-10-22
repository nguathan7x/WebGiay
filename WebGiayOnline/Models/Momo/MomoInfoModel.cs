using System.ComponentModel.DataAnnotations;

namespace WebGiayOnline.Models.Momo
{
    public class MomoInfoModel
    {
        [Key]
        public string OrderId { get; set; }           // Mã đơn hàng bạn tạo
        public string RequestId { get; set; }         // Mã yêu cầu gửi đến Momo
        public string TransId { get; set; }           // Mã giao dịch do Momo cung cấp
        public string OrderInfo { get; set; }         // Mô tả đơn hàng
        public int Amount { get; set; }               // Số tiền thanh toán
        public int ResultCode { get; set; }           // 0 = Thành công
        public string Message { get; set; }           // Mô tả trạng thái
        public DateTime PaymentTime { get; set; }     // Thời điểm thanh toán (lấy từ hệ thống của bạn hoặc Momo)
        public string ExtraData { get; set; }         // Dữ liệu thêm nếu có
        public string Signature { get; set; }         // Chữ ký để xác thực
    }

}
