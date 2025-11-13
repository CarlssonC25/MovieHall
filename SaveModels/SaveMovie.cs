using MovieHall.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieHall.SaveModel
{
    public class SaveMovie
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name ist ein Pflichtfeld.")]
        public string Name { get; set; }

        public int Buy { get; set; }
        public string? Description { get; set; }
        public IFormFile? Img { get; set; }
        public string? ImgPath { get; set; }
        public string? TempImgPath { get; set; }

        [Required(ErrorMessage = "FSK ist ein Pflichtfeld.")]
        public int FSK { get; set; }
        public bool Favorit { get; set; }
        public DateTime? ReleaseDate { get; set; }

        [Required(ErrorMessage = "Name ist ein Pflichtfeld.")]
        public int? ReleaseYear { get; set; }
        public string? Link { get; set; }

        [Required(ErrorMessage = "Sprache ist ein Pflichtfeld.")]
        public List<string> Language { get; set; }


        // Fortsetzung (Verweis auf einen anderen Movie)
        public int? ParentId { get; set; }
        [ForeignKey("ParentId")]
        public Movie? Parent { get; set; }


        // Viele-zu-viele
        [Required(ErrorMessage = "Genre ist ein Pflichtfeld.")]
        public List<int> SelectedGenreIds { get; set; }
        public List<int> SelectedWatchedWithIds { get; set; } = new();


        // Viele-zu-viele
        public List<MovieGenre> MovieGenres { get; set; } = new();
        public List<MovieWatchedWith> MovieWatchedWiths { get; set; } = new();
    }
}
