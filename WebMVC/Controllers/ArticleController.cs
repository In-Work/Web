using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Data.Entities;
using Web.Mapper;
using Web.Models;

namespace Web.MVC.Controllers
{
    public class ArticleController : Controller
    {
        private readonly ApplicationContext _context;

        public ArticleController(ApplicationContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var articles = await _context.Articles
                .AsNoTracking()
                .Include(a => a.Source)
                .Include(a => a.Comments)
                .ThenInclude(c => c.User)
                .ThenInclude(u => u.Roles)
                .ToListAsync();

            var models = ApplicationMapper.ArticleListToArticleModelList(articles);

            return View(models);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ArticleModel model)
        {
            if (ModelState.IsValid)
            {
                var article = ApplicationMapper.ArticleModelToArticle(model);

                await _context.Articles.AddAsync(article);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            else
            {
                return View(model);
                //return BadRequest(ModelState);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            _context.Articles.AsNoTracking();
            var article = await _context.Articles
                .AsNoTracking()
                .Where(a => a.Id.Equals(id))
                .Include(a => a.Source)
                .Include(a => a.Comments)
                .ThenInclude(c => c.User)
                .ThenInclude(u => u.Roles)
                .SingleOrDefaultAsync();

            if (article != null)
            {
                var model = ApplicationMapper.ArticleToArticleModel(article);
                return View(model);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Guid id)
        {
            var article = await _context.Articles.Where(a => a.Id.Equals(id)).FirstOrDefaultAsync();

            if (article != null)
            {
                _context.Articles.Remove(article);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var article = await _context.Articles.Where(a => a.Id.Equals(id)).FirstOrDefaultAsync();

            if (article != null)
            {
                _context.Articles.Remove(article);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult AddComment(CommentModel model)
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.Email))?.Value;
            var user = _context.Users.FirstOrDefault(u => u.Email.Equals(userEmail));

            if (user!=null)
            {
                model.UserModel = ApplicationMapper.UserModelToUser(user);
                var comment = ApplicationMapper.CommentModelToComment(model);
                _context.Comments.Add(comment);
                _context.SaveChangesAsync();

                return RedirectToAction("Index", "Article");
            }

            return BadRequest();
        }
    }
}
