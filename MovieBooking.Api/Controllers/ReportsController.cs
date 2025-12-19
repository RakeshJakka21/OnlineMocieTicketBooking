
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieBooking.Api.Data;

namespace MovieBooking.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public ReportsController(ApplicationDbContext db) { _db = db; }

        [HttpGet("bookings-by-movie")]
        public async Task<ActionResult<IEnumerable<object>>> GetBookingsByMovie()
        {
            var result = await _db.Bookings
                .Include(b => b.Screening)!.ThenInclude(s => s.Movie)
                .GroupBy(b => b.Screening!.Movie!.Title)
                .Select(g => new { Movie = g.Key, Bookings = g.Count() })
                .OrderByDescending(r => r.Bookings)
                .ToListAsync();
            return Ok(result);
        }
    }
}
