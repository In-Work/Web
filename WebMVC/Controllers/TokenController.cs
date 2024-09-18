using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Web.Models;
using Web.Services.Abstractions;

namespace Web.MVC.Controllers
{
    public class TokenController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;

        public TokenController(IConfiguration configuration, IUserService userService, ITokenService tokenService)
        {
            _configuration = configuration;
            _userService = userService;
            _tokenService = tokenService;
        }

        [HttpPost]
        public async Task<IActionResult> RefreshToken(Guid refreshTokenId, CancellationToken token = default)
        {
            var isRefreshTokenCorrect = await _tokenService.CheckIsRefreshTokenCorrectByIdAsync(refreshTokenId, token);

            if (isRefreshTokenCorrect)
            {
                var user = await _userService.GetUserDataByRefreshTokenIdAsync(refreshTokenId, token);

                if (user != null)
                {
                    var (accessToken, refreshToken) = await GenerateTokenPairByUserIdAsync(user.Id, user.RoleNames, token);

                    //await _tokenService.RemoveToken(refreshTokenId, token); //todo need to be implemented

                    var result = new
                    {
                        AccessToken = accessToken,
                        RefreshToken = refreshToken
                    };
                }
            }

            return NotFound();
        }


        [HttpGet]
        public async Task<IActionResult> LoginToken()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoginToken(UserLoginModel model, CancellationToken token = default)
        {
            var isEmailRegistered = await _userService.CheckIsEmailRegisteredAsync(model.Email, token);
            var isPasswordCorrect = await _userService.CheckPasswordAsync(model.Email, model.Password, token);

            if (!isEmailRegistered || !isPasswordCorrect)
            {
                ModelState.AddModelError("", "Incorrect email or password!");
                return View(model);
            }

            var userRoles = await _userService.GetUserRolesByEmailAsync(model.Email, token);
            var userId = await _userService.GetUserIdByEmailAsync(model.Email, token);

            if (userRoles.IsNullOrEmpty() || !userId.HasValue)
            {
                ModelState.AddModelError("", "Incorrect email or password!");
                return View(model);
            }

            var (accessToken, refreshToken) = await GenerateTokenPairByUserIdAsync(userId.Value, userRoles, token);

            var result = new
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
            
            return RedirectToAction("Index", "Article");
        }

        //[HttpPost]
        //public async Task<IActionResult> Revoke(Guid id,
        //    CancellationToken cancellationToken = default)
        //{
        //    //set IsRevoked true for refreshToken 
        //    return NotFound();
        //}

        [HttpPost]
        private async Task<(string?, string?)> GenerateTokenPairByUserIdAsync(Guid userId, List<string> userRoles, CancellationToken token = default)
        {
            var accessToken = await _tokenService.GenerateJwtTokenStringAsync(userId, userRoles, token);
            var deviceInfo = "localhost";
            var refreshToken = await _tokenService.GenerateRefreshTokenAsync(userId, deviceInfo, token);

            return (accessToken, refreshToken);
        }
    }
}