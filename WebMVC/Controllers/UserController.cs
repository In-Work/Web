using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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
            if (await _userService.CheckIsEmailRegisteredAsync(model.Email, token))
            {
                ModelState.AddModelError(nameof(model.Email), "Email has been registered already");
                return View();
            }
            
            if (ModelState.IsValid)
            {
                await _userService.RegisterUserAsync(model, token);
                return RedirectToAction("Login");
            }
            else
            {
                return View();
            }
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
            var isPasswordCorrect = await _userService.CheckPasswordAsync(model.Email, model.Password, token);
            var userId = (await _userService.GetUserIdByEmailAsync(model.Email, token)).ToString();

            if (!isEmailRegistered)
            {
                ModelState.AddModelError(nameof(model.Email), "Incorrect Email or UserName");
                return View();
            }

            if (!isPasswordCorrect)
            {
                ModelState.AddModelError(nameof(model.Password), "Incorrect Password");
                return View();
            }

            if (isEmailRegistered && isPasswordCorrect && userId != null)
            {
                var userRoles = await _userService.GetUserRolesByEmailAsync(model.Email, token);
                //TODO: checking user login with multiple roles

                var claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim(ClaimTypes.Email, model.Email),
                };

                claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(principal);

                return RedirectToAction("Index", "Article");
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

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model, CancellationToken token = default)
        {
            var isEmailCorrect = await _userService.CheckIsEmailRegisteredAsync(model.Email, token);

            if (!isEmailCorrect)
            {
                ModelState.AddModelError(nameof(model.Email), "Incorrect email adress!");
                return View();
            }

            if (ModelState.IsValid)
            {  
                return View("ForgotPasswordConfirmation");
            }

            //TODO email comform
            ModelState.AddModelError(nameof(model.Email), "Incorrect email adress!");
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model, CancellationToken token)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var isEmailRegistered = await _userService.CheckIsEmailRegisteredAsync(model.Email, token);

            if (isEmailRegistered)
            {
                await _userService.ResetPasswordAsync(model.Email, model.Password, token);
                return View("ResetPasswordConfirmation");
            }

            return View(model);
        }
    }
}
