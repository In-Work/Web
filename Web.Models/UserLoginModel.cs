using System.ComponentModel.DataAnnotations;

namespace Web.Models
{
    public class UserLoginModel
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Password must contain at least 6 characters", MinimumLength = 6)]
        public required string Password { get; set; }
    }
}
