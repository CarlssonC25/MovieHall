using MovieHall.Models;

namespace MovieHall.ViewModels
{
    public class MovieVM
    {
        public IEnumerable<Movie> Movies { get; set; } = new List<Movie>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string? CurrentFilter { get; set; }

        public int MovieSume { get; set; }
        public List<MovieNoteVM>? Notes { get; set; }
    }
}
