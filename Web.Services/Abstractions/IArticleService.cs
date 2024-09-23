using Web.Data.Entities;
using Web.Data.Migrations;

namespace Web.Services.Abstractions;

public interface IArticleService
{
    Task<List<Article>?> GetArticlesAsync(CancellationToken token);
    Task<Article?> GetArticleByIdAsync(Guid articleId, CancellationToken token);
    Task AggregateArticleAsync(CancellationToken token);
    Task<List<Comment>?> GetCommentsByArticleId(Guid articleId, CancellationToken token);
    Task UpdateCommentById(Guid commentId, string commentText, CancellationToken token);
    Task<List<Source>?> GetArticleSourcesAsync(CancellationToken token);
    Task PositivityAssessmentAsync(Dictionary<string, int?> afinnData, CancellationToken token);
}