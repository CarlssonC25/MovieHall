using MovieHall.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieHall.SaveModel
{
    public class SaveAnime
    {
        public int Id { get; set; }

        [Required]
        public int Top { get; set; }

        [Required]
        public string Name { get; set; }
        public string? Orginal_Name { get; set; }
        public string? Description { get; set; }
        public int? Buy { get; set; }
        public IFormFile? Img { get; set; }
        public string? ImgPath { get; set; }
        public string? Link { get; set; }
        public DateTime ReleaseDate { get; set; }
        public int? Episodes { get; set; }
        public string Language { get; set; }

        // Fortsetzung (Verweis auf einen anderen Movie)
        public int? ParentId { get; set; }
        [ForeignKey("ParentId")]
        public Anime? Parent { get; set; }

        // Viele-zu-viele
        public List<int> SelectedGenreIds { get; set; } = new();
        public List<int> SelectedWatchedWithIds { get; set; } = new();

        // Viele-zu-viele
        public List<AnimeGenre> AnimeGenres { get; set; } = new();
        public List<AnimeWatchedWith> AnimeWatchedWiths { get; set; } = new();
    }
}
