using WebGiayOnline.Models;
namespace WebGiayOnline.Models;
public class OrderStatusHistory
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public string OldStatus { get; set; }
    public string NewStatus { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.Now;
    public string ChangedBy { get; set; } // lưu email, username hoặc userId của người cập nhật

    // Navigation
    public Order Order { get; set; }
}
