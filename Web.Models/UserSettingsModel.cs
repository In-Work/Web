using System.ComponentModel.DataAnnotations;

namespace Web.Models
{
    public class UserSettingsModel
    {
        public string? UserName { get; set; }

        [Range(-5, 5, ErrorMessage = "MinRank must be between -5 and 5.")]
        [RegularExpression(@"^(-?[0-5]|[0-4](\.\d+)?|5(\.0+)?)$", ErrorMessage = "MinRank must be a number between -5 and 5.")]

        public int MinRank { get; set; }
    }
}