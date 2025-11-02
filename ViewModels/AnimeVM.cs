using MovieHall.Models;

namespace MovieHall.ViewModels
{
    public class AnimeVM
    {
        public IEnumerable<Anime> Animes { get; set; } = new List<Anime>();
        public IEnumerable<AnimeViewInfos> AnimeInfos { get; set; } = new List<AnimeViewInfos>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string? RankFilter { get; set; }
        public string? CountryFilter { get; set; }
        public string? LanguageFilter { get; set; }

        public int AnimeSum { get; set; }
        public int AnimeEpSum { get; set; }
        public string AnimeTime { get; set; }
        public string WatchTime { get; set; }

        public List<AnimeNoteVM>? Notes { get; set; }
    }
}
