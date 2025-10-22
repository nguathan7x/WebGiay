using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
//using WebGiayOnline.Models;

namespace WebGiayOnline.Models
{
    public class Order
    {
        public int OrderId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Total { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Chờ Xác nhận";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ApplicationUser? User { get; set; }
        public ICollection<OrderDetail>? OrderDetails { get; set; }
        public ICollection<OrderStatusHistory> StatusHistories { get; set; } = new List<OrderStatusHistory>();




    }
}