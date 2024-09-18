using Web.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Web.DataAccess.CQS.Queries.User;

public class GetUserEmailByIdQueryHandler : IRequestHandler<GetUserEmailByIdQuery, string>
{
    private readonly ApplicationContext _context;

    public GetUserEmailByIdQueryHandler(ApplicationContext context)
    {
        _context = context;
    }

    public async Task<string> Handle(GetUserEmailByIdQuery request, CancellationToken cancellationToken)
    {
        return (await _context.Users.SingleOrDefaultAsync(user => user.Id.Equals(request.Id), cancellationToken))?
            .Email;
    }
}