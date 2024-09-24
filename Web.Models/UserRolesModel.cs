using Web.Data.Entities;

namespace Web.Models
{
    public class UserRolesModel
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public required List<Role> UserRoles { get; set; }
    }
}
