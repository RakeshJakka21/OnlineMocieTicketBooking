
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieBooking.Api.Data;
using MovieBooking.Api.Models;

namespace MovieBooking.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public CustomersController(ApplicationDbContext db) { _db = db; }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> Get() => await _db.Customers.AsNoTracking().ToListAsync();
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetById(int id)
        {
            var entity = await _db.Customers.FindAsync(id);
            return entity is null ? NotFound() : entity;
        }
        [HttpPost]
        public async Task<ActionResult<Customer>> Create(Customer entity)
        {
            _db.Customers.Add(entity);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Customer entity)
        {
            if (id != entity.Id) return BadRequest();
            _db.Entry(entity).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _db.Customers.FindAsync(id);
            if (entity is null) return NotFound();
            _db.Customers.Remove(entity);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
