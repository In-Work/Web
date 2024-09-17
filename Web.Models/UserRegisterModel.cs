using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace Web.Models
{
    public class UserRegisterModel
    {
        [Required]
        public required string Name { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Password must contain at least 6 characters", MinimumLength = 6)]
        public required string Password { get; set; }
    }
}
