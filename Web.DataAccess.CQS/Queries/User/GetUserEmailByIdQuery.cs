using MediatR;

namespace Web.DataAccess.CQS.Queries.User;

public class GetUserEmailByIdQuery : IRequest<string>
{
    public Guid Id { get; set; }
}