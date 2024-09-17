using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Data.Entities;
using Web.Services.Abstractions;

namespace Web.Services.Implementations
{
    public class ArticleService : IArticleService
    {
        private readonly ApplicationContext _context;

        public ArticleService(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<List<Article>?> GetArticlesAsync(CancellationToken token = default)
        {
            return await _context.Articles
                .AsNoTracking()
                .Include(a => a.Comments)
                .Include(a => a.Source)
                .OrderByDescending(a => a.PublicationDate)
                .ToListAsync(token);
        }

        public async Task<Article?> GetArticleByIdAsync(Guid articleId, CancellationToken token = default)
        {
            return await _context.Articles
                .AsNoTracking()
                .Where(a => a.Id.Equals(articleId))
                .Include(a => a.Comments)!
                .ThenInclude(c => c.User)
                .Include(a => a.Source)
                .FirstOrDefaultAsync(token);
        }

        public async Task<List<Comment>?> GetCommentsByArticleId(Guid articleId, CancellationToken token = default)
        {
            return await _context.Comments
                .Where(c => c.ArticleId == articleId)
                .ToListAsync(token)
                .ConfigureAwait(false);
        }
    }
}
