using MediatR;
using Web.Data.Entities;

namespace Web.DataAccess.CQS.Queries.Tokens;

public class GetRefreshTokenByIdQuery : IRequest<RefreshToken?>
{
    public Guid Id { get; set; }
}