using MediatR;
using Web.DTOs;

namespace Web.DataAccess.CQS.Queries.User;

public class GetUserDataByRefreshTokenQuery : IRequest<UserTokenDto?>
{
    public Guid ToklenId { get; set; }
}