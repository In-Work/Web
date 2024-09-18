using Web.Data;
using Web.Mapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Web.DataAccess.CQS.Commands.Articles;

public class InsertUniqueArticlesFromRssDataCommandHandler :
    IRequestHandler<InsertUniqueArticlesFromRssDataCommand>
{
    private readonly ApplicationContext _context;
    
    public InsertUniqueArticlesFromRssDataCommandHandler(ApplicationContext context)  
    {
        _context = context;
    }
    
    public async Task Handle(InsertUniqueArticlesFromRssDataCommand command, 
        CancellationToken cancellationToken = default)
    {
        var existedArticleUrls = await _context.Articles
            .AsNoTracking()
            .Select(article => article.OriginalUrl)
            .ToArrayAsync(cancellationToken);
        
        var articles = command.Articles
            .Where(article => !existedArticleUrls.Contains(article.OriginalUrl))
            .Select(ApplicationMapper.ArticleDtoToArticle).ToList();
        
        await _context.Articles.AddRangeAsync(articles, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}