using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieHall.Data;
using MovieHall.Models;
using MovieHall.ViewModels;

namespace MovieHall.Controllers
{
    public class SettingsController : Controller
    {
        private readonly AppDbContext _context;
        public SettingsController(AppDbContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index()
        {
            var vm = new SettingsPageVM
            {
                Genres = await _context.Genre.OrderBy(g => g.Name).ToListAsync(),
                Settings = await _context.Settings.ToListAsync(),
                WatchedWiths = await _context.WatchedWith.ToListAsync()

            };
            return View(vm);
        }

        /* ----- Genre ----- */
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
    }
}
