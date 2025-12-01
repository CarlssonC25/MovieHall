using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieHall.Data;
using MovieHall.Models;
using MovieHall.SaveModel;
using MovieHall.ViewModels;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static Azure.Core.HttpHeader;

namespace MovieHall.Controllers
{
    public class MovieController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        public MovieController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index(int page = 1, string? filter = "All")
        {
            int pageSize = 12;
            bool isFskFilter = false;

            var query = _context.Movies
                .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
                .Include(m => m.MovieWatchedWiths).ThenInclude(mw => mw.WatchedWith)
                .Where(m => m.ParentId == null);

            var DBnotes = await _context.MovieNotes
                    .Include(n => n.Movie)
                    .ToListAsync();

            var notes = new List<MovieNoteVM>();

            if (DBnotes.Any())
            {
                notes = DBnotes.Select(note => new MovieNoteVM
                {
                    Id = note.Id,
                    Comment = note.Comment,
                    MovieId = note.MovieId,
                    Img = note.Movie.Img
                }).ToList();
            }

            // Filter anwenden
            if (!string.IsNullOrEmpty(filter) && filter != "All")
            {
                if (filter == "Favorit")
                {
                    query = query.Where(m => m.Favorit == true);
                }
                else if (int.TryParse(filter, out int fskValue))
                {
                    isFskFilter = true;
                    query = query.Where(m => m.FSK <= fskValue);
                }
            }

            int totalMovies = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalMovies / (double)pageSize);

            // Sortierung je nach Filter
            if (isFskFilter)
            {
                query = query.OrderByDescending(m => m.FSK)
                             .ThenByDescending(m => m.ReleaseDate);
            }
            else
            {
                query = query.OrderByDescending(m => m.ReleaseDate);
            }

            var movies = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            //movie Infos
            var movieInfos = new List<MovieViewInfos>();

            foreach (var parent in movies)
            {
                var children = await _context.Movies
                    .Where(c => c.ParentId == parent.Id)
                    .ToListAsync();

                var allRelated = new List<Movie> { parent };
                allRelated.AddRange(children);

                int allSeasons = allRelated.Count;

                int available = allRelated.Count(a => a.Buy > 0);

                movieInfos.Add(new MovieViewInfos
                {
                    MovieId = parent.Id,
                    AllSeasons = allSeasons,
                    Available = available
                });
            }

            var viewModel = new MovieVM
            {
                Movies = movies,
                MovieInfos = movieInfos,
                CurrentPage = page,
                TotalPages = totalPages,
                CurrentFilter = filter,
                Notes = notes,
                MovieSume = await _context.Movies.CountAsync(),

            };
            ViewBag.Type = "Movie";

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("~/Views/Movie/_MovieListPartial.cshtml", viewModel);
            }

            return View(viewModel);
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

                if (svMovie.Img != null)
                {
                    // TEMP PFAD NUR BEI FEHLERN
                    string tempFolder = Path.Combine(_env.WebRootPath, "temp");
                    Directory.CreateDirectory(tempFolder);

                    string tempFile = Path.Combine(tempFolder, svMovie.Img.FileName);
                    using (var stream = new FileStream(tempFile, FileMode.Create))
                    {
                        await svMovie.Img.CopyToAsync(stream);
                    }
                    svMovie.TempImgPath = "/temp/" + svMovie.Img.FileName;
                }

                return PartialView("~/Views/Movie/_CreatePartial.cshtml", svMovie);
            }

            var movie = new Movie
            {
                Name = svMovie.Name,
                Buy = svMovie.Buy,
                Description = svMovie.Description,
                Favorit = svMovie.Favorit,
                FSK = svMovie.FSK,
                Language = string.Join(", ", svMovie.Language.ToArray()),
                ReleaseDate = svMovie.ReleaseDate ?? new DateTime((int)svMovie.ReleaseYear, 1, 1),
                Link = svMovie.Link,

                Parent = svMovie.Parent,
            };

            // IMG 
            if (svMovie.Img != null && (svMovie.Img.ContentType == "image/jpeg" || svMovie.Img.ContentType == "image/png"))
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "img/Movie_imgs");
                Directory.CreateDirectory(uploadsFolder);

                string fileName = "";
                var unique = false;
                while (!unique)
                {
                    Random rnd = new Random();
                    string cleanName = Regex.Replace(svMovie.Name, @"[^a-zA-Z0-9]", "");
                    fileName = $"{cleanName}{rnd.Next(1, 100)}{Path.GetExtension(svMovie.Img.FileName)}";

                    var exists = await _context.Movies.FirstOrDefaultAsync(s => s.Img == fileName);
                    if (exists == null)
                    {
                        unique = true;
                    }
                }

                movie.Img = fileName;

                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await svMovie.Img.CopyToAsync(stream);
                }
            } else if (!string.IsNullOrEmpty(svMovie.TempImgPath))
            {
                // TEMP → FINAL
                string tempFullPath = Path.Combine(_env.WebRootPath, svMovie.TempImgPath.TrimStart('/'));

                if (System.IO.File.Exists(tempFullPath))
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "img/Movie_imgs");
                    Directory.CreateDirectory(uploadsFolder);
                                        
                    string fileName = "";
                    var unique = false;
                    while (!unique)
                    {
                        Random rnd = new Random();
                        string cleanName = Regex.Replace(svMovie.Name, @"[^a-zA-Z0-9]", "");
                        string ext = Path.GetExtension(tempFullPath);
                        fileName = $"{cleanName}{rnd.Next(1, 100)}{ext}";

                        var exists = await _context.Movies.FirstOrDefaultAsync(s => s.Img == fileName);
                        if (exists == null)
                        {
                            unique = true;
                        }
                    }

                    string dest = Path.Combine(uploadsFolder, fileName);
                    System.IO.File.Move(tempFullPath, dest);

                    movie.Img = fileName;
                }
                else
                {
                    ModelState.AddModelError(nameof(svMovie.Img), "Temporäres Bild nicht gefunden. Bitte erneut auswählen.");
                    ViewBag.Genres = Genres;
                    ViewBag.WatchedWith = WatchedWith;
                    return PartialView("~/Views/Anime/_CreatePartial.cshtml", svMovie);
                }
            } else
            {
                ModelState.AddModelError(nameof(svMovie.Img), "Bild ist ein Pflichtfeld");

                ViewBag.Genres = Genres;
                ViewBag.WatchedWith = WatchedWith;

                return PartialView("~/Views/Movie/_CreatePartial.cshtml", svMovie);
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
                ReleaseYear = movieDb.ReleaseDate.Year,
                Link = movieDb.Link,
                Language = movieDb.Language?
                    .Split(", ", StringSplitOptions.RemoveEmptyEntries)
                    .ToList() ?? new List<string>(),
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
            dbMovie.Language = string.Join(", ", svMovie.Language.ToArray());
            dbMovie.Link = svMovie.Link;
            dbMovie.FSK = svMovie.FSK;
            dbMovie.Buy = svMovie.Buy;
            dbMovie.Favorit = svMovie.Favorit;
            dbMovie.ReleaseDate = svMovie.ReleaseDate ?? new DateTime((int)svMovie.ReleaseYear, 1, 1);
            dbMovie.ParentId = svMovie.ParentId;

            if (svMovie.Img != null && (svMovie.Img.ContentType == "image/jpeg" || svMovie.Img.ContentType == "image/png"))
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "img/Movie_imgs");
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
                var unique = false;
                while (!unique)
                {
                    Random rnd = new Random();
                    string cleanName = Regex.Replace(svMovie.Name, @"[^a-zA-Z0-9]", "");
                    fileName = $"{cleanName}{rnd.Next(1, 100)}{Path.GetExtension(svMovie.Img.FileName)}";

                    var exists = await _context.Animes.FirstOrDefaultAsync(s => s.Img == fileName);
                    if (exists == null)
                    {
                        unique = true;
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
            string uploadsFolder = Path.Combine(_env.WebRootPath, "img/Movie_imgs");
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
    }
}
