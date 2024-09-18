namespace Web.DTOs
{
    public class UserTokenDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public List<string> RoleNames { get; set; }
        public Guid RefreshToken { get; set; }
    }
}
