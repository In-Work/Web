using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Web.Mapper;
using Web.Models;
using Web.Services.Abstractions;

namespace WebMVC.Controllers
{
    public class UserSettingsController : Controller
    {
        private readonly IUserService _userService;

        public UserSettingsController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken token = default)
        {
            var userEmail = @User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value!;
            var user = await _userService.GetUserByEmailAsync(userEmail, token);
            var model = ApplicationMapper.UserToUserSettingsModel(user);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeMinRank(UserSettingsModel model, CancellationToken token = default)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(nameof(model.MinRank), "error");
                return RedirectToAction("Index");
            }
            else
            {
                var userEmail = @User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value!;
                await _userService.ChangeMinRankAsync(userEmail, model.MinRank, token);
            }

            return RedirectToAction("Index", "Article");
        }
    }
}