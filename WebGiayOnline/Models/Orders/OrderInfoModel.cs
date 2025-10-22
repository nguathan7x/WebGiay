namespace WebGiayOnline.Models.Orders;

using System.ComponentModel.DataAnnotations;

public class OrderInfoModel
{
    [Required]
    public string FullName { get; set; }

    public string OrderId { get; set; }

    [Required]
    public string OrderInfo { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Số tiền phải lớn hơn 0")]
    public int Amount { get; set; }
}

