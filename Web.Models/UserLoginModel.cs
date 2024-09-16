using System.ComponentModel.DataAnnotations;

namespace Web.Models
{
    public class UserLoginModel
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
