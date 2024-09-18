using Web.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Web.DataAccess.CQS.Queries.User;

public class GetUserIdByEmailQueryHandler : IRequestHandler<GetUserIdByEmailQuery, Guid?>
{
    private readonly ApplicationContext _context;

    public GetUserIdByEmailQueryHandler(ApplicationContext context)
    {
        _context = context;
    }

    public async Task<Guid?> Handle(GetUserIdByEmailQuery request, CancellationToken cancellationToken)
    {
        return (await _context.Users.SingleOrDefaultAsync(user => user.Email.Equals(request.Email), cancellationToken))?
            .Id;
    }
}