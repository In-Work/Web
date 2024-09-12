namespace Web.Data.Entities
{
    public class Source
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string? BaseUrl { get; set; }
        public string? RssUrl { get; set; }
        public List<Article>? Articles { get; set; }
    }
}
