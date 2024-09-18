using Web.Data;
using Web.Data.Entities;
using MediatR;

namespace Web.DataAccess.CQS.Commands.Tokens;
public class CreateRefreshTokenCommandHandler : IRequestHandler<CreateRefreshTokenCommand, Guid>
{
    private readonly ApplicationContext _context;

    public CreateRefreshTokenCommandHandler(ApplicationContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateRefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var refreshToken = new RefreshToken()
        {
            Id = Guid.NewGuid(),
            DeviceInfo = request.DeviceInfo,
            UserId = request.UserId,
            ExpireDateTime = DateTime.UtcNow.AddDays(1)
        };

        await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        
        return refreshToken.Id;
    }
}