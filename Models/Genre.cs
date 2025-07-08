namespace MovieHall.Models
{
    public class Genre
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Belonging_to { get; set; }

        public List<MovieGenre> MovieGenres { get; set; } = new();
        public List<AnimeGenre> AnimeGenres { get; set; } = new();
    }
}
