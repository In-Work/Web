namespace Web.Data.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public List<Role> UserRoles { get; set; } = new List<Role>();
    }
}
