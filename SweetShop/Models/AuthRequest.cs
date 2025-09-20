using System.ComponentModel.DataAnnotations;

namespace SweetShop.Mvc.Models
{
    public class AuthRequest
    {
        public string? Name { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Email is not in proper format.")]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
