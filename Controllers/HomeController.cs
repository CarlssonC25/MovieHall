using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieHall.Data;
using MovieHall.Models;
using MovieHall.ViewModels;
using System.Diagnostics;

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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        //----------------------------------------- Anime -----------------------------------------


        public IActionResult Anime()
        {
            return View();
        }


        //----------------------------------------- Movie -----------------------------------------
        public async Task<IActionResult> Movie()
        {
            var movies = await _context.Movies
                .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
                .Include(m => m.MovieWatchedWiths).ThenInclude(mw => mw.WatchedWith)
                .Where(m => m.ParentId == null)
                .ToListAsync();

            return View(movies);
        }

        // ---------- ADD ----------
        [HttpGet]
        public async Task<IActionResult> CreateMoviePartial()
        {
            ViewBag.Genres = await _context.Genre.OrderBy(g => g.Name).ToListAsync();
            ViewBag.WatchedWith = await _context.WatchedWith.OrderBy(w => w.Name).ToListAsync();
            return PartialView("~/Views/Movie/_CreatePartial.cshtml", new Movie());
        }

        [HttpPost]
        public async Task<IActionResult> CreateMoviePartial(Movie movie, int[] selectedGenres, int[] selectedWatchedWith)
        {
            if (!ModelState.IsValid) return PartialView("~/Views/Movie/_CreatePartial.cshtml", movie);

            foreach (var genreId in selectedGenres)
                movie.MovieGenres.Add(new MovieGenre { GenreId = genreId });

            foreach (var wId in selectedWatchedWith)
                movie.MovieWatchedWiths.Add(new MovieWatchedWith { WatchedWithId = wId });

            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // ---------- EDIT ----------
        [HttpGet]
        public async Task<IActionResult> EditMoviePartial(int id)
        {
            var movie = await _context.Movies
                .Include(m => m.MovieGenres)
                .Include(m => m.MovieWatchedWiths)
                .FirstOrDefaultAsync(m => m.Id == id);

            var genres = await _context.Genre.OrderBy(g => g.Name).ToListAsync();
            var watchedWith = await _context.WatchedWith.OrderBy(w => w.Name).ToListAsync();

            ViewBag.Genres = genres;
            ViewBag.WatchedWith = watchedWith;

            return PartialView("~/Views/Movie/_EditPartial.cshtml", movie);
        }

        [HttpPost]
        public async Task<IActionResult> Movie(Movie movie, int[] selectedGenres, int[] selectedWatchedWith)
        {
            var dbMovie = await _context.Movies
                .Include(m => m.MovieGenres)
                .Include(m => m.MovieWatchedWiths)
                .FirstOrDefaultAsync(m => m.Id == movie.Id);

            if (dbMovie == null)
                return NotFound();

            // Update properties
            dbMovie.Name = movie.Name;
            dbMovie.Description = movie.Description;
            dbMovie.Language = movie.Language;
            dbMovie.Link = movie.Link;
            dbMovie.Img = movie.Img;
            dbMovie.FSK = movie.FSK;
            dbMovie.Buy = movie.Buy;
            dbMovie.Favorit = movie.Favorit;
            dbMovie.ReleaseDate = movie.ReleaseDate;
            dbMovie.ParentId = movie.ParentId;

            // Update genres
            dbMovie.MovieGenres.Clear();
            foreach (var genreId in selectedGenres)
                dbMovie.MovieGenres.Add(new MovieGenre { MovieId = dbMovie.Id, GenreId = genreId });

            // Update watched with
            dbMovie.MovieWatchedWiths.Clear();
            foreach (var wId in selectedWatchedWith)
                dbMovie.MovieWatchedWiths.Add(new MovieWatchedWith { MovieId = dbMovie.Id, WatchedWithId = wId });

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // ---------- DELETE ----------
        [HttpPost]
        public async Task<IActionResult> DeleteMovieConfirmed(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null) return NotFound();

            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }


        //----------------------------------------- Settings -----------------------------------------

        /* ------------- Genre ------------- */
        public async Task<IActionResult> Settings()
        {
            var vm = new SettingsPageVM
            {
                Genres = await _context.Genre.OrderBy(g => g.Name).ToListAsync(),
                Settings = await _context.Settings.ToListAsync(),
                WatchedWiths = await _context.WatchedWith.ToListAsync()

            };
            return View(vm);
        }

        // ---------- ADD ----------
        [HttpGet]
        public IActionResult CreatePartial()
        {
            return PartialView("~/Views/Genre/_CreatePartial.cshtml", new Genre());
        }

        [HttpPost]
        public async Task<IActionResult> CreatePartial(Genre genre)
        {
            if (!ModelState.IsValid)
                return PartialView("~/Views/Genre/_CreatePartial.cshtml", genre);

            _context.Add(genre);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // ---------- EDIT ----------
        [HttpGet]
        public async Task<IActionResult> EditPartial(int id)
        {
            var genre = await _context.Genre.FindAsync(id);
            if (genre == null) return NotFound();

            return PartialView("~/Views/Genre/_EditPartial.cshtml", genre);
        }

        [HttpPost]
        public async Task<IActionResult> EditPartial(Genre genre)
        {
            if (!ModelState.IsValid)
                return PartialView("~/Views/Genre/_EditPartial.cshtml", genre);

            _context.Update(genre);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // ---------- DELETE ----------
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var genre = await _context.Genre.FindAsync(id);
            if (genre == null) return NotFound();

            _context.Genre.Remove(genre);
            await _context.SaveChangesAsync();

            return Ok();
        }


        /* ------------- Setting ------------- */
        [HttpGet]
        public IActionResult CreateSettingPartial()
            => PartialView("~/Views/Setting/_CreatePartial.cshtml", new Setting());

        // ---------- ADD ----------
        [HttpPost]
        public async Task<IActionResult> CreateSettingPartial(Setting s)
        {
            if (!ModelState.IsValid) return PartialView("~/Views/Setting/_CreatePartial.cshtml", s);
            _context.Add(s); await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // ---------- EDIT ----------
        [HttpGet]
        public async Task<IActionResult> EditSettingPartial(int id)
        {
            var s = await _context.Settings.FindAsync(id);
            return s == null ? NotFound() : PartialView("~/Views/Setting/_EditPartial.cshtml", s);
        }

        [HttpPost]
        public async Task<IActionResult> EditSettingPartial(Setting s)
        {
            if (!ModelState.IsValid) return PartialView("~/Views/Setting/_EditPartial.cshtml", s);
            _context.Update(s); await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // ---------- DELETE ----------
        [HttpPost]
        public async Task<IActionResult> DeleteSettingConfirmed(int id)
        {
            var s = await _context.Settings.FindAsync(id);
            if (s == null) return NotFound();
            _context.Settings.Remove(s); await _context.SaveChangesAsync();
            return Ok();
        }


        /* ------------- WatchedWith ------------- */
        [HttpGet]
        public IActionResult CreateWatchedWithPartial()
            => PartialView("~/Views/WatchedWith/_CreatePartial.cshtml", new WatchedWith());

        // ---------- ADD ----------
        [HttpPost]
        public async Task<IActionResult> CreateWatchedWithPartial(WatchedWith s)
        {
            if (!ModelState.IsValid) return PartialView("~/Views/WatchedWith/_CreatePartial.cshtml", s);
            _context.Add(s); await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // ---------- EDIT ----------
        [HttpGet]
        public async Task<IActionResult> EditWatchedWithPartial(int id)
        {
            var s = await _context.WatchedWith.FindAsync(id);
            return s == null ? NotFound() : PartialView("~/Views/WatchedWith/_EditPartial.cshtml", s);
        }

        [HttpPost]
        public async Task<IActionResult> EditWatchedWithPartial(WatchedWith s)
        {
            if (!ModelState.IsValid) return PartialView("~/Views/WatchedWith/_EditPartial.cshtml", s);
            _context.Update(s); await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // ---------- DELETE ----------
        [HttpPost]
        public async Task<IActionResult> DeleteWatchedWithConfirmed(int id)
        {
            var s = await _context.WatchedWith.FindAsync(id);
            if (s == null) return NotFound();
            _context.WatchedWith.Remove(s); await _context.SaveChangesAsync();
            return Ok();
        }




        /* ------- Home Imgs (Settings) ------- */

        [HttpPost]
        public async Task<IActionResult> UploadSettingImage(IFormFile file, string settingName)
        {
            if (file != null && (file.ContentType == "image/jpeg" || file.ContentType == "image/png"))
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img");
                Directory.CreateDirectory(uploadsFolder);

                string fileName = $"{settingName}{Path.GetExtension(file.FileName)}";
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // DB aktualisieren
                var setting = await _context.Settings.FirstOrDefaultAsync(s => s.SettingName == settingName);
                if (setting == null)
                {
                    setting = new Setting { SettingName = settingName };
                    _context.Settings.Add(setting);
                }

                setting.Comment = fileName;
                await _context.SaveChangesAsync();

                return Json(new { success = true, fileName = fileName });
            }

            return Json(new { success = false, error = "Nur JPG oder PNG erlaubt." });
        }


        //----------------------------------------- Search -----------------------------------------

        public IActionResult Search()
        {
            return View();
        }


    }
}
