using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Web.Services.Abstractions;

namespace Web.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly IArticleService _articleService;
        public JobsController(IArticleService articleService)
        {
            _articleService = articleService;
        }

        [HttpGet]
        public async Task<IActionResult> AgregateAtricle(CancellationToken token = default)
        {
            RecurringJob.AddOrUpdate("myrecurringjob", () => _articleService.AggregateArticleAsync(token), "0 2 * * *");
            return Ok();
        }
    }
}
