
namespace MovieBooking.Api.Models
{
    public class Seat
    {
        public int Id { get; set; }
        public int ScreeningId { get; set; }
        public int Row { get; set; }
        public int Number { get; set; }
        public bool IsBooked { get; set; }
        public Screening? Screening { get; set; }
        public ICollection<BookingSeat> BookingSeats { get; set; } = new List<BookingSeat>();
    }
}
