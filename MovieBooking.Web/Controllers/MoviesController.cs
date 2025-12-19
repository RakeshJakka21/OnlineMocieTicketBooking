
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace MovieBooking.Web.Controllers
{
    public class MoviesController : Controller
    {
        private readonly IHttpClientFactory _httpFactory;
        public MoviesController(IHttpClientFactory httpFactory) { _httpFactory = httpFactory; }

        public async Task<IActionResult> Index()
        {
            var client = _httpFactory.CreateClient("Api");
            var resp = await client.GetAsync("/api/movies");
            resp.EnsureSuccessStatusCode();
            var json = await resp.Content.ReadAsStringAsync();
            var movies = JsonSerializer.Deserialize<List<MovieDto>>(json, new JsonSerializerOptions{PropertyNameCaseInsensitive=true})!;
            return View(movies);
        }

        public async Task<IActionResult> Details(int id)
        {
            var client = _httpFactory.CreateClient("Api");
            var resp = await client.GetAsync($"/api/movies/{id}");
            if (!resp.IsSuccessStatusCode) return NotFound();
            var json = await resp.Content.ReadAsStringAsync();
            var movie = JsonSerializer.Deserialize<MovieDto>(json, new JsonSerializerOptions{PropertyNameCaseInsensitive=true})!;
            return View(movie);
        }

        [HttpGet]
        public IActionResult Create() => View(new MovieDto());

        [HttpPost]
        public async Task<IActionResult> Create(MovieDto dto)
        {
            var client = _httpFactory.CreateClient("Api");
            var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
            var resp = await client.PostAsync("/api/movies", content);
            if (resp.IsSuccessStatusCode) return RedirectToAction(nameof(Index));
            ModelState.AddModelError("", "Create failed");
            return View(dto);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var client = _httpFactory.CreateClient("Api");
            var resp = await client.GetAsync($"/api/movies/{id}");
            if (!resp.IsSuccessStatusCode) return NotFound();
            var json = await resp.Content.ReadAsStringAsync();
            var movie = JsonSerializer.Deserialize<MovieDto>(json, new JsonSerializerOptions{PropertyNameCaseInsensitive=true})!;
            return View(movie);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, MovieDto dto)
        {
            var client = _httpFactory.CreateClient("Api");
            var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
            var resp = await client.PutAsync($"/api/movies/{id}", content);
            if (resp.IsSuccessStatusCode) return RedirectToAction(nameof(Index));
            ModelState.AddModelError("", "Update failed");
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var client = _httpFactory.CreateClient("Api");
            await client.DeleteAsync($"/api/movies/{id}");
            return RedirectToAction(nameof(Index));
        }
    }

    public class MovieDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DurationMinutes { get; set; }
        public decimal Rating { get; set; }
        public string? PosterPath { get; set; }
    }
}
