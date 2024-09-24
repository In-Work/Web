using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Data.Entities;
using Web.Mapper;
using Web.Services.Abstractions;

namespace Web.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticleController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly IArticleService _articleService;

        public ArticleController(ApplicationContext context, IArticleService articleService)
        {
            _context = context;
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

        [HttpPut("{id}")]
        public async Task<IActionResult> PutArticle(Guid id, Article article)
        {
            if (id != article.Id)
            {
                return BadRequest();
            }

            _context.Entry(article).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ArticleExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArticle(Guid id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ArticleExists(Guid id)
        {
            return _context.Articles.Any(e => e.Id == id);
        }
    }
}
