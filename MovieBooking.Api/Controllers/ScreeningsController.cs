
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieBooking.Api.Data;
using MovieBooking.Api.Models;

namespace MovieBooking.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScreeningsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public ScreeningsController(ApplicationDbContext db) { _db = db; }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Screening>>> Get() => await _db.Screenings.AsNoTracking().ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<Screening>> GetById(int id)
        {
            var entity = await _db.Screenings.FindAsync(id);
            return entity is null ? NotFound() : entity;
        }

        [HttpPost]
        public async Task<ActionResult<Screening>> Create(Screening entity)
        {
            if (!await _db.Movies.AnyAsync(m => m.Id == entity.MovieId) || !await _db.Theaters.AnyAsync(t => t.Id == entity.TheaterId))
                return BadRequest("Invalid MovieId or TheaterId");
            _db.Screenings.Add(entity);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Screening entity)
        {
            if (id != entity.Id) return BadRequest();
            _db.Entry(entity).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _db.Screenings.FindAsync(id);
            if (entity is null) return NotFound();
            _db.Screenings.Remove(entity);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // JOIN: Screening details with Movie + Theater
        [HttpGet("details")]
        public async Task<ActionResult<IEnumerable<object>>> GetDetails()
        {
            var query = await _db.Screenings
                .Include(s => s.Movie)
                .Include(s => s.Theater)
                .Select(s => new {
                    ScreeningId = s.Id,
                    MovieId = s.MovieId,
                    Movie = s.Movie!.Title,
                    Theater = s.Theater!.Name,
                    Location = s.Theater!.Location,
                    StartTime = s.StartTime,
                    Price = s.Price
                }).ToListAsync();
            return Ok(query);
        }
    }
}
