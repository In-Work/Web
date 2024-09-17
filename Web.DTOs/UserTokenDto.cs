namespace Web.DTOs
{
    public class UserTokenDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string RoleName { get; set; }
        public Guid RefreshToken { get; set; }
    }
}
