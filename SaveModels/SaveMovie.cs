using MovieHall.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieHall.SaveModel
{
    public class SaveMovie
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public int Buy { get; set; }
        public string? Description { get; set; }
        public IFormFile? Img { get; set; }
        public string? ImgPath { get; set; }
        public int FSK { get; set; }
        public bool Favorit { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string? Link { get; set; }
        public string Language { get; set; }


        // Fortsetzung (Verweis auf einen anderen Movie)
        public int? ParentId { get; set; }
        [ForeignKey("ParentId")]
        public Movie? Parent { get; set; }


        // Viele-zu-viele
        public List<int> SelectedGenreIds { get; set; } = new();
        public List<int> SelectedWatchedWithIds { get; set; } = new();


        // Viele-zu-viele
        public List<MovieGenre> MovieGenres { get; set; } = new();
        public List<MovieWatchedWith> MovieWatchedWiths { get; set; } = new();
    }
}
