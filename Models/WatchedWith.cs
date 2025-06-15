namespace MovieHall.Models
{
    public class WatchedWith
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<MovieWatchedWith> MovieWatchedWiths { get; set; } = new();
    }
}
