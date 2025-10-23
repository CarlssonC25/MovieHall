using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieHall.Data;
using MovieHall.Models;
using MovieHall.SaveModel;
using MovieHall.ViewModels;
using System.Text.RegularExpressions;

namespace MovieHall.Controllers
{
    public class NoteController : Controller
    {
        private readonly AppDbContext _context;
        public NoteController(AppDbContext context)
        {
            _context = context;
        }

        // ------------------------ Anime ------------------------
        // ------ ADD ------
        [HttpGet]
        public async Task<IActionResult> CreateAnimeNotePartial(int animeId, string img)
        {
            var note = new AnimeNoteVM()
            {
                AnimeId = animeId,
                Img = img
            };

            return PartialView("~/Views/Notes/_CreateAnimePartial.cshtml", note);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAnimeNotePartial(AnimeNoteVM svNote)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("~/Views/Notes/_CreateAnimePartial.cshtml", svNote);
            }

            var note = new AnimeNote
            {
                AnimeId = svNote.AnimeId,
                Comment = svNote.Comment
            };

            _context.AnimeNotes.Add(note);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }


        // ------ DELETE ------
        [HttpPost]
        public async Task<IActionResult> DeleteAnimeNoteConfirmed(int id)
        {
            var note = await _context.AnimeNotes
                .FirstOrDefaultAsync(m => m.Id == id);

            if (note == null) return NotFound();

            _context.AnimeNotes.Remove(note);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // ------------------------ Movie ------------------------
        // ------ ADD ------
        [HttpGet]
        public async Task<IActionResult> CreateMovieNotePartial(int movieId, string img)
        {
            var note = new MovieNoteVM()
            {
                MovieId = movieId,
                Img = img
            };

            return PartialView("~/Views/Notes/_CreateMoviePartial.cshtml", note);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMovieNotePartial(MovieNoteVM svNote)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("~/Views/Notes/_CreateMoviePartial.cshtml", svNote);
            }

            var note = new MovieNote
            {
                MovieId = svNote.MovieId,
                Comment = svNote.Comment
            };

            _context.MovieNotes.Add(note);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }


        // ------ DELETE ------
        [HttpPost]
        public async Task<IActionResult> DeleteMovieNoteConfirmed(int id)
        {
            var note = await _context.MovieNotes
                .FirstOrDefaultAsync(m => m.Id == id);

            if (note == null) return NotFound();

            _context.MovieNotes.Remove(note);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

    }
}
