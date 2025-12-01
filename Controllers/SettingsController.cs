using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieHall.Data;
using MovieHall.Models;
using MovieHall.SaveModel;
using MovieHall.SaveModels;
using MovieHall.ViewModels;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace MovieHall.Controllers
{
    public class SettingsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        public SettingsController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }


        public async Task<IActionResult> Index()
        {
            var RawCustomLinks = await _context.Settings.Where(s => s.SettingName.Contains("Link")).ToListAsync();
            var CustomLinks = new List<SaveCustomLink>();
            foreach (var link in RawCustomLinks)
            {
                var LinkNameImg = link.SettingName.Split("|");
                var LinkSpace = link.Comment.Split("|");
                CustomLinks.Add(new SaveCustomLink()
                {
                    Id = link.Id,
                    Name = LinkNameImg[1],
                    ImgLink = LinkNameImg[2],
                    Link = LinkSpace[0],
                    Space = LinkSpace[1]
                });
            }

            var AImg = await _context.Settings.FirstOrDefaultAsync(s => s.SettingName == "AnimeImg");
            var MImg = await _context.Settings.FirstOrDefaultAsync(s => s.SettingName == "MovieImg");

            if (AImg != null && MImg != null)
            {
                ViewData["AnimeImg"] = "/img/Settings_imgs/" + AImg.Comment;
                ViewData["MovieImg"] = "/img/Settings_imgs/" + MImg.Comment;
            }
            else
            {
                ViewData["AnimeImg"] = "/img/test.jpg";
                ViewData["MovieImg"] = "/img/test.jpg";
            }

            var vm = new SettingsPageVM
            {
                Genres = await _context.Genre.OrderBy(g => g.Name).ToListAsync(),
                Settings = await _context.Settings.Where(s => !s.SettingName.Contains("Link")).ToListAsync(),
                WatchedWiths = await _context.WatchedWith.ToListAsync(),
                CustomLink = CustomLinks
            };

            return View(vm);
        }

        /* ---------- Genre ---------- */
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


        /* ---------- Setting ---------- */
        [HttpGet]
        public IActionResult CreateSettingPartial()
            => PartialView("~/Views/Settings/_CreatePartial.cshtml", new Setting());

        // --- ADD ---
        [HttpPost]
        public async Task<IActionResult> CreateSettingPartial(Setting s)
        {
            if (!ModelState.IsValid) return PartialView("~/Views/Settings/_CreatePartial.cshtml", s);
            _context.Add(s);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // --- EDIT ---
        [HttpGet]
        public async Task<IActionResult> EditSettingPartial(int id)
        {
            var s = await _context.Settings.FindAsync(id);
            return s == null ? NotFound() : PartialView("~/Views/Settings/_EditPartial.cshtml", s);
        }

        [HttpPost]
        public async Task<IActionResult> EditSettingPartial(Setting s)
        {
            if (!ModelState.IsValid) return PartialView("~/Views/Settings/_EditPartial.cshtml", s);
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


        /* ---------- WatchedWith ---------- */
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


        /* ---------- Home Imgs (Settings) ---------- */

        [HttpPost]
        public async Task<IActionResult> UploadSettingImage(IFormFile file, string settingName)
        {
            if (file != null && (file.ContentType == "image/jpeg" || file.ContentType == "image/png"))
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "img/Settings_imgs");
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


        /* ---------- Custom Link ---------- */

        [HttpGet]
        public async Task<IActionResult> CreateCustomLinkPartial()
            => PartialView("~/Views/CustomLink/_CreatePartial.cshtml", new SaveCustomLink());

        // --- ADD ---
        [HttpPost]
        public async Task<IActionResult> CreateCustomLinkPartial(SaveCustomLink customLink)
        {
            if (!ModelState.IsValid) 
                return PartialView("~/Views/CustomLink/_CreatePartial.cshtml", customLink);

            var img = "";

            // img
            if (customLink.Img != null && (customLink.Img.ContentType == "image/jpeg" || customLink.Img.ContentType == "image/png"))
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "img/Settings_imgs");
                Directory.CreateDirectory(uploadsFolder);

                string fileName = "";
                var fileNameUnique = false;
                while (!fileNameUnique)
                {
                    string cleanName = Regex.Replace(customLink.Name, @"[^a-zA-Z0-9]", "");
                    fileName = $"{cleanName}{Path.GetExtension(customLink.Img.FileName)}";

                    var animeImg = await _context.Animes.FirstOrDefaultAsync(s => s.Img == fileName);

                    if (animeImg == null)
                    {
                        fileNameUnique = true;
                    }
                }

                img = fileName;

                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await customLink.Img.CopyToAsync(stream);
                }
            }
            else
            {
                return PartialView("~/Views/CustomLink/_CreatePartial.cshtml", customLink);
            }

            var setting = new Setting()
            {
                // SettingName ("Link to filter" | [Name] | [img])
                SettingName = "Link|" + customLink.Name + "|" + img,
                // Comment ("[Link]" | [Belonging_to] | [space filler])
                Comment = customLink.Link + "|" + customLink.Belonging_to + "|" + customLink.Space,
            };

            _context.Add(setting);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // --- DELETE ---
        [HttpPost]
        public async Task<IActionResult> DeleteCustomLinkConfirmed(int id)
        {
            var s = await _context.Settings.FindAsync(id);
            if (s == null) 
                return NotFound();

            var img = s.SettingName.Split("|")[2];
            // Bild löschen
            string uploadsFolder = Path.Combine(_env.WebRootPath, "img/Settings_imgs");
            if (!string.IsNullOrEmpty(img))
            {
                string oldImagePath = Path.Combine(uploadsFolder, img);
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            _context.Settings.Remove(s); 
            await _context.SaveChangesAsync();

            return Ok();
        }


        /* ---------- To Buy List ---------- */
        [HttpGet]
        public async Task<IActionResult> ExportAnimesWithoutBuy()
        {
            var animes = await _context.Animes
                .Where(a => a.Buy == 0)
                .OrderBy(a => a.Top)
                .ToListAsync();

            var grouped = animes
                .GroupBy(a => a.ParentId ?? a.Id)
                .ToList();

            var sb = new StringBuilder();
            foreach (var group in grouped)
            {
                var ordered = group.OrderBy(a => a.ReleaseDate).ToList();
                foreach (var anime in ordered)
                {
                    sb.AppendLine(anime.Name);
                }
                sb.AppendLine();
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/plain", $"Animes_zu_Kaufen_{animes.Count()}.txt");
        }

        [HttpGet]
        public async Task<IActionResult> ExportMoviesWithoutBuy()
        {
            var movies = await _context.Movies
                .Where(m => m.Buy == 0)
                .OrderByDescending(m => m.Favorit)
                .ToListAsync();

            var grouped = movies
                .GroupBy(m => m.ParentId ?? m.Id)
                .ToList();

            var sb = new StringBuilder();
            foreach (var group in grouped)
            {
                var ordered = group.OrderBy(m => m.ReleaseDate).ToList();
                foreach (var movie in ordered)
                {
                    sb.AppendLine(movie.Name);
                }
                sb.AppendLine();
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/plain", $"Movies_zu_Kaufen_{movies.Count()}.txt");
        }

    }
}
