using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace vinted2.ViewModels
{
    public class ProfileViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Display Name")]
        public string? DisplayName { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string? Password { get; set; }

        [Display(Name = "Avatar")]
        public IFormFile? AvatarFile { get; set; }

        public string? AvatarPath { get; set; }
    }
}
