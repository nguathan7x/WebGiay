using System.ComponentModel.DataAnnotations;

namespace WebGiayOnline.Models
{
    public class Discount
    {
        public int DiscountId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Range(0, 100)]
        public decimal Percentage { get; set; }

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<DiscountGiay> DiscountGiays { get; set; } = new List<DiscountGiay>();
    }

}
