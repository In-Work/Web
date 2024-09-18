using Web.DTOs;
using MediatR;

namespace Web.DataAccess.CQS.Commands.Articles
{
    public class InsertUniqueArticlesFromRssDataCommand : IRequest
    {
        public List<ArticleDto>? Articles { get; set; }
    }
}
