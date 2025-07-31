using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieHall.Data;
using MovieHall.Models;
using MovieHall.SaveModel;
using MovieHall.ViewModels;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;

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


        //----------------------------------------- View -----------------------------------------

        public async Task<IActionResult> ItemView(string id, string type)
        {
            var ID = Int32.Parse(id);

            // Error check
            if (ID <= 0 || type == null)
            {
                if (type == "Anime")
                {
                    return View("Anime.cshtml");
                }
                else if (type == "Movie")
                {
                    return View("Movie.cshtml");
                }
                else
                {
                    return View("Index.cshtml");
                }
            }

            ViewVM aniMov = new();

            if (type == "Anime")
            {
                var item = await _context.Animes
                    .Include(m => m.AnimeGenres).ThenInclude(mg => mg.Genre)
                    .Include(m => m.AnimeWatchedWiths).ThenInclude(mw => mw.WatchedWith)
                    .Where(m => m.Id == ID).FirstOrDefaultAsync();

                aniMov.Id = ID;
                aniMov.Name = item.Name;
                aniMov.Buy = item.Buy;
                aniMov.Description = item.Description;
                aniMov.Img = "/img/Anime_imgs/" + item.Img;
                aniMov.Link = item.Link;
                aniMov.Language = item.Language;
                aniMov.ReleaseDate = item.ReleaseDate;
                aniMov.ParentId = item.ParentId;


                aniMov.Orginal_Name = item.Orginal_Name;
                aniMov.Top = item.Top;
                aniMov.Episodes = item.Episodes;
                aniMov.ParentAnime = item.Parent;
                aniMov.AnimeGenres = item.AnimeGenres;
                aniMov.AnimeWatchedWiths = item.AnimeWatchedWiths;

                if (aniMov.ParentId != null)
                {
                    aniMov.ParentAnime = await _context.Animes
                        .Include(m => m.AnimeGenres).ThenInclude(mg => mg.Genre)
                        .Include(m => m.AnimeWatchedWiths).ThenInclude(mw => mw.WatchedWith)
                        .Where(m => m.Id == aniMov.ParentId).FirstOrDefaultAsync();

                    aniMov.ChildAnimes = await _context.Animes
                        .Include(m => m.AnimeGenres).ThenInclude(mg => mg.Genre)
                        .Include(m => m.AnimeWatchedWiths).ThenInclude(mw => mw.WatchedWith)
                        .Where(m => m.ParentId == aniMov.ParentId).ToListAsync();
                }
                else
                {
                    aniMov.ParentAnime = await _context.Animes
                        .Include(m => m.AnimeGenres).ThenInclude(mg => mg.Genre)
                        .Include(m => m.AnimeWatchedWiths).ThenInclude(mw => mw.WatchedWith)
                        .Where(m => m.Id == aniMov.Id).FirstOrDefaultAsync();

                    aniMov.ChildAnimes = await _context.Animes
                        .Include(m => m.AnimeGenres).ThenInclude(mg => mg.Genre)
                        .Include(m => m.AnimeWatchedWiths).ThenInclude(mw => mw.WatchedWith)
                        .Where(m => m.ParentId == aniMov.Id).ToListAsync();
                }
            }
            else if (type == "Movie")
            {
                var item = await _context.Movies
                    .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
                    .Include(m => m.MovieWatchedWiths).ThenInclude(mw => mw.WatchedWith)
                    .Where(m => m.Id == ID).FirstOrDefaultAsync();

                aniMov.Id = ID;
                aniMov.Name = item.Name;
                aniMov.Buy = item.Buy;
                aniMov.Description = item.Description;
                aniMov.Img = "/img/Movie_imgs/" + item.Img;
                aniMov.Link = item.Link;
                aniMov.Language = item.Language;
                aniMov.ReleaseDate = item.ReleaseDate;
                aniMov.ParentId = item.ParentId;

                aniMov.FSK = item.FSK;
                aniMov.Favorit = item.Favorit;
                aniMov.ParentMovie = item.Parent;
                aniMov.MovieGenres = item.MovieGenres;
                aniMov.MovieWatchedWiths = item.MovieWatchedWiths;

                if (aniMov.ParentId != null)
                {
                    aniMov.ParentMovie = await _context.Movies
                        .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
                        .Include(m => m.MovieWatchedWiths).ThenInclude(mw => mw.WatchedWith)
                        .Where(m => m.Id == aniMov.ParentId).FirstOrDefaultAsync();

                    aniMov.ChildMovies = await _context.Movies
                        .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
                        .Include(m => m.MovieWatchedWiths).ThenInclude(mw => mw.WatchedWith)
                        .Where(m => m.ParentId == aniMov.ParentId).ToListAsync();
                }
                else
                {
                    aniMov.ParentMovie = await _context.Movies
                        .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
                        .Include(m => m.MovieWatchedWiths).ThenInclude(mw => mw.WatchedWith)
                        .Where(m => m.Id == aniMov.Id).FirstOrDefaultAsync();

                    aniMov.ChildMovies = await _context.Movies
                        .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
                        .Include(m => m.MovieWatchedWiths).ThenInclude(mw => mw.WatchedWith)
                        .Where(m => m.ParentId == aniMov.Id).ToListAsync();
                }

            }

            return View(aniMov);
        }


        // ---------- Movie ----------
        // -- ADD Movie Child --
        [HttpGet]
        public async Task<IActionResult> CreateMovieChildPartial(int parentId)
        {
            var model = new SaveMovie
            {
                ParentId = parentId
            };

            ViewBag.Genres = await _context.Genre.OrderBy(g => g.Name).ToListAsync();
            ViewBag.WatchedWith = await _context.WatchedWith.OrderBy(w => w.Name).ToListAsync();

            return PartialView("~/Views/Movie/_CreatePartial.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMovieChildPartial(SaveMovie svMovie)
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
                ParentId = svMovie.ParentId,

                Parent = svMovie.Parent,
            };

            if (svMovie.Img != null && (svMovie.Img.ContentType == "image/jpeg" || svMovie.Img.ContentType == "image/png"))
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/Movie_imgs");
                Directory.CreateDirectory(uploadsFolder);

                string fileName = "";
                var fileNameUnique = false;
                while (fileNameUnique == false)
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


        // ---------- Anime ----------
        // -- ADD Anime Child --
        [HttpGet]
        public async Task<IActionResult> CreateAnimeChildPartial(int parentId)
        {
            var model = new SaveAnime
            {
                ParentId = parentId
            };

            ViewBag.Genres = await _context.Genre.OrderBy(g => g.Name).ToListAsync();
            ViewBag.WatchedWith = await _context.WatchedWith.OrderBy(w => w.Name).ToListAsync();

            return PartialView("~/Views/Anime/_CreatePartial.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAnimeChildPartial(SaveAnime svAnime)
        {

            var Genres = await _context.Genre.OrderBy(g => g.Name).ToListAsync();
            var WatchedWith = await _context.WatchedWith.OrderBy(w => w.Name).ToListAsync();

            if (!ModelState.IsValid)
            {
                ViewBag.Genres = Genres;
                ViewBag.WatchedWith = WatchedWith;
                return PartialView("~/Views/Anime/_CreatePartial.cshtml", svAnime);
            }

            var anime = new Anime
            {
                Name = svAnime.Name,
                Orginal_Name = svAnime.Orginal_Name,
                Top = svAnime.Top,
                Buy = svAnime.Buy,
                Description = svAnime.Description,
                ReleaseDate = svAnime.ReleaseDate,
                Link = svAnime.Link,
                Episodes = svAnime.Episodes,
                Language = svAnime.Language,
                ParentId = svAnime.ParentId,

                Parent = svAnime.Parent,
            };

            if (svAnime.Img != null && (svAnime.Img.ContentType == "image/jpeg" || svAnime.Img.ContentType == "image/png"))
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/Anime_imgs");
                Directory.CreateDirectory(uploadsFolder);

                string fileName = "";
                var fileNameUnique = false;
                while (fileNameUnique == false)
                {
                    Random rnd = new Random();
                    fileName = $"{svAnime.Name}{rnd.Next(1, 100)}{Path.GetExtension(svAnime.Img.FileName)}";

                    var animeImg = await _context.Animes.FirstOrDefaultAsync(s => s.Img == fileName);

                    if (animeImg == null)
                    {
                        fileNameUnique = true;
                    }
                }

                anime.Img = fileName;

                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await svAnime.Img.CopyToAsync(stream);
                }
            }

            foreach (var genreId in svAnime.SelectedGenreIds)
                anime.AnimeGenres.Add(new AnimeGenre { GenreId = genreId });

            foreach (var wId in svAnime.SelectedWatchedWithIds)
                anime.AnimeWatchedWiths.Add(new AnimeWatchedWith { WatchedWithId = wId });

            _context.Animes.Add(anime);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }



        //----------------------------------------- Anime -----------------------------------------
        public async Task<IActionResult> Anime()
        {
            var anime = await _context.Animes
                .Include(m => m.AnimeGenres).ThenInclude(mg => mg.Genre)
                .Include(m => m.AnimeWatchedWiths).ThenInclude(mw => mw.WatchedWith)
                .Where(m => m.ParentId == null)
                .ToListAsync();

            return View(anime);
        }


        // ---------- ADD ----------
        [HttpGet]
        public async Task<IActionResult> CreateAnimePartial()
        {
            ViewBag.Genres = await _context.Genre.OrderBy(g => g.Name).ToListAsync();
            ViewBag.WatchedWith = await _context.WatchedWith.OrderBy(w => w.Name).ToListAsync();
            return PartialView("~/Views/Anime/_CreatePartial.cshtml", new SaveAnime());
        }

        [HttpPost]
        public async Task<IActionResult> CreateAnimePartial(SaveAnime svAnime)
        {

            var Genres = await _context.Genre.OrderBy(g => g.Name).ToListAsync();
            var WatchedWith = await _context.WatchedWith.OrderBy(w => w.Name).ToListAsync();

            if (!ModelState.IsValid)
            {
                ViewBag.Genres = Genres;
                ViewBag.WatchedWith = WatchedWith;
                return PartialView("~/Views/Anime/_CreatePartial.cshtml", svAnime);
            }

            var anime = new Anime
            {
                Name = svAnime.Name,
                Orginal_Name = svAnime.Orginal_Name,
                Top = svAnime.Top,
                Buy = svAnime.Buy,
                Description = svAnime.Description,
                ReleaseDate = svAnime.ReleaseDate,
                Link = svAnime.Link,
                Episodes = svAnime.Episodes,
                Language = svAnime.Language,

                Parent = svAnime.Parent,
            };

            if (svAnime.Img != null && (svAnime.Img.ContentType == "image/jpeg" || svAnime.Img.ContentType == "image/png"))
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/Anime_imgs");
                Directory.CreateDirectory(uploadsFolder);

                string fileName = "";
                var fileNameUnique = false;
                while (fileNameUnique == false)
                {
                    Random rnd = new Random();
                    fileName = $"{svAnime.Name}{rnd.Next(1, 100)}{Path.GetExtension(svAnime.Img.FileName)}";

                    var animeImg = await _context.Animes.FirstOrDefaultAsync(s => s.Img == fileName);

                    if (animeImg == null)
                    {
                        fileNameUnique = true;
                    }
                }

                anime.Img = fileName;

                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await svAnime.Img.CopyToAsync(stream);
                }
            }

            foreach (var genreId in svAnime.SelectedGenreIds)
                anime.AnimeGenres.Add(new AnimeGenre { GenreId = genreId });

            foreach (var wId in svAnime.SelectedWatchedWithIds)
                anime.AnimeWatchedWiths.Add(new AnimeWatchedWith { WatchedWithId = wId });

            _context.Animes.Add(anime);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // ---------- EDIT ----------
        [HttpGet]
        public async Task<IActionResult> EditAnimePartial(int id)
        {
            var animeDb = await _context.Animes
                .Include(m => m.AnimeGenres)
                .Include(m => m.AnimeWatchedWiths)
                .FirstOrDefaultAsync(m => m.Id == id);

            var genres = await _context.Genre.OrderBy(g => g.Name).ToListAsync();
            var watchedWith = await _context.WatchedWith.OrderBy(w => w.Name).ToListAsync();

            ViewBag.Genres = genres;
            ViewBag.WatchedWiths = watchedWith;

            var path = $"/img/{animeDb.Img}";

            var anime = new SaveAnime
            {
                Id = animeDb.Id,
                Name = animeDb.Name,
                Orginal_Name = animeDb.Orginal_Name,
                Top = animeDb.Top,
                Buy = animeDb.Buy,
                Description = animeDb.Description,
                ReleaseDate = animeDb.ReleaseDate,
                ImgPath = animeDb.Img,
                Link = animeDb.Link,
                Episodes = animeDb.Episodes,
                Language = animeDb.Language,
                ParentId = animeDb.ParentId,

                AnimeGenres = animeDb.AnimeGenres,
                AnimeWatchedWiths = animeDb.AnimeWatchedWiths,
            };

            return PartialView("~/Views/Anime/_EditPartial.cshtml", anime);
        }

        [HttpPost]
        public async Task<IActionResult> EditAnimePartial(SaveAnime svAnime)
        {
            var dbAnime = await _context.Animes
                .Include(m => m.AnimeGenres)
                .Include(m => m.AnimeWatchedWiths)
                .FirstOrDefaultAsync(m => m.Id == svAnime.Id);

            if (dbAnime == null)
                return NotFound();

            // Update properties
            dbAnime.Name = svAnime.Name;
            dbAnime.Orginal_Name = svAnime.Orginal_Name;
            dbAnime.Description = svAnime.Description;
            dbAnime.Language = svAnime.Language;
            dbAnime.Link = svAnime.Link;
            dbAnime.Top = svAnime.Top;
            dbAnime.Episodes = svAnime.Episodes;
            dbAnime.Buy = svAnime.Buy;
            dbAnime.ReleaseDate = svAnime.ReleaseDate;
            dbAnime.ParentId = svAnime.ParentId;

            if (svAnime.Img != null && (svAnime.Img.ContentType == "image/jpeg" || svAnime.Img.ContentType == "image/png"))
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/Anime_imgs");
                Directory.CreateDirectory(uploadsFolder);

                // Altes Bild löschen
                if (!string.IsNullOrEmpty(svAnime.ImgPath))
                {
                    string oldImagePath = Path.Combine(uploadsFolder, svAnime.ImgPath);
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
                    fileName = $"{svAnime.Name}{rnd.Next(1, 100)}{Path.GetExtension(svAnime.Img.FileName)}";

                    var animeImg = await _context.Animes.FirstOrDefaultAsync(s => s.Img == fileName);

                    if (animeImg == null)
                    {
                        fileNameUnique = true;
                    }
                }

                dbAnime.Img = fileName;

                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await svAnime.Img.CopyToAsync(stream);
                }
            }
            else
            {
                dbAnime.Img = svAnime.ImgPath;
            }

            // Update genres
            dbAnime.AnimeGenres.Clear();
            foreach (var genreId in svAnime.SelectedGenreIds)
                dbAnime.AnimeGenres.Add(new AnimeGenre { AnimeId = dbAnime.Id, GenreId = genreId });

            // Update watched with
            dbAnime.AnimeWatchedWiths.Clear();
            foreach (var wId in svAnime.SelectedWatchedWithIds)
                dbAnime.AnimeWatchedWiths.Add(new AnimeWatchedWith { AnimeId = dbAnime.Id, WatchedWithId = wId });

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }


        // ---------- DELETE ----------
        [HttpPost]
        public async Task<IActionResult> DeleteAnimeConfirmed(int id)
        {
            var anime = await _context.Animes
                .Include(m => m.AnimeGenres)
                .Include(m => m.AnimeWatchedWiths)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (anime == null) return NotFound();

            // Bild löschen
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/Anime_imgs");
            if (!string.IsNullOrEmpty(anime.Img))
            {
                string oldImagePath = Path.Combine(uploadsFolder, anime.Img);
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            // Zuerst abhängige Datensätze entfernen
            _context.AnimeGenres.RemoveRange(anime.AnimeGenres);
            _context.AnimeWatchedWiths.RemoveRange(anime.AnimeWatchedWiths);

            // Wenn anime kein Parent hat => lösche alle Childs
            if (anime.ParentId == null)
            {
                var childAnimes = await _context.Animes
                    .Where(a => a.ParentId == anime.Id)
                    .Include(a => a.AnimeGenres)
                    .Include(a => a.AnimeWatchedWiths)
                    .ToListAsync();

                foreach (var child in childAnimes)
                {
                    // Bild vom Child löschen
                    if (!string.IsNullOrEmpty(child.Img))
                    {
                        string childImagePath = Path.Combine(uploadsFolder, child.Img);
                        if (System.IO.File.Exists(childImagePath))
                        {
                            System.IO.File.Delete(childImagePath);
                        }
                    }

                    _context.AnimeGenres.RemoveRange(child.AnimeGenres);
                    _context.AnimeWatchedWiths.RemoveRange(child.AnimeWatchedWiths);
                    _context.Animes.Remove(child);
                }
            }

            _context.Animes.Remove(anime);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
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


            // Wenn movies kein Parent hat => lösche alle Childs
            if (movie.ParentId == null)
            {
                var childMovies = await _context.Movies
                    .Where(a => a.ParentId == movie.Id)
                    .Include(m => m.MovieGenres)
                    .Include(m => m.MovieWatchedWiths)
                    .ToListAsync();

                foreach (var child in childMovies)
                {
                    // Bild vom Child löschen
                    if (!string.IsNullOrEmpty(child.Img))
                    {
                        string childImagePath = Path.Combine(uploadsFolder, child.Img);
                        if (System.IO.File.Exists(childImagePath))
                        {
                            System.IO.File.Delete(childImagePath);
                        }
                    }

                    _context.MovieGenres.RemoveRange(child.MovieGenres);
                    _context.MovieWatchedWiths.RemoveRange(child.MovieWatchedWiths);
                    _context.Movies.Remove(child);
                }
            }

            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }


        //----------------------------------------- Settings -----------------------------------------

        /* ----- Genre ----- */
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

        // --- ADD ---
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

        // --- EDIT ---
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

        // --- DELETE ---
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var genre = await _context.Genre.FindAsync(id);
            if (genre == null) return NotFound();

            _context.Genre.Remove(genre);
            await _context.SaveChangesAsync();

            return Ok();
        }


        /* ----- Setting ----- */
        [HttpGet]
        public IActionResult CreateSettingPartial()
            => PartialView("~/Views/Setting/_CreatePartial.cshtml", new Setting());

        // --- ADD ---
        [HttpPost]
        public async Task<IActionResult> CreateSettingPartial(Setting s)
        {
            if (!ModelState.IsValid) return PartialView("~/Views/Setting/_CreatePartial.cshtml", s);
            _context.Add(s); 
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // --- EDIT ---
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

        // --- DELETE ---
        [HttpPost]
        public async Task<IActionResult> DeleteSettingConfirmed(int id)
        {
            var s = await _context.Settings.FindAsync(id);
            if (s == null) return NotFound();
            _context.Settings.Remove(s); await _context.SaveChangesAsync();
            return Ok();
        }


        /* ------ WatchedWith ----- */
        [HttpGet]
        public IActionResult CreateWatchedWithPartial()
            => PartialView("~/Views/WatchedWith/_CreatePartial.cshtml", new WatchedWith());

        // --- ADD ---
        [HttpPost]
        public async Task<IActionResult> CreateWatchedWithPartial(WatchedWith s)
        {
            if (!ModelState.IsValid) return PartialView("~/Views/WatchedWith/_CreatePartial.cshtml", s);
            _context.Add(s); await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // --- EDIT ---
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

        // --- DELETE ---
        [HttpPost]
        public async Task<IActionResult> DeleteWatchedWithConfirmed(int id)
        {
            var s = await _context.WatchedWith.FindAsync(id);
            if (s == null) return NotFound();
            _context.WatchedWith.Remove(s); await _context.SaveChangesAsync();
            return Ok();
        }


        /* ----- Home Imgs (Settings) ----- */

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
