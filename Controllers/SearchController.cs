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

        public IActionResult Index()
        {
            var search = new Search
            {
                search = ""
            };
            return View();
        }
    }
}
