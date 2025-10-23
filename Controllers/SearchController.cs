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

        public async Task<IActionResult> Index(string query, string type)
        {
            var search = new SearchVM()
            {
                Type = type,
                Genres = await _context.Genre.OrderBy(g => g.Name).ToListAsync(),
                SearchString = query
            };

            if (type == "Anime")
            {
                var yearOptions = await _context.Animes
                    .Where(a => a.ReleaseDate != null)
                    .Select(a => a.ReleaseDate.Year)
                    .Distinct()
                    .OrderByDescending(y => y)
                    .ToListAsync();

                var watchedWith = await _context.WatchedWith
                    .Select(m => m.Name)
                    .ToListAsync();

                ViewBag.Years = yearOptions;
                ViewBag.WW = watchedWith;

                search.Animes = await _context.Animes
                .Include(m => m.AnimeGenres).ThenInclude(mg => mg.Genre)
                .Include(m => m.AnimeWatchedWiths).ThenInclude(mw => mw.WatchedWith)
                .Where(m => m.ParentId == null).OrderByDescending(m => m.ReleaseDate)
                .ToListAsync();

            } else if (type == "Movie")
            {
                var yearOptions = await _context.Movies
                    .Where(a => a.ReleaseDate != null)
                    .Select(a => a.ReleaseDate.Year)
                    .Distinct()
                    .OrderByDescending(y => y)
                    .ToListAsync();

                ViewBag.Years = yearOptions;

                search.Movies = await _context.Movies
                .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
                .Include(m => m.MovieWatchedWiths).ThenInclude(mw => mw.WatchedWith)
                .Where(m => m.ParentId == null).OrderByDescending(m => m.ReleaseDate)
                .ToListAsync();
            }

            return View(search);
        }

        [HttpPost]
        public async Task<IActionResult> Index(SearchVM filter)
        {
            var search = filter;
            filter.Genres = await _context.Genre.OrderBy(g => g.Name).ToListAsync();

            if (filter.Type == "Anime")
            {
                var yearOptions = await _context.Animes
                    .Select(a => a.ReleaseDate.Year)
                    .Distinct()
                    .OrderByDescending(y => y)
                    .ToListAsync();

                var watchedWith = await _context.WatchedWith
                    .Select(m => m.Name)
                    .ToListAsync();

                ViewBag.Years = yearOptions;
                ViewBag.WW = watchedWith;


                var query = _context.Animes
                .Include(m => m.AnimeGenres).ThenInclude(mg => mg.Genre)
                .Include(m => m.AnimeWatchedWiths).ThenInclude(mw => mw.WatchedWith)
                .AsQueryable();

                if (!string.IsNullOrWhiteSpace(filter.SearchString))
                {
                    var searchSt = filter.SearchString.ToLower();
                    query = query.Where(a =>
                        a.Name.ToLower().Contains(searchSt) ||
                        (a.Orginal_Name != null && a.Orginal_Name.ToLower().Contains(searchSt))
                    );
                }

                if (filter.FilterGenres != null && filter.FilterGenres.Any())
                {
                    query = query.Where(a => filter.FilterGenres.All(gid => a.AnimeGenres.Any(ag => ag.GenreId == gid)));
                }

                if (filter.FilterParents)
                {
                    query = query.Where(a => a.ParentId == null);
                }

                if (!string.IsNullOrWhiteSpace(filter.FilterContry))
                {
                    query = query.Where(a => a.Country == filter.FilterContry);
                }

                if (!string.IsNullOrWhiteSpace(filter.FilterWatchWitch))
                {
                    query = query.Where(a => a.AnimeWatchedWiths.Any(w => w.WatchedWith.Name == filter.FilterWatchWitch));
                }

                search.Animes = await query
                    .OrderByDescending(a => a.ReleaseDate)
                    .ToListAsync();

            }
            else if (filter.Type == "Movie")
            {
                var yearOptions = await _context.Movies
                    .Select(a => a.ReleaseDate.Year)
                    .Distinct()
                    .OrderByDescending(y => y)
                    .ToListAsync();

                ViewBag.Years = yearOptions;

                var query = _context.Movies
                .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
                .Include(m => m.MovieWatchedWiths).ThenInclude(mw => mw.WatchedWith)
                .AsQueryable();

                if (!string.IsNullOrWhiteSpace(filter.SearchString))
                {
                    var searchSt = filter.SearchString.ToLower();
                    query = query.Where(a => a.Name.ToLower().Contains(searchSt));
                }

                if (filter.FilterGenres != null && filter.FilterGenres.Any())
                {
                    query = query.Where(a => filter.FilterGenres.All(gid => a.MovieGenres.Any(ag => ag.GenreId == gid)));
                }

                if (filter.FilterParents)
                {
                    query = query.Where(a => a.ParentId == null);
                }

                if (filter.FilterFSK.HasValue)
                {
                    query = query.Where(a => a.FSK == filter.FilterFSK.Value);
                }

                search.Movies = await query
                    .OrderByDescending(a => a.ReleaseDate)
                    .ToListAsync();
            }


            return View(search);
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
