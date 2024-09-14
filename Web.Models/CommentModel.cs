namespace Web.Models
{
    public class CommentModel
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public DateTime Date { get; set; }
        public Guid UserId { get; set; }
        public bool IsEditing { get; set; }
    }
}
