using MediatR;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Data.Entities;

namespace Web.DataAccess.CQS.Queries.Tokens;

public class GetRefreshTokenByIdQueryHandler : IRequestHandler<GetRefreshTokenByIdQuery, RefreshToken?>
{
    private readonly ApplicationContext _context;

    public GetRefreshTokenByIdQueryHandler(ApplicationContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> Handle(GetRefreshTokenByIdQuery request, CancellationToken token = default)
    {
        return await _context.RefreshTokens
            .SingleOrDefaultAsync(refreshToken => refreshToken.Id.Equals(request.Id), token);
    }
}