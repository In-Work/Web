using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Web.Data;
using Web.Mapper;
using Web.Models;
using Web.Services.Abstractions;
using X.PagedList.Extensions;

namespace Web.MVC.Controllers
{
    public class ArticleController : Controller
    {
        private readonly IArticleService _articleService;
        private readonly IUserService _userService;
        private readonly ICommentService _commentService;
        private readonly ILogger<ArticleController> _logger;
        private readonly IWebHostEnvironment _env;

        public ArticleController(ApplicationContext context, 
            ILogger<ArticleController> logger, 
            IArticleService articleService, 
            ICommentService commentService,
            IWebHostEnvironment env,
            IUserService userService)
        {
            _articleService = articleService;
            _commentService = commentService;
            _logger = logger;
            _env = env;
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? page, CancellationToken token = default)
        {
            _logger.LogInformation("Index action called with page: {Page}", page);
            var pageSize = 20;
            var pageNumber = page ?? 1;

            var articles = await _articleService.GetArticlesAsync(token);
            var models = ApplicationMapper.ArticleListToArticleModelList(articles);
            var pagedArticles = models.ToPagedList(pageNumber, pageSize);

            return View(pagedArticles);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> UserArticles(int? page, CancellationToken token = default)
        {
            _logger.LogInformation("Index action called with page: {Page}", page);
            var pageSize = 20;
            var pageNumber = page ?? 1;
            
            var articles = await _articleService.GetArticlesAsync(token);
            var userEmail = @User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var minRank = (await _userService.GetUserByEmailAsync(userEmail, token)).MinRank;
            var rangedArtickes = articles.Where(a => a.Rate >= minRank).ToList();
            var models = ApplicationMapper.ArticleListToArticleModelList(rangedArtickes);
            var pagedArticles = models.ToPagedList(pageNumber, pageSize);

            return View(pagedArticles);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid articleId, CancellationToken token = default)
        {
            _logger.LogInformation("Details action called with articleId: {ArticleId}", articleId);
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

            _logger.LogWarning("Article not found with articleId: {ArticleId}", articleId);
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(Guid articleId, string commentText, CancellationToken token = default)
        {
            _logger.LogInformation("AddComment action called with articleId: {ArticleId}, commentText: {CommentText}", articleId, commentText);
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid.TryParse(userIdString, out var userId);
            await _commentService.AddCommentByArticleIdAsync(articleId, userId, commentText, token);
            return RedirectToAction("Details", new { articleId });
        }

        [HttpPost]
        public async Task<IActionResult> EditComment(Guid commentId, CancellationToken token = default)
        {
            _logger.LogInformation("EditComment action called with commentId: {CommentId}", commentId);
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
            _logger.LogInformation("UpdateComment action called with articleId: {ArticleId}, commentId: {CommentId}, commentText: {CommentText}", articleId, commentId, commentText);
            try
            {
                await _articleService.UpdateCommentById(commentId, commentText, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating comment with commentId: {CommentId}", commentId);
                return StatusCode(500, "Internal server error");
            }

            return RedirectToAction("Details", new { articleId });
        }

        [HttpPost]
        public IActionResult CancelEdit(Guid articleId)
        {
            _logger.LogInformation("CancelEdit action called with articleId: {ArticleId}", articleId);
            return RedirectToAction("Details", new { articleId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteComment(Guid commentId, Guid articleId, CancellationToken token = default)
        {
            _logger.LogInformation("DeleteComment action called with commentId: {CommentId}, articleId: {ArticleId}", commentId, articleId);
            await _commentService.DeleteCommentById(commentId, token);
            return RedirectToAction("Details", new { articleId });
        }

        [HttpPost]
        public async Task<IActionResult> AgregateArticle(CancellationToken token = default)
        {
            _logger.LogInformation("AgregateArticle action called");
            await _articleService.AggregateArticleAsync(token);
            return RedirectToAction("UserArticles", "Article");
        }

        [HttpPost]
        public async Task<IActionResult> PositivityAssessment(CancellationToken token = default)
        {
            var filePath = Path.Combine(_env.WebRootPath, "res", "AFINN-ru.json");
            string json = await System.IO.File.ReadAllTextAsync(filePath);
            Dictionary<string, int> afinnData = JsonConvert.DeserializeObject<Dictionary<string, int>>(json);

            await _articleService.PositivityAssessmentAsync(afinnData, token);
            return RedirectToAction("UserArticles", "Article");
        }
    }
}
