namespace MovieHall.Models
{
    public class MovieWatchedWith
    {
        public int MovieId { get; set; }
        public Movie Movie { get; set; }

        public int WatchedWithId { get; set; }
        public WatchedWith WatchedWith { get; set; }
    }
}
