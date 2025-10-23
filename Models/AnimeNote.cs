using System.ComponentModel.DataAnnotations.Schema;

namespace MovieHall.Models
{
    public class AnimeNote
    {
        public int Id { get; set; }
        public int AnimeId { get; set; }
        public string Comment { get; set; }

        [ForeignKey("AnimeId")]
        public Anime Anime { get; set; }
    }
}
