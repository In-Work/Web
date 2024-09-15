using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Web.Data.Entities;
using Web.Data;
using Web.Models;

namespace Web.MVC.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationContext _context;
        public UserController(ApplicationContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(UserModel model, CancellationToken token = default)
        {
            var password = "123";
            var email = "test";

            var userId = _context.Users.FirstOrDefault(u => u.Email.Equals(email))?.Id.ToString();

            if (model.Password.Equals(password) && model.Email.Equals(email) && userId != null)
            {
                var userRole = "Admin";

                var claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim(ClaimTypes.Name, model.Name),
                    new Claim(ClaimTypes.Email, model.Email),
                    new Claim(ClaimTypes.Role, userRole)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(principal);
                return RedirectToAction("Index", "Article");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(UserModel model, CancellationToken token = default)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Name = model.Name,
                    Email = model.Email,
                    PasswordHash = model.Password 
                    // В реальном приложении пароль должен быть захеширован
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync(token);

                return RedirectToAction("Login");
            }

            return View(model);
        }
    }
}
