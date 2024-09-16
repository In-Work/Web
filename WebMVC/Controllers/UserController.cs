using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Web.Data;
using Web.Models;
using Web.Services.Abstractions;

namespace Web.MVC.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationContext _context;
        private readonly IUserService _userService;

        public UserController(ApplicationContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(UserRegisterModel model, CancellationToken token = default)
        {
            if (ModelState.IsValid)
            {
                await _userService.RegisterUserAsync(model, token);
                return RedirectToAction("Login");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(UserLoginModel model, CancellationToken token = default)
        {
            var isEmailRegistered = await _userService.CheckIsEmailRegisteredAsync(model.Email, token);
            var isPasswordCorrect = await _userService.CheckPassword(model.Email, model.Password, token);
            var userId = (await _userService.GetUserIdByEmailAsync(model.Email, token)).ToString();

            if (isEmailRegistered && isPasswordCorrect && userId != null)
            {
                var userRoles = await _userService.GetUserRolesByEmailAsync(model.Email, token);

                var claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim(ClaimTypes.Email, model.Email),
                };

                claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(principal);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Incorrect Email or Password");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}
