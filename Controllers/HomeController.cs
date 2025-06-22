using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieHall.Data;
using MovieHall.Models;
using MovieHall.SaveModel;
using MovieHall.ViewModels;
using System.Diagnostics;
using System.IO;

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
            return PartialView("~/Views/Movie/_CreatePartial.cshtml", new SaveMovie());
        }

        [HttpPost]
        public async Task<IActionResult> CreateMoviePartial(SaveMovie svMovie)
        {

            var Genres = await _context.Genre.OrderBy(g => g.Name).ToListAsync();
            var WatchedWith = await _context.WatchedWith.OrderBy(w => w.Name).ToListAsync();

            if (!ModelState.IsValid)
            {
                ViewBag.Genres = Genres;
                ViewBag.WatchedWith = WatchedWith;
                return PartialView("~/Views/Movie/_CreatePartial.cshtml", svMovie);
            }

            var movie = new Movie
            {
                Name = svMovie.Name,
                Buy = svMovie.Buy,
                Description = svMovie.Description,
                Favorit = svMovie.Favorit,
                FSK = svMovie.FSK,
                Language = svMovie.Language,
                ReleaseDate = svMovie.ReleaseDate,
                Link = svMovie.Link,

                Parent = svMovie.Parent,
            };

            if (svMovie.Img != null && (svMovie.Img.ContentType == "image/jpeg" || svMovie.Img.ContentType == "image/png"))
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/Movie_imgs");
                Directory.CreateDirectory(uploadsFolder);

                string fileName = "";
                var fileNameUnique = false;
                while(fileNameUnique == false)
                {
                    Random rnd = new Random();
                    fileName = $"{svMovie.Name}{rnd.Next(1, 100)}{Path.GetExtension(svMovie.Img.FileName)}";

                    var movieImg = await _context.Movies.FirstOrDefaultAsync(s => s.Img == fileName);

                    if (movieImg == null)
                    {
                        fileNameUnique = true;
                    }
                }

                movie.Img = fileName;

                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await svMovie.Img.CopyToAsync(stream);
                }
            }

            foreach (var genreId in svMovie.SelectedGenreIds)
                movie.MovieGenres.Add(new MovieGenre { GenreId = genreId });

            foreach (var wId in svMovie.SelectedWatchedWithIds)
                movie.MovieWatchedWiths.Add(new MovieWatchedWith { WatchedWithId = wId });

            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // ---------- EDIT ----------
        [HttpGet]
        public async Task<IActionResult> EditMoviePartial(int id)
        {
            var movieDb = await _context.Movies
                .Include(m => m.MovieGenres)
                .Include(m => m.MovieWatchedWiths)
                .FirstOrDefaultAsync(m => m.Id == id);

            var genres = await _context.Genre.OrderBy(g => g.Name).ToListAsync();
            var watchedWith = await _context.WatchedWith.OrderBy(w => w.Name).ToListAsync();

            ViewBag.Genres = genres;
            ViewBag.WatchedWiths = watchedWith;

            var path = $"/img/{movieDb.Img}";

            var movie = new SaveMovie
            {
                Id = movieDb.Id,
                Name = movieDb.Name,
                Buy = movieDb.Buy,
                Description = movieDb.Description,
                ImgPath = movieDb.Img,
                FSK = movieDb.FSK,
                Favorit = movieDb.Favorit,
                ReleaseDate = movieDb.ReleaseDate,
                Link = movieDb.Link,
                Language = movieDb.Language,
                ParentId = movieDb.ParentId,

                MovieGenres = movieDb.MovieGenres,
                MovieWatchedWiths = movieDb.MovieWatchedWiths,
            };

            return PartialView("~/Views/Movie/_EditPartial.cshtml", movie);
        }

        [HttpPost]
        public async Task<IActionResult> EditMoviePartial(SaveMovie svMovie)
        {
            var dbMovie = await _context.Movies
                .Include(m => m.MovieGenres)
                .Include(m => m.MovieWatchedWiths)
                .FirstOrDefaultAsync(m => m.Id == svMovie.Id);

            if (dbMovie == null)
                return NotFound();

            // Update properties
            dbMovie.Name = svMovie.Name;
            dbMovie.Description = svMovie.Description;
            dbMovie.Language = svMovie.Language;
            dbMovie.Link = svMovie.Link;
            dbMovie.FSK = svMovie.FSK;
            dbMovie.Buy = svMovie.Buy;
            dbMovie.Favorit = svMovie.Favorit;
            dbMovie.ReleaseDate = svMovie.ReleaseDate;
            dbMovie.ParentId = svMovie.ParentId;

            if (svMovie.Img != null && (svMovie.Img.ContentType == "image/jpeg" || svMovie.Img.ContentType == "image/png"))
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/Movie_imgs");
                Directory.CreateDirectory(uploadsFolder);

                // Altes Bild löschen
                if (!string.IsNullOrEmpty(svMovie.ImgPath))
                {
                    string oldImagePath = Path.Combine(uploadsFolder, svMovie.ImgPath);
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // Neues Bild adden
                string fileName = "";
                var fileNameUnique = false;
                while (!fileNameUnique)
                {
                    Random rnd = new Random();
                    fileName = $"{svMovie.Name}{rnd.Next(1, 100)}{Path.GetExtension(svMovie.Img.FileName)}";

                    var movieImg = await _context.Movies.FirstOrDefaultAsync(s => s.Img == fileName);

                    if (movieImg == null)
                    {
                        fileNameUnique = true;
                    }
                }

                dbMovie.Img = fileName;

                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await svMovie.Img.CopyToAsync(stream);
                }
            }
            else
            {
                dbMovie.Img = svMovie.ImgPath;
            }

            // Update genres
            dbMovie.MovieGenres.Clear();
            foreach (var genreId in svMovie.SelectedGenreIds)
                dbMovie.MovieGenres.Add(new MovieGenre { MovieId = dbMovie.Id, GenreId = genreId });

            // Update watched with
            dbMovie.MovieWatchedWiths.Clear();
            foreach (var wId in svMovie.SelectedWatchedWithIds)
                dbMovie.MovieWatchedWiths.Add(new MovieWatchedWith { MovieId = dbMovie.Id, WatchedWithId = wId });

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // ---------- DELETE ----------
        [HttpPost]
        public async Task<IActionResult> DeleteMovieConfirmed(int id)
        {
            var movie = await _context.Movies
                .Include(m => m.MovieGenres)
                .Include(m => m.MovieWatchedWiths)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null) return NotFound();

            // Bild löschen
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/Movie_imgs");
            if (!string.IsNullOrEmpty(movie.Img))
            {
                string oldImagePath = Path.Combine(uploadsFolder, movie.Img);
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            // Zuerst abhängige Datensätze entfernen
            _context.MovieGenres.RemoveRange(movie.MovieGenres);
            _context.MovieWatchedWiths.RemoveRange(movie.MovieWatchedWiths);

            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }


        //----------------------------------------- Settings -----------------------------------------

        /* ---------------- Genre ---------------- */
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


        /* ---------------- Setting ---------------- */
        [HttpGet]
        public IActionResult CreateSettingPartial()
            => PartialView("~/Views/Setting/_CreatePartial.cshtml", new Setting());

        // ---------- ADD ----------
        [HttpPost]
        public async Task<IActionResult> CreateSettingPartial(Setting s)
        {
            if (!ModelState.IsValid) return PartialView("~/Views/Setting/_CreatePartial.cshtml", s);
            _context.Add(s); 
            await _context.SaveChangesAsync();
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
