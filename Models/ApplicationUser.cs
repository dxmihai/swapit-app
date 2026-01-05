using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace vinted2.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? DisplayName { get; set; }
        public string? AvatarPath { get; set; }

        // Navigations
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
