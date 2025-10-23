using System.ComponentModel.DataAnnotations.Schema;

namespace MovieHall.Models
{
    public class MovieNote
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public string Comment { get; set; }

        [ForeignKey("MovieId")]
        public Movie Movie { get; set; }
    }
}
