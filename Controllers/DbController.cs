using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieHall.Data;
using MovieHall.Models;
using System.Globalization;
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
                        var values = props.Select(p =>
                        {
                            var val = (p.GetValue(item) ?? "").ToString();

                            // Kommas ersetzen
                            val = val.Replace(",", "§COMMA§");

                            // Text mit Anführungszeichen oder Zeilenumbrüchen korrekt escapen
                            if (val.Contains('"') || val.Contains('\n') || val.Contains('§'))
                            {
                                val = "\"" + val.Replace("\"", "\"\"") + "\"";
                            }
                            return val;
                        });

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
                await ExportToCsv(await _context.AnimeNotes.ToListAsync(), "AnimeNotes.csv");
                await ExportToCsv(await _context.MovieNotes.ToListAsync(), "MovieNotes.csv");

                // --- Export Bilder ---
                string[] folders = { "Anime_imgs", "Movie_imgs", "Settings_imgs" };

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
        [RequestSizeLimit(100_000_000)] // 100 MB
        public async Task<IActionResult> ImportAll(IFormFile zipFile)
        {
            if (zipFile == null || zipFile.Length == 0)
                return BadRequest("Keine Datei hochgeladen.");

            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPath);
            var zipPath = Path.Combine(tempPath, "import.zip");

            using (var stream = new FileStream(zipPath, FileMode.Create))
                await zipFile.CopyToAsync(stream);

            ZipFile.ExtractToDirectory(zipPath, tempPath);
            var csvPath = Path.Combine(tempPath, "CSV");

            // --- Alles löschen ---
            _context.AnimeNotes.RemoveRange(_context.AnimeNotes);
            _context.MovieNotes.RemoveRange(_context.MovieNotes);
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

            // --- CSV Leser ---
            List<string[]> ReadCsv(string file)
            {
                return System.IO.File.ReadAllLines(file, Encoding.UTF8)
                    .Skip(1)
                    .Where(l => !string.IsNullOrWhiteSpace(l))
                    .Select(l => ParseCsvLine(l))
                    .ToList();
            }

            string[] ParseCsvLine(string line)
            {
                var result = new List<string>();
                var sb = new StringBuilder();
                bool inQuotes = false;

                foreach (char c in line)
                {
                    if (c == '"')
                        inQuotes = !inQuotes;
                    else if (c == ',' && !inQuotes)
                    {
                        result.Add(sb.ToString());
                        sb.Clear();
                    }
                    else
                        sb.Append(c);
                }
                result.Add(sb.ToString());
                return result.ToArray();
            }

            // --- Settings ---
            var oldSettings = new List<Setting>();
            var settingsFile = Path.Combine(csvPath, "Settings.csv");
            if (System.IO.File.Exists(settingsFile))
            {
                foreach (var row in ReadCsv(settingsFile))
                {
                    oldSettings.Add(new Setting
                    {
                        SettingName = row[1],
                        Comment = row[2]
                    });
                }
                _context.Settings.AddRange(oldSettings);
                await _context.SaveChangesAsync();
            }

            // --- Genres ---
            var oldGenreMap = new Dictionary<int, int>();
            var genreFile = Path.Combine(csvPath, "Genres.csv");
            if (System.IO.File.Exists(genreFile))
            {
                foreach (var row in ReadCsv(genreFile))
                {
                    int oldId = int.Parse(row[0]);
                    var g = new Genre { Name = row[1], Belonging_to = row[2] };
                    _context.Genre.Add(g);
                    await _context.SaveChangesAsync();
                    oldGenreMap[oldId] = g.Id;
                }
            }

            // --- WatchedWith ---
            var oldWatchedWithMap = new Dictionary<int, int>();
            var watchedFile = Path.Combine(csvPath, "WatchedWith.csv");
            if (System.IO.File.Exists(watchedFile))
            {
                foreach (var row in ReadCsv(watchedFile))
                {
                    int oldId = int.Parse(row[0]);
                    var w = new WatchedWith { Name = row[1] };
                    _context.WatchedWith.Add(w);
                    await _context.SaveChangesAsync();
                    oldWatchedWithMap[oldId] = w.Id;
                }
            }

            // --- Animes ---
            var oldAnimeMap = new Dictionary<int, int>();
            var animeFile = Path.Combine(csvPath, "Animes.csv");
            var animes = new List<(int OldId, Anime NewAnime, int? OldParentId)>();
            if (System.IO.File.Exists(animeFile))
            {
                foreach (var row in ReadCsv(animeFile))
                {
                    int oldId = int.Parse(row[0]);
                    int? parentOldId = string.IsNullOrEmpty(row[13]) ? (int?)null : int.Parse(row[13]);

                    var a = new Anime
                    {
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
                        Country = row[12]
                    };

                    _context.Animes.Add(a);
                    await _context.SaveChangesAsync();
                    animes.Add((oldId, a, parentOldId));
                    oldAnimeMap[oldId] = a.Id;
                }

                // Jetzt ParentIds setzen
                foreach (var (OldId, NewAnime, OldParentId) in animes)
                {
                    if (OldParentId.HasValue && oldAnimeMap.ContainsKey(OldParentId.Value))
                    {
                        NewAnime.ParentId = oldAnimeMap[OldParentId.Value];
                    }
                }
                await _context.SaveChangesAsync();
            }

            // --- AnimeGenres ---
            var animeGenreFile = Path.Combine(csvPath, "AnimeGenres.csv");
            if (System.IO.File.Exists(animeGenreFile))
            {
                foreach (var row in ReadCsv(animeGenreFile))
                {
                    int oldAnimeId = int.Parse(row[0]);
                    int oldGenreId = int.Parse(row[2]);
                    if (oldAnimeMap.ContainsKey(oldAnimeId) && oldGenreMap.ContainsKey(oldGenreId))
                    {
                        _context.AnimeGenres.Add(new AnimeGenre
                        {
                            AnimeId = oldAnimeMap[oldAnimeId],
                            GenreId = oldGenreMap[oldGenreId]
                        });
                    }
                }
                await _context.SaveChangesAsync();
            }

            // --- AnimeWatchedWiths ---
            var animeWatchedFile = Path.Combine(csvPath, "AnimeWatchedWiths.csv");
            if (System.IO.File.Exists(animeWatchedFile))
            {
                foreach (var row in ReadCsv(animeWatchedFile))
                {
                    int oldAnimeId = int.Parse(row[0]);
                    int oldWatchedWithId = int.Parse(row[2]);
                    if (oldAnimeMap.ContainsKey(oldAnimeId) && oldWatchedWithMap.ContainsKey(oldWatchedWithId))
                    {
                        _context.AnimeWatchedWiths.Add(new AnimeWatchedWith
                        {
                            AnimeId = oldAnimeMap[oldAnimeId],
                            WatchedWithId = oldWatchedWithMap[oldWatchedWithId]
                        });
                    }
                }
                await _context.SaveChangesAsync();
            }

            // --- AnimeNotes ---
            var animeNotesFile = Path.Combine(csvPath, "AnimeNotes.csv");
            if (System.IO.File.Exists(animeNotesFile))
            {
                foreach (var row in ReadCsv(animeNotesFile))
                {
                    int oldAnimeId = int.Parse(row[1]);
                    if (oldAnimeMap.ContainsKey(oldAnimeId))
                    {
                        _context.AnimeNotes.Add(new AnimeNote
                        {
                            AnimeId = oldAnimeMap[oldAnimeId],
                            Comment = row[2]
                        });
                    }
                }
                await _context.SaveChangesAsync();
            }

            // --- Movies ---
            var oldMovieMap = new Dictionary<int, int>();
            var movies = new List<(int OldId, Movie NewMovie, int? OldParentId)>();
            var movieFile = Path.Combine(csvPath, "Movies.csv");
            if (System.IO.File.Exists(movieFile))
            {
                foreach (var row in ReadCsv(movieFile))
                {
                    int oldId = int.Parse(row[0]);
                    int? parentOldId = string.IsNullOrEmpty(row[10]) ? (int?)null : int.Parse(row[10]);

                    var m = new Movie
                    {
                        Name = row[1],
                        Buy = int.Parse(row[2]),
                        Description = row[3],
                        Img = row[4],
                        FSK = int.Parse(row[5]),
                        Favorit = bool.TryParse(row[6], out var fav) && fav,
                        ReleaseDate = DateTime.Parse(row[7]),
                        Link = row[8],
                        Language = row[9]
                    };

                    _context.Movies.Add(m);
                    await _context.SaveChangesAsync();
                    movies.Add((oldId, m, parentOldId));
                    oldMovieMap[oldId] = m.Id;
                }

                // ParentIds setzen
                foreach (var (OldId, NewMovie, OldParentId) in movies)
                {
                    if (OldParentId.HasValue && oldMovieMap.ContainsKey(OldParentId.Value))
                    {
                        NewMovie.ParentId = oldMovieMap[OldParentId.Value];
                    }
                }
                await _context.SaveChangesAsync();
            }

            // --- MovieGenres ---
            var movieGenreFile = Path.Combine(csvPath, "MovieGenres.csv");
            if (System.IO.File.Exists(movieGenreFile))
            {
                foreach (var row in ReadCsv(movieGenreFile))
                {
                    int oldMovieId = int.Parse(row[0]);
                    int oldGenreId = int.Parse(row[2]);
                    if (oldMovieMap.ContainsKey(oldMovieId) && oldGenreMap.ContainsKey(oldGenreId))
                    {
                        _context.MovieGenres.Add(new MovieGenre
                        {
                            MovieId = oldMovieMap[oldMovieId],
                            GenreId = oldGenreMap[oldGenreId]
                        });
                    }
                }
                await _context.SaveChangesAsync();
            }

            // --- MovieWatchedWiths ---
            var movieWatchedFile = Path.Combine(csvPath, "MovieWatchedWiths.csv");
            if (System.IO.File.Exists(movieWatchedFile))
            {
                foreach (var row in ReadCsv(movieWatchedFile))
                {
                    int oldMovieId = int.Parse(row[0]);
                    int oldWatchedWithId = int.Parse(row[2]);
                    if (oldMovieMap.ContainsKey(oldMovieId) && oldWatchedWithMap.ContainsKey(oldWatchedWithId))
                    {
                        _context.MovieWatchedWiths.Add(new MovieWatchedWith
                        {
                            MovieId = oldMovieMap[oldMovieId],
                            WatchedWithId = oldWatchedWithMap[oldWatchedWithId]
                        });
                    }
                }
                await _context.SaveChangesAsync();
            }

            // --- MovieNotes ---
            var movieNotesFile = Path.Combine(csvPath, "MovieNotes.csv");
            if (System.IO.File.Exists(movieNotesFile))
            {
                foreach (var row in ReadCsv(movieNotesFile))
                {
                    int oldMovieId = int.Parse(row[1]);
                    if (oldMovieMap.ContainsKey(oldMovieId))
                    {
                        _context.MovieNotes.Add(new MovieNote
                        {
                            MovieId = oldMovieMap[oldMovieId],
                            Comment = row[2]
                        });
                    }
                }
                await _context.SaveChangesAsync();
            }

            // --- Bilder kopieren ---
            string[] folders = { "Anime_imgs", "Movie_imgs", "Settings_imgs" };
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
