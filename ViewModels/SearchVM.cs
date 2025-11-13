using MovieHall.Models;
using MovieHall.SaveModels;

namespace MovieHall.ViewModels
{
    public class SearchVM
    {
        public string Type { get; set; }
        public string? SearchString { get; set; }


        public List<int>? FilterGenres { get; set; }
        public int? FilterDate { get; set; }
        public bool FilterParents { get; set; }

        public IEnumerable<SearchViewInfos> ItemInfos { get; set; } = Enumerable.Empty<SearchViewInfos>();

        //only view
        public List<Genre> Genres { get; set; }

        // Anime
        public string? FilterContry { get; set; }
        public string? FilterWatchWitch { get; set; }
        public IEnumerable<Anime>? Animes { get; set; }


        // Movie
        public int? FilterFSK { get; set; }
        public IEnumerable<Movie>? Movies { get; set; }


    }
}
