using MovieHall.Models;

namespace MovieHall.ViewModels
{
    public class MovieVM
    {
        public IEnumerable<Movie> Movies { get; set; } = new List<Movie>();
        public IEnumerable<MovieViewInfos> MovieInfos { get; set; } = new List<MovieViewInfos>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string? CurrentFilter { get; set; }

        public int MovieSume { get; set; }
        public List<MovieNoteVM>? Notes { get; set; }
    }
}
