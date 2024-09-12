namespace Web.Models
{
    public class CommentModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Text { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public Guid UserId { get; set; }
        public UserModel UserModel { get; set; }
    }
}
