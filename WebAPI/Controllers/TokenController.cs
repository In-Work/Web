using Microsoft.AspNetCore.Mvc;
using Web.Data.Migrations;
using Web.Models;
using Web.Services.Abstractions;

namespace Web.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;

        public TokenController(IConfiguration configuration, IUserService userService, ITokenService tokenService)
        {
            _userService = userService;
            _tokenService = tokenService;
        }

        [HttpPost("/login")]
        public async Task<IActionResult> Login([FromBody] UserLoginModel model,
            CancellationToken cancellationToken = default)
        {
            var isEmailRegistered = await _userService.CheckIsEmailRegisteredAsync(model.Email, cancellationToken);
            var isPasswordCorrect = await _userService.CheckPasswordAsync(model.Email, model.Password, cancellationToken);

            if (!isEmailRegistered || !isPasswordCorrect)
            {
                return Unauthorized();
            }

            var userRoles = await _userService.GetUserRolesByEmailAsync(model.Email, cancellationToken);
            var userId = await _userService.GetUserIdByEmailAsync(model.Email, cancellationToken);

            if (userRoles==null || !userId.HasValue)
            {
                return StatusCode(500);
            }

            var (accessToken, refreshToken) = await _tokenService.GenerateTokenPairByUserIdAsync(userId.Value, userRoles, cancellationToken);
            return Ok(new { AccessToken = accessToken, RefreshToken = refreshToken });
        }
    }
}
