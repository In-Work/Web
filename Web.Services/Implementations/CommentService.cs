using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Data.Entities;
using Web.Services.Abstractions;

namespace Web.Services.Implementations;

public class CommentService : ICommentService
{
    private readonly ApplicationContext _context;
    public CommentService(ApplicationContext context)
    {
        _context = context;
    }

    public async Task AddCommentByArticleIdAsync(Guid articleId, Guid userId, string commentText, CancellationToken token = default)
    {
        var article = await _context.Articles
            .FirstOrDefaultAsync(a => a.Id == articleId, token)
            .ConfigureAwait(false);

        if (article == null)
        {
            throw new ArgumentException("Article not found", nameof(articleId));
        }

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            PublicationDateTime = DateTime.Now,
            Text = commentText,
            ArticleId = articleId,
            UserId = userId
        };

        await _context.Comments.AddAsync(comment, token).ConfigureAwait(false);
        await _context.SaveChangesAsync(token).ConfigureAwait(false);
    }

    public async Task<List<Comment>?> GetCommentsByArticleId(Guid articleId, CancellationToken token = default)
    {
        return await _context.Comments
            .AsNoTracking()
            .Where(c => c.ArticleId.Equals(articleId))
            .Include(c => c.User)
            .ToListAsync(token);
    }

    public async Task<Comment?> GetCommentById(Guid commentId, CancellationToken token = default)
    {
        return await _context.Comments
            .AsNoTracking()
            .Where(c => c.Id.Equals(commentId))
            .Include(c => c.User)
            .FirstOrDefaultAsync(token)
            .ConfigureAwait(false);
    }
}