using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MediatR;
using Web.DataAccess.CQS.Commands.Tokens;
using Web.DataAccess.CQS.Queries.Tokens;
using Web.DataAccess.CQS.Queries.User;
using Web.Services.Abstractions;
using System.Net.Http;

namespace Web.Services.Implementations
{
    public class TokenService : ITokenService
    {
        private readonly IMediator _mediator;
        private readonly IConfiguration _configuration;

        public TokenService(IMediator mediator, IConfiguration configuration)
        {
            _mediator = mediator;
            _configuration = configuration;
        }

        public async Task<string?> GenerateJwtTokenStringAsync(Guid userId, List<string> userRoles, CancellationToken cancellationtoken = default)
        {
            var userEmail = await _mediator.Send(new GetUserEmailByIdQuery(){ Id = userId }, cancellationtoken);

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, userEmail)
            };

            claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));
            
            var accessTokenString = await CreateAccessTokenStringAsync(claims, cancellationtoken);
            return accessTokenString;
        }

        public async Task<string?> CreateAccessTokenStringAsync(List<Claim> claims, CancellationToken token = default)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["jwtToken:Secret"]));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _configuration["jwtToken:Issuer"],
                Audience = _configuration["jwtToken:Audience"],
                Expires = DateTime.UtcNow.AddMinutes(15),
                Subject = new ClaimsIdentity(claims),
                SigningCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature)
            };

            var jwtToken = jwtHandler.CreateToken(tokenDescriptor);
            var tokenString = jwtHandler.WriteToken(jwtToken);
            return tokenString;
        }

        public async Task<string?> GenerateRefreshTokenAsync(Guid userId, string deviceInfo, CancellationToken token = default)
        {
            var refreshToken = await _mediator.Send(new CreateRefreshTokenCommand()
            {
                UserId = userId,
                DeviceInfo = deviceInfo
            });

            return refreshToken.ToString("D");
        }

        public async Task<bool> CheckIsRefreshTokenCorrectByIdAsync(Guid tokenId, CancellationToken cancellationToken = default)
        {
            var rToken = await _mediator.Send(new GetRefreshTokenByIdQuery() { Id = tokenId }, cancellationToken);
            return rToken
                is { IsRevoked: false }
                   && (rToken.ExpireDateTime <= DateTime.UtcNow || rToken.ExpireDateTime == null);
        }
    }
}
