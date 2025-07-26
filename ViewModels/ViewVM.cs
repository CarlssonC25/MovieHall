using MovieHall.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieHall.ViewModels
{
    public class ViewVM
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public int Buy { get; set; }
        public string? Description { get; set; }
        public string Img { get; set; }
        public string? Link { get; set; }
        public string Language { get; set; }
        public DateTime ReleaseDate { get; set; }

        public int? ParentId { get; set; }


        //------------------------- ANIME -------------------------

        public int? Top { get; set; }
        public string? Orginal_Name { get; set; }
        public int? Episodes { get; set; }


        // Parent (Verweis auf Parent Anime)
        public Anime? ParentAnime { get; set; }
        public List<Anime>? ChildAnimes { get; set; }
        
        // Viele-zu-viele
        public List<AnimeGenre>? AnimeGenres { get; set; } = new();
        public List<AnimeWatchedWith>? AnimeWatchedWiths { get; set; } = new();


        //------------------------- Movie -------------------------

        public int? FSK { get; set; }
        public bool? Favorit { get; set; }


        // Parent (Verweis Parent Movie)
        public Movie? ParentMovie { get; set; }
        public List<Movie>? ChildMovies { get; set; }

        // Viele-zu-viele
        public List<MovieGenre>? MovieGenres { get; set; } = new();
        public List<MovieWatchedWith>? MovieWatchedWiths { get; set; } = new();
    }
}
