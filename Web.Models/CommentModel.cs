namespace Web.Models
{
    public class CommentModel
    {
        public Guid Id { get; set; }
        public DateTime PublicationDateTime { get; set; }
        public required string Text { get; set; }
        public required string UserName { get; set; }
        public bool IsEditing { get; set; }
    }
}
