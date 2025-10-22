namespace WebGiayOnline.Models.Momo;

public class MomoExecuteResponseModel
{
    public string OrderId { get; set; }
    public string Amount { get; set; }
    public string OrderInfo { get; set; }

    public string ResultCode { get; set; }  // ?? th�m d�ng n�y

    public bool IsSuccess => ResultCode == "0"; // ?? t�nh to�n t? ??ng
}
