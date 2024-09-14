namespace Web.Models
{
    public class SourceModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string? BaseUrl { get; set; }
        public string? RssUrl { get; set; }
    }
}
