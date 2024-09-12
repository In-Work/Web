using Microsoft.AspNetCore.Mvc;

namespace Web.MVC.Controllers
{
    public class ArticleDetailsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
