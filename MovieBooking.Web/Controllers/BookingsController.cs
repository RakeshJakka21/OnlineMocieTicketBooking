
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace MovieBooking.Web.Controllers
{
    public class BookingsController : Controller
    {
        private readonly IHttpClientFactory _httpFactory;
        public BookingsController(IHttpClientFactory httpFactory) { _httpFactory = httpFactory; }

        public async Task<IActionResult> SelectSeats(int screeningId)
        {
            var client = _httpFactory.CreateClient("Api");
            var resp = await client.GetAsync($"/api/seats/screening/{screeningId}");
            if (!resp.IsSuccessStatusCode) return NotFound();
            var json = await resp.Content.ReadAsStringAsync();
            var seats = JsonSerializer.Deserialize<List<SeatDto>>(json, new JsonSerializerOptions{PropertyNameCaseInsensitive=true})!;
            ViewBag.ScreeningId = screeningId;
            return View(seats);
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(int screeningId, int customerId, List<int> seatIds, string? promoCode)
        {
            var client = _httpFactory.CreateClient("Api");
            var payload = new BookingCreateDto { ScreeningId = screeningId, CustomerId = customerId, SeatIds = seatIds, PromoCode = promoCode };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var resp = await client.PostAsync("/api/bookings", content);
            if (!resp.IsSuccessStatusCode)
            {
                TempData["Error"] = await resp.Content.ReadAsStringAsync();
                return RedirectToAction(nameof(SelectSeats), new { screeningId });
            }
            var json = await resp.Content.ReadAsStringAsync();
            var booking = JsonSerializer.Deserialize<BookingDto>(json, new JsonSerializerOptions{PropertyNameCaseInsensitive=true})!;
            return View("Confirmation", booking);
        }
    }

    public class SeatDto { public int Id { get; set; } public int Row { get; set; } public int Number { get; set; } public bool IsBooked { get; set; } }
    public class BookingCreateDto { public int ScreeningId { get; set; } public int CustomerId { get; set; } public List<int> SeatIds { get; set; } = new(); public string? PromoCode { get; set; } }
    public class BookingDto { public int Id { get; set; } public int ScreeningId { get; set; } public int CustomerId { get; set; } public int Seats { get; set; } public decimal TotalPrice { get; set; } public DateTime BookingTime { get; set; } }
}
