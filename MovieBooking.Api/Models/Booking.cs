
namespace MovieBooking.Api.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int ScreeningId { get; set; }
        public int CustomerId { get; set; }
        public int Seats { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime BookingTime { get; set; }

        public Screening? Screening { get; set; }
        public Customer? Customer { get; set; }
        public ICollection<BookingSeat> BookingSeats { get; set; } = new List<BookingSeat>();
    }
}
