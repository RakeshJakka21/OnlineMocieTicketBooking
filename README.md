
# BookMyShow-style Online Movie Ticket Booking (ASP.NET Core MVC + Web API)

This solution mirrors BookMyShow-like flows: browse movies with posters, select showtimes, choose seats on a grid, apply promo codes, and confirm bookings.

Projects:
- **MovieBooking.Api** – ASP.NET Core Web API (net8.0) + EF Core SQL Server, Swagger, HealthChecks, central middleware, seat-level booking.
- **MovieBooking.Web** – ASP.NET Core MVC client (Bootstrap + Tailwind) with home hero banner, movie cards, seat selection UI.
- **MovieBooking.Tests** – xUnit tests with EF InMemory for JOIN/GROUP BY and seat booking.
- **PromoCodeService** – Minimal microservice for promo validation.

## Run
```bash
dotnet restore
# API
dotnet run --project MovieBooking.Api
# Promo Service (optional)
dotnet run --project PromoCodeService
# Web
dotnet run --project MovieBooking.Web
```

Set `MovieBooking.Web/appsettings.json` `ApiBaseUrl` to API base (e.g., `https://localhost:5001`).
Set `MovieBooking.Api/appsettings.json` `PromoServiceBaseUrl` if using the promo microservice (e.g., `https://localhost:5101`).

## Endpoints
- `GET /api/screenings/details` **JOIN** (Screening + Movie + Theater)
- `GET /api/reports/bookings-by-movie` **GROUP BY** (Bookings per movie)
- `GET /api/seats/screening/{id}` Seat map for the screening
- Booking `POST /api/bookings` accepts `{screeningId, customerId, seatIds[], promoCode?}`

## Notes
- Uses `EnsureCreated()` with seed data (movies, theaters, screenings, customers, **seats**) for a working demo.
- Posters and hero banner are included under `wwwroot/images`.
- Styling: Bootstrap 5 + Tailwind CDN.
