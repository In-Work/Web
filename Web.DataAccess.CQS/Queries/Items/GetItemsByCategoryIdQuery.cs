using MediatR;
using Web.Data.Entities;

namespace Web.DataAccess.CQS.Queries.Items
{
    public class GetItemsByCategoryIdQuery :IRequest<List<Item>?>
    {
        public int CategoryId { get; set; }
    }
}
