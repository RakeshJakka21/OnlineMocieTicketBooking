
namespace MovieBooking.Api.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DurationMinutes { get; set; }
        public decimal Rating { get; set; }
        public string? PosterPath { get; set; }
        public ICollection<Screening> Screenings { get; set; } = new List<Screening>();
    }
}
