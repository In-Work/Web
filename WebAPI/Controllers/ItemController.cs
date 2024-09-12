using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.DataAccess.CQS.Queries.Items;
using Web.Data;
using Web.Data.Entities;

namespace Web.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ItemController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: api/items/{categoryId}
        [HttpGet("{categoryId}")]
        public async Task<ActionResult<List<Item>>> GetItemByCategoryId(int categoryId)
        {
            var items = await _mediator.Send(new GetItemsByCategoryIdQuery()
            {
                CategoryId = categoryId
            });

            if (items == null)
            {
                return NotFound();
            }

            return Ok(items);
        }
    }
}
