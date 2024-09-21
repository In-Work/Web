using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Web.Models;
using Web.Services.Abstractions;

namespace Web.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> Register(UserRegisterModel model, CancellationToken token)
        {
            var isEmailRegistered = await _userService.CheckIsEmailRegisteredAsync(model.Email, token);

            if (isEmailRegistered)
            {
                return Conflict(new { error = "EmailAlreadyExists", message = "This email is already registered." });
            }

            await _userService.RegisterUserAsync(model, token);
            return Ok();
        }
    }
}
