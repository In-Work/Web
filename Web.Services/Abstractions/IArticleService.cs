using Web.Data.Entities;

namespace Web.Services.Abstractions;

public interface IArticleService
{
    Task<List<Article>?> GetArticlesAsync(CancellationToken token);
    Task<Article?> GetArticleByIdAsync(Guid articleId, CancellationToken token);
    Task<List<Comment>?> GetCommentsByArticleId(Guid articleId, CancellationToken token);
}