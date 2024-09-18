using MediatR;

namespace Web.DataAccess.CQS.Queries.User;

public class GetUserIdByEmailQuery  : IRequest<Guid?>
{
    public string Email { get; set; }
}