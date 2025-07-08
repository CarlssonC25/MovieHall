namespace MovieHall.Models
{
    public class AnimeWatchedWith
    {
        public int AnimeId { get; set; }
        public Anime Anime { get; set; }

        public int WatchedWithId { get; set; }
        public WatchedWith WatchedWith { get; set; }
    }
}
