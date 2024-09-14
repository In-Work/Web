namespace Web.Models;

public class ArticleWithCommentsModel
{
    public ArticleModel ArticleModel { get; set; }
    public List<CommentModel> CommentsModels  { get; set; }
}