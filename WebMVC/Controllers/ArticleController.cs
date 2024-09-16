﻿using System.Net;
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
using X.PagedList.Extensions;

namespace Web.MVC.Controllers
{
    public class ArticleController : Controller
    {
        private readonly ApplicationContext _context;
        private readonly ILogger<ArticleController> _logger;

        public ArticleController(ApplicationContext context, ILogger<ArticleController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? page)
        {
            int pageSize = 20;
            int pageNumber = page ?? 1;

            var articles = await _context.Articles
                .AsNoTracking()
                .Include(a => a.Source)
                .ToListAsync()
                .ConfigureAwait(false);

            var models = ApplicationMapper.ArticleListToArticleModelList(articles);
            var pagedArticles = models.ToPagedList(pageNumber, pageSize);
            return View(pagedArticles);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid articleId)
        {
            var article = await _context.Articles
                .AsNoTracking()
                .Include(a => a.Source)
                .FirstOrDefaultAsync(a => a.Id == articleId)
                .ConfigureAwait(false);

            var comments = await _context.Comments
                .AsNoTracking()
                .Where(c => c.ArticleId == articleId)
                .Include(c => c.User)
                .ToListAsync()
                .ConfigureAwait(false);

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
            var article = await _context.Articles
                .FirstOrDefaultAsync(a => a.Id == articleId)
                .ConfigureAwait(false);

            if (article == null)
            {
                return NotFound();
            }

            Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId);

            var comment = new Comment()
            {
                Date = DateTime.Now,
                Text = commentText,
                ArticleId = articleId,
                UserId = userId
            };

            await _context.Comments.AddAsync(comment).ConfigureAwait(false);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            return RedirectToAction("Details", new { articleId });
        }

        [HttpPost]
        public async Task<IActionResult> EditComment(Guid commentId, Guid articleId)
        {
            var article = await _context.Articles
                .AsNoTracking()
                .SingleOrDefaultAsync(a => a.Id == articleId)
                .ConfigureAwait(false);

            if (article == null)
            {
                return NotFound();
            }

            var comments = await _context.Comments
                .AsNoTracking()
                .Include(c => c.User)
                .ToListAsync()
                .ConfigureAwait(false);

            var commentsModels = ApplicationMapper.CommentsModelsToCommentsList(comments);
            var articleModel = ApplicationMapper.ArticleToArticleModel(article);

            foreach (var commentModel in commentsModels)
            {
                commentModel.IsEditing = commentModel.Id == commentId;
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
                .Include(c => c.Article)
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == commentId)
                .ConfigureAwait(false);

            if (comment == null)
            {
                return NotFound();
            }

            comment.Text = commentText;
            await _context.SaveChangesAsync().ConfigureAwait(false);

            return RedirectToAction("Details", new { articleId });
        }

        [HttpGet]
        public IActionResult CancelEdit(Guid commentId, Guid articleId)
        {
            return RedirectToAction("Details", new { articleId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteComment(Guid commentId, Guid articleId)
        {
            var comment = await _context.Comments
                .FirstOrDefaultAsync(c => c.Id == commentId)
                .ConfigureAwait(false);

            if (comment == null)
            {
                return NotFound();
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            return RedirectToAction("Details", new { articleId });
        }

        [HttpPost]
        public async Task<IActionResult> Agregate()
        {
            try
            {
                await getArticlesDataTask().ConfigureAwait(false);

                var articlesWithoutText = await _context.Articles
                    .AsNoTracking()
                    .Where(a => string.IsNullOrEmpty(a.Text))
                    .Include(a => a.Source)
                    .ToListAsync()
                    .ConfigureAwait(false);

                foreach (var articleWithoutText in articlesWithoutText)
                {
                    await getArticlesTextTask(articleWithoutText).ConfigureAwait(false);
                }

                return View("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during aggregation");
                return StatusCode(500, "Internal server error");
            }
        }

        private async Task getArticlesDataTask()
        {
            var sources = await _context.Sources
                .Where(s => !string.IsNullOrEmpty(s.RssUrl))
                .ToListAsync()
                .ConfigureAwait(false);

            foreach (var source in sources)
            {
                if (string.IsNullOrEmpty(source.RssUrl))
                {
                    continue;
                }

                using (var xmlReader = XmlReader.Create(source.RssUrl))
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
                                    var newImgTag = imgTag.Replace("src=", "style=\"max-width: 100%; border-radius: 4px; height: auto; object-fit: contain;\" src=");
                                    description = description.Replace(imgTag, newImgTag);
                                }
                            }
                        }

                        return new Article()
                        {
                            Id = Guid.NewGuid(),
                            Title = item.Title.Text,
                            Description = description,
                            SourceId = source.Id,
                            OriginalUrl = item.Id
                        };
                    }).ToList();

                    _context.Articles.AddRange(articles);
                    await _context.SaveChangesAsync().ConfigureAwait(false);
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
                    selector = "//div[@class='news-text']";
                    break;
                case "positivnews.ru":
                    selector = "//div[@itemprop='articleBody']";
                    break;
                case "habr.com":
                    selector = "//div[@class='tm-article-body']";
                    break;
            }

            var htmlDocument = await web.LoadFromWebAsync(article.OriginalUrl).ConfigureAwait(false);
            var articleNode = htmlDocument.DocumentNode.SelectSingleNode(selector);

            if (articleNode != null)
            {
                var innerText = articleNode.InnerText;
                var cleanedText = WebUtility.HtmlDecode(innerText).Trim();

                var existingArticle = await _context.Articles
                    .FirstOrDefaultAsync(a => a.Id == article.Id)
                    .ConfigureAwait(false);

                if (existingArticle != null)
                {
                    existingArticle.Text = cleanedText;
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                }
            }
        }
    }
}
