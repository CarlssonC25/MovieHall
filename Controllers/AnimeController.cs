using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieHall.Data;
using MovieHall.Models;
using MovieHall.SaveModel;
using MovieHall.ViewModels;
using System.Text.RegularExpressions;

namespace MovieHall.Controllers
{
    public class AnimeController : Controller
    {

        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AnimeController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index(int page = 1, string? Cfilter = "ALL", string? Lfilter = "ALL", string? Rfilter = "ALL")
        {
            int pageSize = 12;

            var query = _context.Animes
                .Include(m => m.AnimeGenres).ThenInclude(mg => mg.Genre)
                .Include(m => m.AnimeWatchedWiths).ThenInclude(mw => mw.WatchedWith)
                .Where(m => m.ParentId == null);

            // notes
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

            // Language-Filter
            if (!string.IsNullOrEmpty(Lfilter) && Lfilter != "ALL")
            {
                if (Lfilter == "DUB")
                {
                    query = query.Where(a => a.Language.Contains("Deutsch") || a.Language.Contains("Englisch"));
                }
                else if (Lfilter == "SUB")
                {
                    query = query.Where(a => !a.Language.Contains("Deutsch") && !a.Language.Contains("Englisch"));
                }
            }

            // Country-Filter
            if (!string.IsNullOrEmpty(Cfilter) && Cfilter != "ALL")
            {
                query = query.Where(a => a.Country == Cfilter);
            }

            // Rank-Filter
            if (Rfilter != "ALL" && int.TryParse(Rfilter, out int rank))
            {
                query = query.Where(a => a.Top == rank);
            }

            // Pagination
            int totalAnimes = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalAnimes / (double)pageSize);

            query = query
                .OrderBy(a => a.Top)
                .ThenByDescending(a => a.ReleaseDate);

            var animes = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            //anime Infos
            var animeInfos = new List<AnimeViewInfos>();

            foreach (var parent in animes)
            {
                var children = await _context.Animes
                    .Where(c => c.ParentId == parent.Id)
                    .ToListAsync();

                var allRelated = new List<Anime> { parent };
                allRelated.AddRange(children);

                int allEpisodes = allRelated.Sum(a => a.Episodes ?? 0);

                int allSeasons = allRelated.Count;

                int available = allRelated.Count(a => a.Buy > 0);

                animeInfos.Add(new AnimeViewInfos
                {
                    AnimeId = parent.Id,
                    AllEpisodes = allEpisodes,
                    AllSeasons = allSeasons,
                    Available = available
                });
            }

            // retrun Model
            var sum = await _context.Animes.CountAsync();
            var eps = (int)await _context.Animes.SumAsync(a => a.Episodes);
            var totalEpisodes = await _context.Animes.Where(a => a.Episodes.HasValue).SumAsync(a => a.Episodes.Value * a.WhatTimes);
            var viewModel = new AnimeVM
            {
                Animes = animes,
                AnimeInfos = animeInfos,
                CurrentPage = page,
                TotalPages = totalPages,
                CountryFilter = Cfilter,
                LanguageFilter = Lfilter,
                RankFilter = Rfilter,
                Notes = notes,
                AnimeTime = (eps * 20) / 60 + "H",
                WatchTime = (totalEpisodes * 20) / 60 + "H",

                AnimeSum = sum,
                AnimeEpSum = eps,
            };

            ViewBag.Type = "Anime";


            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("~/Views/Anime/_AnimeListPartial.cshtml", viewModel);
            }

            return View(viewModel);
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

                if (svAnime.Img != null)
                {
                    // TEMP PFAD NUR BEI FEHLERN
                    string tempFolder = Path.Combine(_env.WebRootPath, "temp");
                    Directory.CreateDirectory(tempFolder);

                    string tempFile = Path.Combine(tempFolder, svAnime.Img.FileName);
                    using (var stream = new FileStream(tempFile, FileMode.Create))
                    {
                        await svAnime.Img.CopyToAsync(stream);
                    }
                    svAnime.TempImgPath = "/temp/" + svAnime.Img.FileName;
                }

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
                WhatTimes = svAnime.WhatTimes,
                Country = svAnime.Country,

                Parent = svAnime.Parent,
            };

            // IMG
            if (svAnime.Img != null && (svAnime.Img.ContentType == "image/jpeg" || svAnime.Img.ContentType == "image/png"))
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "img/Anime_imgs");
                Directory.CreateDirectory(uploadsFolder);

                string fileName = "";
                var unique = false;
                while (!unique)
                {
                    Random rnd = new Random();
                    string cleanName = Regex.Replace(svAnime.Name, @"[^a-zA-Z0-9]", "");
                    fileName = $"{cleanName}{rnd.Next(1, 100)}{Path.GetExtension(svAnime.Img.FileName)}";

                    var exists = await _context.Animes.FirstOrDefaultAsync(s => s.Img == fileName);
                    if (exists == null)
                    {
                        unique = true;
                    }
                }

                anime.Img = fileName;

                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await svAnime.Img.CopyToAsync(stream);
                }
            } else if (!string.IsNullOrEmpty(svAnime.TempImgPath))
            {
                // TEMP → FINAL
                string tempFullPath = Path.Combine(_env.WebRootPath, svAnime.TempImgPath.TrimStart('/'));

                if (System.IO.File.Exists(tempFullPath))
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "img/Anime_imgs");
                    Directory.CreateDirectory(uploadsFolder);

                    string fileName = "";
                    var unique = false;
                    while (!unique)
                    {
                        Random rnd = new Random();
                        string cleanName = Regex.Replace(svAnime.Name, @"[^a-zA-Z0-9]", "");
                        string ext = Path.GetExtension(tempFullPath);
                        fileName = $"{cleanName}{rnd.Next(1, 100)}{ext}";

                        var exists = await _context.Animes.FirstOrDefaultAsync(s => s.Img == fileName);
                        if (exists == null) 
                        {
                            unique = true;
                        } 
                    }

                    string dest = Path.Combine(uploadsFolder, fileName);
                    System.IO.File.Move(tempFullPath, dest);

                    anime.Img = fileName;
                }
                else
                {
                    ModelState.AddModelError(nameof(svAnime.Img), "Temporäres Bild nicht gefunden. Bitte erneut auswählen.");
                    ViewBag.Genres = Genres;
                    ViewBag.WatchedWith = WatchedWith;
                    return PartialView("~/Views/Anime/_CreatePartial.cshtml", svAnime);
                }
            } else
            {
                ModelState.AddModelError(nameof(svAnime.Img), "Bild ist ein Pflichtfeld");

                ViewBag.Genres = Genres;
                ViewBag.WatchedWith = WatchedWith;

                return PartialView("~/Views/Anime/_CreatePartial.cshtml", svAnime);
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
                ReleaseYear = animeDb.ReleaseDate.Year,
                ReleaseMonth = animeDb.ReleaseDate.Month,
                ImgPath = animeDb.Img,
                Link = animeDb.Link,
                Episodes = animeDb.Episodes,
                WhatTimes = animeDb.WhatTimes,
                ParentId = animeDb.ParentId,
                Country = animeDb.Country,
                Language = animeDb.Language?
                    .Split(", ", StringSplitOptions.RemoveEmptyEntries)
                    .ToList() ?? new List<string>(),

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
            {
                return NotFound();
            }

            if (dbAnime.Top != svAnime.Top)
            {
                var dbAnimeChild = await _context.Animes
                .Include(m => m.AnimeGenres)
                .Include(m => m.AnimeWatchedWiths)
                .Where(m => m.ParentId == svAnime.Id)
                .ToListAsync();

                if (dbAnimeChild != null)
                {
                    for (var i = 0; i < dbAnimeChild.Count; i++)
                    {
                        dbAnimeChild[i].Top = svAnime.Top;
                    }
                }
            }

            // Update properties
            dbAnime.Name = svAnime.Name;
            dbAnime.Orginal_Name = svAnime.Orginal_Name;
            dbAnime.Description = svAnime.Description;
            dbAnime.Language = string.Join(", ", svAnime.Language.ToArray());
            dbAnime.Link = svAnime.Link;
            dbAnime.Top = svAnime.Top;
            dbAnime.Episodes = svAnime.Episodes;
            dbAnime.Buy = svAnime.Buy;
            dbAnime.ReleaseDate = svAnime.ReleaseDate ?? new DateTime((int)svAnime.ReleaseYear, (int)svAnime.ReleaseMonth, 1);
            dbAnime.ParentId = svAnime.ParentId;
            dbAnime.WhatTimes = svAnime.WhatTimes;
            dbAnime.Country = svAnime.Country;

            if (svAnime.Img != null && (svAnime.Img.ContentType == "image/jpeg" || svAnime.Img.ContentType == "image/png"))
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "img/Anime_imgs");
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
                var unique = false;
                while (!unique)
                {
                    Random rnd = new Random();
                    string cleanName = Regex.Replace(svAnime.Name, @"[^a-zA-Z0-9]", "");
                    fileName = $"{cleanName}{rnd.Next(1, 100)}{Path.GetExtension(svAnime.Img.FileName)}";

                    var exists = await _context.Animes.FirstOrDefaultAsync(s => s.Img == fileName);
                    if (exists == null)
                    {
                        unique = true;
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
            string uploadsFolder = Path.Combine(_env.WebRootPath, "img/Anime_imgs");
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

    }
}
