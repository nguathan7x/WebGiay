namespace WebGiayOnline.Models.Momo;

public class MomoExecuteResponseModel
{
    public string OrderId { get; set; }
    public string Amount { get; set; }
    public string OrderInfo { get; set; }

    public string ResultCode { get; set; }  // ?? thêm dòng này

    public bool IsSuccess => ResultCode == "0"; // ?? tính toán t? ??ng
}
