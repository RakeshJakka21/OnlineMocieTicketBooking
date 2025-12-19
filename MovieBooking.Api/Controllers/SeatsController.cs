
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieBooking.Api.Data;

namespace MovieBooking.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeatsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public SeatsController(ApplicationDbContext db) { _db = db; }

        [HttpGet("screening/{screeningId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetByScreening(int screeningId)
        {
            var exists = await _db.Screenings.AnyAsync(s => s.Id == screeningId);
            if (!exists) return NotFound();
            var seats = await _db.Seats.Where(s => s.ScreeningId == screeningId)
                .OrderBy(s => s.Row).ThenBy(s => s.Number)
                .Select(s => new { s.Id, s.Row, s.Number, s.IsBooked })
                .ToListAsync();
            return Ok(seats);
        }
    }
}
