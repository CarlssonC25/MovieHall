using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieHall.Data;
using MovieHall.ViewModels;

namespace MovieHall.Controllers
{
    public class SearchController : Controller
    {
        private readonly AppDbContext _context;
        public SearchController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string query = "", string type = "")
        {
            ViewBag.Query = query;
            ViewBag.Type = type;
            return View();
        }

        [HttpGet]
        public IActionResult AutoCompleteAnime(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Json(new { results = new object[0] });

            var results = _context.Animes
                .Where(a => EF.Functions.Like(a.Name, $"%{query}%"))
                .Select(a => new { id = a.Id, name = a.Name, img = a.Img })
                .Take(3)
                .ToList();

            return Json(new { results });
        }

        [HttpGet]
        public IActionResult AutoCompleteMovie(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Json(new { results = new object[0] });

            var results = _context.Movies
                .Where(m => EF.Functions.Like(m.Name, $"%{query}%"))
                .Select(m => new { id = m.Id, name = m.Name, img = m.Img })
                .Take(3)
                .ToList();

            return Json(new { results });
        }

    }
}
