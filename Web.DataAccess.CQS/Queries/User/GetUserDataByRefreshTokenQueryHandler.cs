using Web.Data;
using Web.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Web.DataAccess.CQS.Queries.User;

public class GetUserDataByRefreshTokenQueryHandler : IRequestHandler<GetUserDataByRefreshTokenQuery, UserTokenDto?>
{
    private readonly ApplicationContext _context;

    public GetUserDataByRefreshTokenQueryHandler(ApplicationContext context)
    {
        _context = context;
    }

    public async Task<UserTokenDto?> Handle(GetUserDataByRefreshTokenQuery request, CancellationToken cancellationToken)
    {
        var userId = (await _context.RefreshTokens
            .AsNoTracking()
            .SingleOrDefaultAsync(refreshToken => 
                refreshToken.Id.Equals(request.ToklenId),
                cancellationToken))?.UserId;

        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
            .SingleOrDefaultAsync(u => u.Id.Equals(userId), cancellationToken);

        if (user != null)
        {
            return new UserTokenDto()
            {
                Id = userId.Value,
                Email = user.Email,
                RoleNames = user.UserRoles.Select(ur => ur.RoleName).ToList(),
                RefreshToken = request.ToklenId
            };
        }

        return null;
    }
}