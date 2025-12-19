
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieBooking.Api.Data;
using MovieBooking.Api.Models;

namespace MovieBooking.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TheatersController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public TheatersController(ApplicationDbContext db) { _db = db; }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Theater>>> Get() => await _db.Theaters.AsNoTracking().ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<Theater>> GetById(int id)
        {
            var entity = await _db.Theaters.FindAsync(id);
            return entity is null ? NotFound() : entity;
        }

        [HttpPost]
        public async Task<ActionResult<Theater>> Create(Theater entity)
        {
            _db.Theaters.Add(entity);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Theater entity)
        {
            if (id != entity.Id) return BadRequest();
            _db.Entry(entity).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _db.Theaters.FindAsync(id);
            if (entity is null) return NotFound();
            _db.Theaters.Remove(entity);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
