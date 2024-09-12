using MediatR;
using Microsoft.EntityFrameworkCore;
using Web.Specification;
using Web.Data;
using Web.Data.Entities;

namespace Web.DataAccess.CQS.Queries.Items
{
    public class GetItemsByCategoryIdQueryHandler : IRequestHandler<GetItemsByCategoryIdQuery, List<Item>?>
    {
        private readonly ApplicationContext _context;

        public GetItemsByCategoryIdQueryHandler(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<List<Item>?> Handle(GetItemsByCategoryIdQuery request,
            CancellationToken cancellationToken)
        {
            ISpecification<Item> spec = new ItemByCategorySpecification(request.CategoryId);
            var items = await _context.Items
                .AsNoTracking()
                .Where(spec.ItemsCriteria)
                .ToListAsync(cancellationToken);

            return items;
        }
    }
}
