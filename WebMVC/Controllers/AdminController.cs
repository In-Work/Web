using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Mapper;
using Web.Models;
using Web.Services.Abstractions;
using Web.Services.Implementations;

namespace Web.MVC.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly IUserService _userService;

        public AdminController(IAdminService adminService, IUserService userService)
        {
            _userService = userService;
            _adminService = adminService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(CancellationToken token = default)
        {
            var users = await _userService.GetAllUsersAsync(token);
            var userRolesModel = ApplicationMapper.UsersToUserRolesModel(users);
            return View(userRolesModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(Guid userId, CancellationToken token = default)
        {
            await _userService.RemoveUserByUserIdAsync(userId, token);
            return RedirectToAction("Index");
        }
        
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddAdminRole(Guid userId, CancellationToken token = default)
        {
            await _userService.AddAdminRoleByUserIdAsync(userId, token);
            return RedirectToAction("Index");
        }
    }
}
