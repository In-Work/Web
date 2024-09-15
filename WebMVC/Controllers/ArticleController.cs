using System.Net;
using System.Security.Claims;
using System.ServiceModel.Syndication;
using System.Xml;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Data.Entities;
using Web.Mapper;
using Web.Models;

namespace Web.MVC.Controllers
{
    public class ArticleController : Controller
    {
        private readonly ApplicationContext _context;

        public ArticleController(ApplicationContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var articles = await _context.Articles
                .AsNoTracking()
                .Include(a => a.Source)
                .ToListAsync();

            var models = ApplicationMapper.ArticleListToArticleModelList(articles);
            return View(models);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid articleId)
        {
            var article = await _context.Articles
                .AsNoTracking()
                .Include(a => a.Source)
                .FirstOrDefaultAsync(a => a.Id.Equals(articleId));

            var comments = await _context.Comments
                .AsNoTracking()
                .Where(c => c.ArticleId.Equals(articleId))
                .Include(c => c.User)
                .ToListAsync();

            if (article != null)
            {
                var articleWithCommentsModels = new ArticleWithCommentsModel()
                {
                    ArticleModel = ApplicationMapper.ArticleToArticleModel(article),
                    CommentsModels = ApplicationMapper.CommentsModelsToCommentsList(comments)
                };

                return View(articleWithCommentsModels);
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(Guid articleId, string commentText)
        {
            var article = await _context.Articles.Where(a => a.Id.Equals(articleId)).FirstOrDefaultAsync();
            Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId);

            var comment = new Comment()
            {
                Date = DateTime.Now,
                Text = commentText,
                ArticleId = articleId,
                UserId = userId
            };

            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new {articleId = articleId});
        }

        [HttpPost]
        public async Task<IActionResult> EditComment(Guid commentId, Guid articleId)
        {
            var article = await _context.Articles
                .AsNoTracking()
                .Where(a => a.Id.Equals(articleId))
                .SingleOrDefaultAsync();

            var comments = await _context.Comments
                .AsNoTracking()
                .Include(c => c.User)
                .ToListAsync();

            if (article == null)
            {
                return NotFound();
            }

            var commentsModels = ApplicationMapper.CommentsModelsToCommentsList(comments);
            var articleModel = ApplicationMapper.ArticleToArticleModel(article);

            foreach (var commentModel in commentsModels)
            {
                commentModel.IsEditing = commentModel.Id.Equals(commentId);
            }

            var articleWithCommentsModel = new ArticleWithCommentsModel
            {
                ArticleModel = articleModel,
                CommentsModels = commentsModels
            };

            return View("Details", articleWithCommentsModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateComment(Guid articleId, Guid commentId, string commentText = "")
        {
            var comment = await _context.Comments
                .Where(c => c.Id.Equals(commentId))
                .Include(c => c.Article)
                .Include(c => c.User)
                .FirstOrDefaultAsync();

            if (comment == null)
            {
                return NotFound();
            }

            comment.Text = commentText;
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { articleId = articleId });
        }

        [HttpGet]
        public IActionResult CancelEdit(Guid commentId, Guid articleId)
        {
            return RedirectToAction("Details", new { articleId = articleId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteComment(Guid commentId, Guid articleId)
        {
            var comment = await _context.Comments.Where(c => c.Id.Equals(commentId)).FirstOrDefaultAsync();

            if (comment == null)
            {
                return NotFound();
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { articleId = articleId });
        }

        [HttpPost]
        public async Task<IActionResult> Agregate()
        {
            await getArticlesDataTask();
            
            var articlesWithoutText = await _context.Articles
                .AsNoTracking()
                .Where(a => string.IsNullOrEmpty(a.Text))
                .Include(a => a.Source)
                .ToListAsync();

            foreach (var articleWithoutText in articlesWithoutText)
            {
               await getArticlesTextTask(articleWithoutText);
            }

            return View("Index");
        }

        private async Task getArticlesDataTask()
        {
            var sources = await _context.Sources.Where(s => !string.IsNullOrEmpty(s.RssUrl)).ToListAsync();

           
            foreach (var Source in sources)
            {
                if(string.IsNullOrEmpty(Source.RssUrl)){
                    break;
                }

                using (var xmlReader = XmlReader.Create(Source.RssUrl))
                {
                    var syndicationFeed = SyndicationFeed.Load(xmlReader);

                    var articles = syndicationFeed.Items.Select(item =>
                    {
                        var description = item.Summary?.Text;
                        if (!string.IsNullOrEmpty(description))
                        {
                            var imgStartIndex = description.IndexOf("<img");
                            if (imgStartIndex != -1)
                            {
                                var imgEndIndex = description.IndexOf(">", imgStartIndex);
                                if (imgEndIndex != -1)
                                {
                                    var imgTag = description.Substring(imgStartIndex, imgEndIndex - imgStartIndex + 1);
                                    var newImgTag = imgTag.Replace("src=", "style=\"width: 400px; height: 300px; object-fit: cover;\" src=");
                                    description = description.Replace(imgTag, newImgTag);
                                }
                            }
                        }

                        return new Article()
                        {
                            Id = Guid.NewGuid(),
                            Title = item.Title.Text,
                            Description = description,
                            SourceId = Source.Id,
                            OriginalUrl = item.Id
                        };
                    } ).ToList();

                    _context.Articles.AddRange(articles);
                    await _context.SaveChangesAsync();
                }
            }
        }

        private async Task getArticlesTextTask(Article article)
        {
            var web = new HtmlWeb();
            var selector = string.Empty;

            if (article.Source == null)
            {
                return;
            }

            switch (article.Source.Title)
            {
                case "onliner.by":
                {
                    selector = "//div[@class='news-text']";
                } break;

                case "positivnews.ru":
                {
                    selector = "//div[@itemprop='articleBody']";
                } break;

                case "habr.com":
                {
                    selector = "//div[@class='tm-article-body']";
                } break;
            }

            var htmlDocument = web.Load(article.OriginalUrl);
            var articleNode = htmlDocument.DocumentNode.SelectSingleNode(selector);

            if (articleNode != null)
            {

                var innerText = articleNode.InnerText;
                var cleanedText = WebUtility.HtmlDecode(innerText).Trim();

                var existingArticle = await _context.Articles.FirstOrDefaultAsync(a => a.Id == article.Id);
                if (existingArticle != null)
                {
                    existingArticle.Text = cleanedText;
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}
