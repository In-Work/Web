namespace Web.Models
{
    public class ArticleModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Text { get; set; }
        public string? OriginalUrl { get; set; }
        public DateTime PublicationDate { get; set; } = DateTime.Now;
        public double Rate { get; set; }
        public SourceModel? SourceModel { get; set; }
        public List<CommentModel>? CommentsModels { get; set; }
    }
}
