using System.ComponentModel.DataAnnotations;

namespace Web.Models;

public class ForgotPasswordModel
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }
}