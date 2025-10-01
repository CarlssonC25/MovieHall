using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieHall.Models
{
    public class Anime
    {
        public int Id { get; set; }

        [Required]
        public int Top { get; set; }

        [Required]
        public string Name { get; set; }
        public string? Orginal_Name { get; set; }
        public string? Description { get; set; }
        public int Buy { get; set; }
        public string Img { get; set; }
        public string? Link { get; set; }
        public DateTime ReleaseDate { get; set; }
        public int? Episodes { get; set; }
        public string Language { get; set; }
        public int WhatTimes { get; set; }
        public string? Country { get; set; }


        // Fortsetzung (Verweis auf einen anderen Movie)
        public int? ParentId { get; set; }
        [ForeignKey("ParentId")]
        public Anime? Parent { get; set; }


        // Viele-zu-viele
        public List<AnimeGenre> AnimeGenres { get; set; } = new();
        public List<AnimeWatchedWith> AnimeWatchedWiths { get; set; } = new();
    }
}
