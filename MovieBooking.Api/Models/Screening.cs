
namespace MovieBooking.Api.Models
{
    public class Screening
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public int TheaterId { get; set; }
        public DateTime StartTime { get; set; }
        public decimal Price { get; set; }

        public Movie? Movie { get; set; }
        public Theater? Theater { get; set; }
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    }
}
