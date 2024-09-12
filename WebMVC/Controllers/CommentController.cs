using Microsoft.AspNetCore.Mvc;
using Web.Data;
using Web.Mapper;
using Web.Models;

namespace Web.MVC.Controllers
{
    public class CommentController : Controller
    {
        private readonly ApplicationContext _context;

        public CommentController(ApplicationContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult Add(CommentModel model)
        {
            var comment = ApplicationMapper.CommentModelToComment(model);
            _context.Comments.Add(comment);
            return RedirectToAction("Index", "Article");
        }
    }
}
