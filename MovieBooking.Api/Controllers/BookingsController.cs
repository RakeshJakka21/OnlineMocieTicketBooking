using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieBooking.Api.Data;
using MovieBooking.Api.Models;
using System.Text.Json;

namespace MovieBooking.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public BookingsController(
            ApplicationDbContext db,
            IHttpClientFactory httpClientFactory,
            IConfiguration config)
        {
            _db = db;
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Booking>>> Get()
            => await _db.Bookings.AsNoTracking().ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<Booking>> GetById(int id)
        {
            var entity = await _db.Bookings.FindAsync(id);
            return entity is null ? NotFound() : entity;
        }

        public class BookingCreateDto
        {
            public int ScreeningId { get; set; }
            public int CustomerId { get; set; }
            public List<int> SeatIds { get; set; } = new();
            public string? PromoCode { get; set; }
        }

        private class PromoResponse
        {
            public bool Valid { get; set; }
            public int Percent { get; set; }
        }

        [HttpPost]
        public async Task<ActionResult<Booking>> Create([FromBody] BookingCreateDto dto)
        {
            if (dto.SeatIds.Count == 0)
                return BadRequest("At least one seat must be selected");

            var screening = await _db.Screenings
                .Include(s => s.Theater)
                .Include(s => s.Movie)
                .FirstOrDefaultAsync(s => s.Id == dto.ScreeningId);

            var customer = await _db.Customers.FindAsync(dto.CustomerId);

            if (screening is null || customer is null)
                return BadRequest("Invalid ScreeningId or CustomerId");

            var seats = await _db.Seats
                .Where(s => dto.SeatIds.Contains(s.Id) &&
                            s.ScreeningId == dto.ScreeningId)
                .ToListAsync();

            if (seats.Count != dto.SeatIds.Count)
                return BadRequest("Invalid seat selection");

            if (seats.Any(s => s.IsBooked))
                return Conflict("One or more selected seats are already booked");

            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var booking = new Booking
                {
                    ScreeningId = dto.ScreeningId,
                    CustomerId = dto.CustomerId,
                    Seats = dto.SeatIds.Count,
                    TotalPrice = dto.SeatIds.Count * screening.Price,
                    BookingTime = DateTime.UtcNow
                };

                // Promo service
                var promoBase = _config["PromoServiceBaseUrl"];
                if (!string.IsNullOrWhiteSpace(dto.PromoCode) &&
                    !string.IsNullOrWhiteSpace(promoBase))
                {
                    var client = _httpClientFactory.CreateClient();
                    client.BaseAddress = new Uri(promoBase);

                    var resp = await client.GetAsync(
                        $"/api/promos/validate?code={Uri.EscapeDataString(dto.PromoCode)}");

                    if (resp.IsSuccessStatusCode)
                    {
                        var json = await resp.Content.ReadAsStringAsync();
                        var promo = JsonSerializer.Deserialize<PromoResponse>(json);

                        if (promo?.Valid == true && promo.Percent > 0)
                        {
                            var discount = booking.TotalPrice * promo.Percent / 100M;
                            booking.TotalPrice -= discount;
                        }
                    }
                }

                _db.Bookings.Add(booking);
                await _db.SaveChangesAsync();

                foreach (var seat in seats)
                {
                    seat.IsBooked = true;
                    _db.BookingSeats.Add(new BookingSeat
                    {
                        BookingId = booking.Id,
                        SeatId = seat.Id
                    });
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return CreatedAtAction(nameof(GetById),
                    new { id = booking.Id }, booking);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _db.Bookings.FindAsync(id);
            if (entity is null) return NotFound();

            var bookedSeats = await _db.BookingSeats
                .Where(bs => bs.BookingId == id)
                .Include(bs => bs.Seat)
                .ToListAsync();

            foreach (var bs in bookedSeats)
            {
                if (bs.Seat != null)
                    bs.Seat.IsBooked = false;
            }

            _db.BookingSeats.RemoveRange(bookedSeats);
            _db.Bookings.Remove(entity);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
