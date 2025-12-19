
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieBooking.Api.Data;
using MovieBooking.Api.Models;

namespace MovieBooking.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MoviesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public MoviesController(ApplicationDbContext db) { _db = db; }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Movie>>> Get() => await _db.Movies.AsNoTracking().ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<Movie>> GetById(int id)
        {
            var movie = await _db.Movies.FindAsync(id);
            return movie is null ? NotFound() : movie;
        }

        [HttpPost]
        public async Task<ActionResult<Movie>> Create(Movie movie)
        {
            _db.Movies.Add(movie);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = movie.Id }, movie);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Movie movie)
        {
            if (id != movie.Id) return BadRequest();
            _db.Entry(movie).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var movie = await _db.Movies.FindAsync(id);
            if (movie is null) return NotFound();
            _db.Movies.Remove(movie);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
