using MovieHall.Models;

namespace MovieHall.ViewModels
{
    public class AnimeVM
    {
        public IEnumerable<Anime> Animes { get; set; } = new List<Anime>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string? RankFilter { get; set; }
        public string? CountryFilter { get; set; }
        public string? LanguageFilter { get; set; }
    }
}
