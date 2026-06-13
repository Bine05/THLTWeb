using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WebBanHang.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string FullName { get; set; }
        public string? Address { get; set; }
        public string? Age { get; set; }
        public string? AvatarUrl { get; set; }

        // Mới thêm cho Module 2
        public int RewardPoints { get; set; } = 0;
    }
}
