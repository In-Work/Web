using System.ComponentModel.DataAnnotations;

namespace Web.Models;

public class ResetPasswordModel
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "Password must contain at least 6 characters", MinimumLength = 6)]
    [DataType(DataType.Password)]
    public required string Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "Passwords don't match")]
    public required string ConfirmPassword { get; set; }
}