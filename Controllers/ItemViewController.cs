using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieHall.Data;
using MovieHall.Models;
using MovieHall.SaveModel;
using MovieHall.ViewModels;
using System.Text.RegularExpressions;
using static Azure.Core.HttpHeader;

namespace MovieHall.Controllers
{
    public class ItemViewController : Controller
    {
        private readonly AppDbContext _context;
        public ItemViewController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string id, string type)
        {
            var ID = Int32.Parse(id);
            ViewBag.Type = type;

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

                var DBnotes = await _context.AnimeNotes
                    .Include(n => n.Anime)
                    .ToListAsync();

                var notes = new List<AnimeNoteVM>();

                if (DBnotes.Any())
                {
                    notes = DBnotes.Select(note => new AnimeNoteVM
                    {
                        Id = note.Id,
                        Comment = note.Comment,
                        AnimeId = note.AnimeId,
                        Img = note.Anime.Img
                    }).ToList();
                }

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
                aniMov.Country = item.Country;
                aniMov.Episodes = item.Episodes;
                aniMov.WhatTimes = item.WhatTimes;
                aniMov.ParentAnime = item.Parent;
                aniMov.AnimeGenres = item.AnimeGenres;
                aniMov.AnimeWatchedWiths = item.AnimeWatchedWiths;
                aniMov.AnimeSum = await _context.Animes.CountAsync();
                aniMov.AnimeEpSum = (int)await _context.Animes.SumAsync(a => a.Episodes);
                aniMov.AnimeNotes = notes;

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
                aniMov.MovieSum = await _context.Movies.CountAsync();
                aniMov.MovieNotes = notes;

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
            var parent = await _context.Movies
                .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
                .Include(m => m.MovieWatchedWiths).ThenInclude(mw => mw.WatchedWith)
                .Where(a => a.Id == parentId).FirstOrDefaultAsync();

            var model = new SaveMovie
            {
                ParentId = parent.Id,
                MovieGenres = parent.MovieGenres,
                FSK = parent.FSK
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
                ReleaseDate = svMovie.ReleaseDate ?? new DateTime((int)svMovie.ReleaseYear, 1, 1),
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
                    string cleanName = Regex.Replace(svMovie.Name, @"[^a-zA-Z0-9]", "");
                    fileName = $"{cleanName}{rnd.Next(1, 100)}{Path.GetExtension(svMovie.Img.FileName)}";

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
            var parent = await _context.Animes
                .Include(m => m.AnimeGenres).ThenInclude(mg => mg.Genre)
                .Include(m => m.AnimeWatchedWiths).ThenInclude(mw => mw.WatchedWith)
                .Where(a => a.Id == parentId).FirstOrDefaultAsync();


            var model = new SaveAnime
            {
                ParentId = parent.Id,
                AnimeGenres = parent.AnimeGenres,
                Country = parent.Country,
                Top = parent.Top,
                Language = parent.Language?
                    .Split(", ", StringSplitOptions.RemoveEmptyEntries)
                    .ToList() ?? new List<string>(),
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
                ReleaseDate = svAnime.ReleaseDate ?? new DateTime((int)svAnime.ReleaseYear, (int)svAnime.ReleaseMonth, 1),
                Link = svAnime.Link,
                Episodes = svAnime.Episodes,
                Language = string.Join(", ", svAnime.Language.ToArray()),
                ParentId = svAnime.ParentId,
                WhatTimes = svAnime.WhatTimes,
                Country = svAnime.Country,

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
                    string cleanName = Regex.Replace(svAnime.Name, @"[^a-zA-Z0-9]", "");
                    fileName = $"{cleanName}{rnd.Next(1, 100)}{Path.GetExtension(svAnime.Img.FileName)}";

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

        // ---------- WhatTimes EDIT ----------
        [HttpPost]
        public async Task<IActionResult> UpdateRewatch(int id, int change)
        {
            var dbAnime = await _context.Animes
                .FirstOrDefaultAsync(m => m.Id == id);

            dbAnime.WhatTimes += change;
            if (dbAnime.WhatTimes <= 0)
            {
                dbAnime.WhatTimes = 1;
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, newValue = dbAnime.WhatTimes });
        }
    }
}
