using MovieHall.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieHall.SaveModel
{
    public class SaveAnime
    {
        public int Id { get; set; }
        public int Top { get; set; }

        [Required(ErrorMessage = "Name ist ein Pflichtfeld.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Orginal Name ist ein Pflichtfeld.")]
        public string? Orginal_Name { get; set; }
        public string? Description { get; set; }
        public int Buy { get; set; }
        public IFormFile? Img { get; set; }
        public string? ImgPath { get; set; }
        public string? TempImgPath { get; set; }

        public string? Link { get; set; }
        public DateTime? ReleaseDate { get; set; }

        [Required(ErrorMessage = "Jahr ist ein Pflichtfeld.")]
        public int? ReleaseYear { get; set; }

        [Required(ErrorMessage = "Monat ist ein Pflichtfeld.")]
        public int? ReleaseMonth { get; set; }

        [Required(ErrorMessage = "Episoden ist ein Pflichtfeld.")]
        public int? Episodes { get; set; }

        [Required(ErrorMessage = "Sprache ist ein Pflichtfeld.")]
        public List<string> Language { get; set; }
        public int WhatTimes { get; set; }

        [Required(ErrorMessage = "Land ist ein Pflichtfeld.")]
        public string? Country { get; set; }


        // Fortsetzung (Verweis auf einen anderen Movie)
        public int? ParentId { get; set; }
        [ForeignKey("ParentId")]
        public Anime? Parent { get; set; }

        // Viele-zu-viele
        [Required(ErrorMessage = "Genres ist ein Pflichtfeld.")]
        public List<int> SelectedGenreIds { get; set; }
        public List<int> SelectedWatchedWithIds { get; set; } = new();

        // Viele-zu-viele
        public List<AnimeGenre> AnimeGenres { get; set; } = new();
        public List<AnimeWatchedWith> AnimeWatchedWiths { get; set; } = new();
    }
}
