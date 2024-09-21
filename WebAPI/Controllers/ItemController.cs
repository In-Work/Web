using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Data.Entities;
using Web.DataAccess.CQS.Queries.Items;
using Microsoft.AspNetCore.Mvc;

namespace Web.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly string? _job1;
        private readonly string? _job2;

        public ItemController(IMediator mediator)
        {
            _job1 = BackgroundJob.Enqueue(() => Console.WriteLine("Fire-and-Forget Job (GetItemByCategoryId)"));
            _job2 = BackgroundJob.Schedule(() => Console.WriteLine("Delayed Job (GetItemByCategoryId)"), TimeSpan.FromMilliseconds(5000));
            _mediator = mediator;
        }

        // GET: api/items/{categoryId}
        [HttpGet("{categoryId}")]
        [Authorize]
        public async Task<IActionResult> GetItemByCategoryId(int categoryId)
        {
            var items = await _mediator.Send(new GetItemsByCategoryIdQuery()
            {
                CategoryId = categoryId
            });

            // hangfire https://www.hangfire.io
          
            var jobIdContinueJobWith =  BackgroundJob.ContinueJobWith(_job1, _job2, () => Console.WriteLine("ContinueJobWith! (GetItemByCategoryId)"));


            if (items == null)
            {
                return NotFound();
            }

            return Ok(items);
        }
    }
}
