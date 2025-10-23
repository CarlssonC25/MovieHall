using MovieHall.Models;

namespace MovieHall.ViewModels
{
    public class SearchVM
    {
        public string Type { get; set; }
        public string? SearchString { get; set; }


        public List<int>? FilterGenres { get; set; }
        public int? FilterDate { get; set; }
        public bool FilterParents { get; set; }

        //only view
        public List<Genre> Genres { get; set; }

        // Anime
        public string? FilterContry { get; set; }
        public string? FilterWatchWitch { get; set; }
        public List<Anime>? Animes { get; set; }


        // Movie
        public int? FilterFSK { get; set; }
        public List<Movie>? Movies { get; set; }


    }
}
