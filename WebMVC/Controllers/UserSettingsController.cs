using Microsoft.AspNetCore.Mvc;
using Web.Models;

namespace WebMVC.Controllers
{
    public class UserSettingsController : Controller
    {
        public IActionResult Index()
        {
            var model = new UserSettingsModel
            {
                UserName = "CurrentUserName", 
                Password = "", 
                Email = "user@example.com" 
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Save(UserSettingsModel model)
        {
            if (ModelState.IsValid)
            {
                return RedirectToAction("Index");
            }
            return View("Index", model);
        }
    }
}