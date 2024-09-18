using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.ServiceModel.Syndication;
using System.Xml;
using Web.Data;
using Web.Data.Entities;
using Web.Mapper;
using Web.Models;
using Web.Services.Abstractions;
using X.PagedList.Extensions;

namespace Web.MVC.Controllers
{
    public class ArticleController : Controller
    {
        private readonly ApplicationContext _context;
        private readonly IArticleService _articleService;
        private readonly ICommentService _commentService;
        private readonly ILogger<ArticleController> _logger;

        public ArticleController(ApplicationContext context, 
            ILogger<ArticleController> logger, 
            IArticleService articleService, 
            ICommentService commentService)
        {
            _context = context;
            _logger = logger;
            _articleService = articleService;
            _commentService = commentService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? page, CancellationToken token = default)
        {
            var pageSize = 20;
            var pageNumber = page ?? 1;

            var articles = await _articleService.GetArticlesAsync(token);
            var models = ApplicationMapper.ArticleListToArticleModelList(articles);
            var pagedArticles = models.ToPagedList(pageNumber, pageSize);

            return View(pagedArticles);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid articleId, CancellationToken token = default)
        {
            var article = await _articleService.GetArticleByIdAsync(articleId, token);

            if (article != null)
            {
                var articleWithCommentsModels = new ArticleWithCommentsModel()
                {
                    ArticleModel = ApplicationMapper.ArticleToArticleModel(article),
                    CommentsModels = ApplicationMapper.CommentsModelsToCommentsList(article.Comments)
                };

                return View(articleWithCommentsModels);
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(Guid articleId, string commentText, CancellationToken token = default)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid.TryParse(userIdString, out var userId);
            await _commentService.AddCommentByArticleIdAsync(articleId, userId, commentText, token);
            return RedirectToAction("Details", new { articleId });
        }

        [HttpPost]
        public async Task<IActionResult> EditComment(Guid commentId, CancellationToken token = default)
        {
            var comment = await _commentService.GetCommentById(commentId, token);
            var article = await _articleService.GetArticleByIdAsync(comment!.ArticleId, token);
            var comments = await _commentService.GetCommentsByArticleId(comment.ArticleId, token);

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
        public async Task<IActionResult> UpdateComment(Guid articleId, Guid commentId, string commentText, CancellationToken token = default)
        {
            await _articleService.UpdateCommentById(commentId, commentText, token);
            return RedirectToAction("Details", new { articleId });
        }

        [HttpPost]
        public IActionResult CancelEdit(Guid articleId)
        {
            return RedirectToAction("Details", new { articleId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteComment(Guid commentId, Guid articleId, CancellationToken token = default)
        {
            await _commentService.DeleteCommentById(commentId, token);
            return RedirectToAction("Details", new { articleId });
        }

        [HttpPost]
        public async Task<IActionResult> AgregateArticle(CancellationToken token = default)
        {
            await _articleService.AggregateArticleAsync(token);
            return Ok();
        }
    }
}
