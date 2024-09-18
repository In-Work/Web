using HtmlAgilityPack;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ServiceModel.Syndication;
using System.Xml;
using Web.Data;
using Web.Data.Entities;
using Web.DataAccess.CQS.Commands.Articles;
using Web.DTOs;
using Web.Services.Abstractions;

namespace Web.Services.Implementations
{
    public class ArticleService : IArticleService
    {
        private readonly ApplicationContext _context;
        private readonly IMediator _mediator;

        public ArticleService(ApplicationContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
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

        public async Task UpdateCommentById(Guid commentId, string commentText, CancellationToken token = default)
        {
            var comment = await _context.Comments
                .Include(c => c.Article)
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == commentId, token)
                .ConfigureAwait(false);

            comment!.Text = commentText;
            await _context.SaveChangesAsync(token).ConfigureAwait(false);
        }

        public async Task AggregateArticleAsync(CancellationToken token = default)
        {
            try
            {
                var sources = await GetArticleSourcesAsync(token);

                foreach (var source in sources!)
                {
                    await InsertUniqueArticlesFromRssDataAsync(source, token);
                }

                var articles = await GetArticlesDataWithoutTextAsync(token);

                foreach (var article in articles)
                {
                    await UpdateTextByWebScrapping(article);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            //get id from sources in dbContext

            //await Task.WhenAll(tasks);
        }
        public async Task<List<Source>?> GetArticleSourcesAsync(CancellationToken token = default)
        {
            return await _context.Sources
                .AsNoTracking()
                .Where(s => !string.IsNullOrEmpty(s.RssUrl))
                .ToListAsync(token);
        }

        private async Task<List<Article>?> GetArticlesDataWithoutTextAsync(CancellationToken token = default)
        {
            return await _context.Articles
                .Where(article => string.IsNullOrEmpty(article.Text))
                .Include(a => a.Source)
                .AsNoTracking()
                .ToListAsync(token);
        }

        public async Task InsertUniqueArticlesFromRssDataAsync(Source source, CancellationToken token = default)
        {
            if (source?.RssUrl != null)
            {
                using var xmlReader = XmlReader.Create(source.RssUrl);
                var syndicationFeed = SyndicationFeed.Load(xmlReader);

                var articles = syndicationFeed.Items
                .Select(item => new ArticleDto()
                    {
                        Id = Guid.NewGuid(),
                        Title = HtmlEntity.DeEntitize(item.Title.Text),
                        OriginalUrl = item.Id,
                        Description = FormatHtmlContent(item.Summary?.Text),
                        PublicationDate = item.PublishDate.DateTime,
                        SourceId = source.Id,
                        SourceName = source.Title
                    }
                ).ToList();

                await _mediator.Send(new InsertUniqueArticlesFromRssDataCommand()
                {
                    Articles = articles
                }, token);
            }
        }

        private string FormatHtmlContent(string htmlContent)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);

            var images = doc.DocumentNode.SelectNodes("//img");
            if (images != null)
            {
                foreach (var img in images)
                {
                    img.Attributes.Remove("srcset");

                    var dataSrc = img.GetAttributeValue("data-src", null);
                    if (!string.IsNullOrEmpty(dataSrc))
                    {
                        img.SetAttributeValue("src", dataSrc);
                        img.Attributes.Remove("data-src");
                    }

                    var style = img.GetAttributeValue("style", string.Empty);
                    style += "max-width: 100%; height: auto; display: block; margin: 0 auto; border-radius: 4px;";
                    img.SetAttributeValue("style", style);
                }
            }

            var paragraphs = doc.DocumentNode.SelectNodes("//p");
            if (paragraphs != null)
            {
                foreach (var p in paragraphs)
                {
                    var style = p.GetAttributeValue("style", string.Empty);
                    style += "text-align: justify; margin-bottom: 1em;";
                    p.SetAttributeValue("style", style);
                }
            }

            var strongElements = doc.DocumentNode.SelectNodes("//strong");
            if (strongElements != null)
            {
                foreach (var strong in strongElements)
                {
                    if (strong.InnerText.StartsWith("Есть о чем рассказать? Пишите в наш"))
                    {
                        strong.Remove();
                    }
                }
            }

            var links = doc.DocumentNode.SelectNodes("//a");
            if (links != null)
            {
                foreach (var link in links)
                {
                    if (link.InnerText.StartsWith("Читать далее"))
                    {
                        link.Remove();
                    }
                }
            }

            var icons = doc.DocumentNode.SelectNodes("//i[@class='pvc-stats-icon medium']");
            if (icons != null)
            {
                foreach (var icon in icons)
                {
                    icon.Remove();
                }
            }

            var formattedHtml = HtmlEntity.DeEntitize(doc.DocumentNode.OuterHtml);

            return formattedHtml.Trim();
        }

        private async Task UpdateTextByWebScrapping(Article? article)
        {
            var web = new HtmlWeb();
            var doc = web.Load(article.OriginalUrl);

            var selector = string.Empty;
            switch (article.Source.Title)
            {
                case "onliner.by":
                    selector = "//div[@class='news-text']";
                    break;
                case "positivnews.ru":
                    selector = "//div[@itemprop='articleBody']";
                    break;
                case "habr.com":
                    selector = "//div[@class='tm-article-body']";
                    break;
            }

            var articleNode = doc.DocumentNode.SelectSingleNode(selector);

            if (articleNode != null)
            {
                var existingArticle = await _context.Articles
                    .FirstOrDefaultAsync(a => a.Id == article.Id);

                if (existingArticle != null)
                {
                    existingArticle.Text = FormatHtmlContent(articleNode.InnerHtml);
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}
