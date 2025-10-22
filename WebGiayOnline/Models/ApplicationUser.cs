using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WebGiayOnline.Models
{
    public class ApplicationUser : IdentityUser
    {
        
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public string? Age { get; set; }
        public bool IsEnabled { get; set; } = true;
        public string? AvatarUrl { get; set; }
        public string? AvatarPublicId { get; set; }

        //public string? CoverImageUrl { get; set; }
    }
}
