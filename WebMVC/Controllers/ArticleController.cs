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
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

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

        [HttpGet]
        public async Task<IActionResult> RecurringJobInit()
        {
            const string data =
                "Цены на авиабилеты редко приводят в восторг, но это как раз такой случай: авиабилеты первого класса туда и обратно из Австралии в США можно было купить со скидкой 85%. Обычно эти билеты стоят у авиакомпании Qantas около 19 000 долларов. Из-за сбоя около 300 счастливчиков смогли купить их на сайте авиакомпании за 3400 долларов, пока ошибку не исправили.«К сожалению, это тот случай, когда тариф был слишком хорош, чтобы быть правдой», — заявили в авиакомпании.Тем не менее Qantas не аннулировала проданные по ошибке билеты, а пообещала перебронировать их в бизнес-класс «в качестве жеста доброй воли» без дополнительной платы. Кроме того, пассажиры, которых не устраивает бизнес-класс, могут получить полный возврат денег.Перелет бизнес-классом на рейсах Qantas между Австралией и США обычно стоит около 11 000 долларов, уточняет CNN.Это не первый случай, когда авиакомпании по ошибке продавали билеты по вопиюще низким ценам. И иногда они действительно перевозили пассажиров с такими билетами.Но бывает и наоборот. Например, авиакомпания British Airways ошибочно продавала за 40 долларов билеты из Северной Америки в Индию, а когда ошибка была обнаружена, то предложила вместо этого ваучеры на 300 долларов.Авиакомпания American Airlines отказалась предоставить места первого класса из США в Австралию стоимостью до 20 000 долларов, которые она продавала по цене эконом-класса — 1100 долларов. Вместо этого она предложила ваучеры на 200 долларов.";
            var key = "b38193c09b66ce4375bfa816f664201d31c42d50";

            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Post,
                    $"http://api.ispras.ru/texterra/v1/nlp?targetType=lemma&apikey={key}");
                
                request.Headers.Add("Accept", "application/json");
                request.Content = JsonContent.Create(new[]
                {
                    new { Text = data }
                });
                
                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    // Process the response as needed
                    return Ok(responseString);
                }
                else
                {
                    return StatusCode(500);
                }
            }
        }
    }
}
