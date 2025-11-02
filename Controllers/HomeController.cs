using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieHall.Data;
using MovieHall.Models;
using MovieHall.SaveModel;
using MovieHall.ViewModels;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static System.Net.WebRequestMethods;

namespace MovieHall.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        //----------------------------------------- Home -----------------------------------------
        public async Task<IActionResult> Index()
        {
            var AImg = await _context.Settings.FirstOrDefaultAsync(s => s.SettingName == "AnimeImg");
            var MImg = await _context.Settings.FirstOrDefaultAsync(s => s.SettingName == "MovieImg");

            ViewData["AnimeImg"] = "/img/" + AImg.Comment;
            ViewData["MovieImg"] = "/img/" + MImg.Comment;

            return View();
        }
        

        //----------------------------------------- Search -----------------------------------------

        public IActionResult Search()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AutoComplete(string query, string type)
        {
            if (string.IsNullOrWhiteSpace(query)) return Json(new { results = new string[0] });

            var results = Enumerable.Empty<object>();

            if (type == "Movie")
            {
                results = _context.Movies
                    .Where(m => m.Name.Contains(query))
                    .Select(m => new { id = m.Id, name = m.Name, type = "Movie" })
                    .Take(3)
                    .ToList();
            }
            else if (type == "Anime")
            {
                results = _context.Animes
                    .Where(a => a.Name.Contains(query))
                    .Select(a => new { id = a.Id, name = a.Name, type = "Anime" })
                    .Take(3)
                    .ToList();
            }

            return Json(new { results });
        }

        [HttpGet]
        public IActionResult Search(string query, string type)
        {
            ViewBag.Query = query;
            ViewBag.Type = type;

            // könnte Listen-Seite zurückgeben (alle Treffer)
            if (type == "Movie")
            {
                var movies = _context.Movies.Where(m => m.Name.Contains(query)).ToList();
                return View("SearchMovies", movies);
            }
            else if (type == "Anime")
            {
                var animes = _context.Animes.Where(a => a.Name.Contains(query)).ToList();
                return View("SearchAnimes", animes);
            }

            return View("SearchEmpty");
        }


    }
}
