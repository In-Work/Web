namespace Web.Data.Entities
{
    public class Comment
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public DateTime Date { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
        public Guid ArticleId { get; set; }
        public Article Article { get; set; }
    }
}
