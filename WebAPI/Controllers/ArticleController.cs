using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Mapper;
using Web.Services.Abstractions;

namespace Web.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticleController : ControllerBase
    {
        private readonly IArticleService _articleService;

        public ArticleController(IArticleService articleService)
        {
            _articleService = articleService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get(CancellationToken token)
        {
            var data = await _articleService.GetArticlesAsync(token);

            var articleDto = ApplicationMapper.ArticlesToArticlesDto(data);
            return Ok(articleDto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id, CancellationToken token = default)
        {
            var article = await _articleService.GetArticleByIdAsync(id, token);

            var articleDto = ApplicationMapper.ArticleToArticleDto(article);
            return Ok(articleDto);
        }
    }
}
