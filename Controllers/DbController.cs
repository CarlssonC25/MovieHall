using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieHall.Data;
using MovieHall.Models;
using System.IO.Compression;
using System.Text;

namespace MovieHall.Controllers
{
    public class DbController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public DbController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> ExportAll()
        {
            // Speicherort im Arbeitsspeicher (MemoryStream)
            var memoryStream = new MemoryStream();

            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                // --- Hilfsfunktion CSV Export ---
                async Task ExportToCsv<T>(IEnumerable<T> items, string fileName)
                {
                    var props = typeof(T).GetProperties();
                    var csv = new StringBuilder();

                    // Header
                    csv.AppendLine(string.Join(",", props.Select(p => p.Name)));

                    // Rows
                    foreach (var item in items)
                    {
                        var values = props.Select(p => (p.GetValue(item) ?? "").ToString());
                        csv.AppendLine(string.Join(",", values));
                    }

                    var entry = archive.CreateEntry($"CSV/{fileName}");
                    using var writer = new StreamWriter(entry.Open());
                    await writer.WriteAsync(csv.ToString());
                }

                // --- Export Tabellen ---
                await ExportToCsv(await _context.Animes.ToListAsync(), "Animes.csv");
                await ExportToCsv(await _context.Movies.ToListAsync(), "Movies.csv");
                await ExportToCsv(await _context.Genre.ToListAsync(), "Genres.csv");
                await ExportToCsv(await _context.Settings.ToListAsync(), "Settings.csv");
                await ExportToCsv(await _context.WatchedWith.ToListAsync(), "WatchedWith.csv");
                await ExportToCsv(await _context.MovieGenres.ToListAsync(), "MovieGenres.csv");
                await ExportToCsv(await _context.MovieWatchedWiths.ToListAsync(), "MovieWatchedWiths.csv");
                await ExportToCsv(await _context.AnimeGenres.ToListAsync(), "AnimeGenres.csv");
                await ExportToCsv(await _context.AnimeWatchedWiths.ToListAsync(), "AnimeWatchedWiths.csv");

                // --- Export Bilder ---
                string[] folders = { "Anime_imgs", "Movie_imgs" };

                foreach (var folder in folders)
                {
                    var path = Path.Combine(_env.WebRootPath, "img", folder);
                    if (Directory.Exists(path))
                    {
                        foreach (var file in Directory.GetFiles(path))
                        {
                            var entryPath = $"{folder}/{Path.GetFileName(file)}";
                            archive.CreateEntryFromFile(file, entryPath);
                        }
                    }
                }
            }

            memoryStream.Position = 0;
            var fileName = $"MovieHallSaves_{DateTime.Now:yyyyMMdd_HHmmss}.zip";
            return File(memoryStream, "application/zip", fileName);
        }

        [HttpPost]
        public async Task<IActionResult> ImportAll(IFormFile zipFile)
        {
            if (zipFile == null || zipFile.Length == 0)
                return BadRequest("Keine Datei hochgeladen.");

            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPath);

            var zipPath = Path.Combine(tempPath, "import.zip");
            using (var stream = new FileStream(zipPath, FileMode.Create))
            {
                await zipFile.CopyToAsync(stream);
            }

            ZipFile.ExtractToDirectory(zipPath, tempPath);

            var csvPath = Path.Combine(tempPath, "CSV");

            // 🧹 Datenbank leeren
            _context.AnimeWatchedWiths.RemoveRange(_context.AnimeWatchedWiths);
            _context.AnimeGenres.RemoveRange(_context.AnimeGenres);
            _context.MovieWatchedWiths.RemoveRange(_context.MovieWatchedWiths);
            _context.MovieGenres.RemoveRange(_context.MovieGenres);
            _context.Animes.RemoveRange(_context.Animes);
            _context.Movies.RemoveRange(_context.Movies);
            _context.Genre.RemoveRange(_context.Genre);
            _context.WatchedWith.RemoveRange(_context.WatchedWith);
            _context.Settings.RemoveRange(_context.Settings);
            await _context.SaveChangesAsync();

            // 📑 Hilfsfunktion CSV lesen
            List<string[]> ReadCsv(string file)
            {
                return System.IO.File.ReadAllLines(file, Encoding.UTF8)
                    .Skip(1)
                    .Where(l => !string.IsNullOrWhiteSpace(l))
                    .Select(l => l.Split(','))
                    .ToList();
            }

            // --- Genres ---
            var genreFile = Path.Combine(csvPath, "Genres.csv");
            if (System.IO.File.Exists(genreFile))
            {
                foreach (var row in ReadCsv(genreFile))
                {
                    _context.Genre.Add(new Genre
                    {
                        Id = int.Parse(row[0]),
                        Name = row[1],
                        Belonging_to = row[2]
                    });
                }
            }

            // --- Settings ---
            var settingsFile = Path.Combine(csvPath, "Settings.csv");
            if (System.IO.File.Exists(settingsFile))
            {
                foreach (var row in ReadCsv(settingsFile))
                {
                    _context.Settings.Add(new Setting
                    {
                        Id = int.Parse(row[0]),
                        SettingName = row[1],
                        Comment = row[2]
                    });
                }
            }

            // --- WatchedWith ---
            var watchedFile = Path.Combine(csvPath, "WatchedWith.csv");
            if (System.IO.File.Exists(watchedFile))
            {
                foreach (var row in ReadCsv(watchedFile))
                {
                    _context.WatchedWith.Add(new WatchedWith
                    {
                        Id = int.Parse(row[0]),
                        Name = row[1]
                    });
                }
            }

            // --- Animes ---
            var animeFile = Path.Combine(csvPath, "Animes.csv");
            if (System.IO.File.Exists(animeFile))
            {
                foreach (var row in ReadCsv(animeFile))
                {
                    _context.Animes.Add(new Anime
                    {
                        Id = int.Parse(row[0]),
                        Top = int.Parse(row[1]),
                        Name = row[2],
                        Orginal_Name = row[3],
                        Description = row[4],
                        Buy = int.Parse(row[5]),
                        Img = row[6],
                        Link = row[7],
                        ReleaseDate = DateTime.Parse(row[8]),
                        Episodes = string.IsNullOrEmpty(row[9]) ? null : int.Parse(row[9]),
                        Language = row[10],
                        WhatTimes = int.Parse(row[11]),
                        Country = row[12],
                        ParentId = string.IsNullOrEmpty(row[13]) ? null : int.Parse(row[13])
                    });
                }
            }

            // --- Movies ---
            var movieFile = Path.Combine(csvPath, "Movies.csv");
            if (System.IO.File.Exists(movieFile))
            {
                foreach (var row in ReadCsv(movieFile))
                {
                    _context.Movies.Add(new Movie
                    {
                        Id = int.Parse(row[0]),
                        Name = row[1],
                        Buy = int.Parse(row[2]),
                        Description = row[3],
                        Img = row[4],
                        FSK = int.Parse(row[5]),
                        Favorit = bool.TryParse(row[6], out var fav) ? fav : false,
                        ReleaseDate = DateTime.Parse(row[7]),
                        Link = row[8],
                        Language = row[9],
                        ParentId = string.IsNullOrEmpty(row[10]) ? null : int.Parse(row[10])
                    });
                }
            }

            await _context.SaveChangesAsync();

            // --- Relationen ---
            var movieGenreFile = Path.Combine(csvPath, "MovieGenre.csv");
            if (System.IO.File.Exists(movieGenreFile))
            {
                foreach (var row in ReadCsv(movieGenreFile))
                {
                    _context.MovieGenres.Add(new MovieGenre
                    {
                        MovieId = int.Parse(row[0]),
                        GenreId = int.Parse(row[2])
                    });
                }
            }

            var movieWatchedFile = Path.Combine(csvPath, "MovieWatchedWith.csv");
            if (System.IO.File.Exists(movieWatchedFile))
            {
                foreach (var row in ReadCsv(movieWatchedFile))
                {
                    _context.MovieWatchedWiths.Add(new MovieWatchedWith
                    {
                        MovieId = int.Parse(row[0]),
                        WatchedWithId = int.Parse(row[2])
                    });
                }
            }

            var animeGenreFile = Path.Combine(csvPath, "AnimeGenre.csv");
            if (System.IO.File.Exists(animeGenreFile))
            {
                foreach (var row in ReadCsv(animeGenreFile))
                {
                    _context.AnimeGenres.Add(new AnimeGenre
                    {
                        AnimeId = int.Parse(row[0]),
                        GenreId = int.Parse(row[2])
                    });
                }
            }

            var animeWatchedFile = Path.Combine(csvPath, "AnimeWatchedWith.csv");
            if (System.IO.File.Exists(animeWatchedFile))
            {
                foreach (var row in ReadCsv(animeWatchedFile))
                {
                    _context.AnimeWatchedWiths.Add(new AnimeWatchedWith
                    {
                        AnimeId = int.Parse(row[0]),
                        WatchedWithId = int.Parse(row[2])
                    });
                }
            }

            await _context.SaveChangesAsync();

            // --- Bilder kopieren ---
            string[] folders = { "Anime_imgs", "Movie_imgs" };
            foreach (var folder in folders)
            {
                var sourcePath = Path.Combine(tempPath, folder);
                var targetPath = Path.Combine(_env.WebRootPath, "img", folder);

                if (!Directory.Exists(targetPath))
                    Directory.CreateDirectory(targetPath);

                if (Directory.Exists(sourcePath))
                {
                    foreach (var file in Directory.GetFiles(sourcePath))
                    {
                        var fileName = Path.GetFileName(file);
                        var destFile = Path.Combine(targetPath, fileName);
                        System.IO.File.Copy(file, destFile, true);
                    }
                }
            }

            Directory.Delete(tempPath, true);

            return RedirectToAction("Index", "Settings");
        }
    }
}
