using Microsoft.EntityFrameworkCore;
using MovieBooking.Api.Data;
using MovieBooking.Api.Models;
using FluentAssertions;
using Xunit;
using System;
using System.Linq;

namespace MovieBooking.Tests
{
    public class ReportsTests
    {
        private ApplicationDbContext CreateDb()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var db = new ApplicationDbContext(options);

            db.Movies.AddRange(
                new Movie { Title = "A", DurationMinutes = 100, Rating = 7.1M },
                new Movie { Title = "B", DurationMinutes = 90, Rating = 6.5M }
            );

            db.Theaters.Add(new Theater { Name = "T1", Location = "L1" });
            db.SaveChanges();

            var theater = db.Theaters.First();
            var movieA = db.Movies.First();
            var movieB = db.Movies.Skip(1).First();

            db.Screenings.AddRange(
                new Screening
                {
                    MovieId = movieA.Id,
                    TheaterId = theater.Id,
                    StartTime = DateTime.UtcNow,
                    Price = 100M
                },
                new Screening
                {
                    MovieId = movieB.Id,
                    TheaterId = theater.Id,
                    StartTime = DateTime.UtcNow.AddHours(1),
                    Price = 120M
                }
            );

            db.Customers.Add(new Customer { FullName = "Test", Email = "t@t" });
            db.SaveChanges();

            var customer = db.Customers.First();
            var screeningA = db.Screenings.First();
            var screeningB = db.Screenings.Skip(1).First();

            db.Bookings.AddRange(
                new Booking
                {
                    ScreeningId = screeningA.Id,
                    CustomerId = customer.Id,
                    Seats = 2,
                    TotalPrice = 200M,
                    BookingTime = DateTime.UtcNow
                },
                new Booking
                {
                    ScreeningId = screeningA.Id,
                    CustomerId = customer.Id,
                    Seats = 1,
                    TotalPrice = 100M,
                    BookingTime = DateTime.UtcNow
                },
                new Booking
                {
                    ScreeningId = screeningB.Id,
                    CustomerId = customer.Id,
                    Seats = 3,
                    TotalPrice = 360M,
                    BookingTime = DateTime.UtcNow
                }
            );

            db.SaveChanges();
            return db;
        }

        [Fact]
        public void GroupBy_BookingsPerMovie_ShouldReturnCounts()
        {
            using var db = CreateDb();

            var result = db.Bookings
                .Include(b => b.Screening)
                .ThenInclude(s => s.Movie)
                .GroupBy(b => b.Screening.Movie.Title)
                .Select(g => new { Movie = g.Key, Bookings = g.Count() })
                .OrderByDescending(r => r.Bookings)
                .ToList();

            result.Should().Contain(x => x.Movie == "A" && x.Bookings == 2);
            result.Should().Contain(x => x.Movie == "B" && x.Bookings == 1);
        }

        [Fact]
        public void Join_ScreeningDetails_ShouldProjectMovieTheater()
        {
            using var db = CreateDb();

            var query = db.Screenings
                .Include(s => s.Movie)
                .Include(s => s.Theater)
                .Select(s => new
                {
                    Movie = s.Movie.Title,
                    Theater = s.Theater.Name,
                    Location = s.Theater.Location
                })
                .ToList();

            query.Should().NotBeEmpty();
            query.First().Movie.Should().Be("A");
        }
    }

    public class SeatBookingTests
    {
        [Fact]
        public void Booking_MarksSeatsBooked()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var db = new ApplicationDbContext(options);

            var movie = new Movie { Title = "Test", DurationMinutes = 100, Rating = 7.0M };
            var theater = new Theater { Name = "T1", Location = "L1" };

            db.Movies.Add(movie);
            db.Theaters.Add(theater);
            db.SaveChanges();

            var screening = new Screening
            {
                MovieId = movie.Id,
                TheaterId = theater.Id,
                StartTime = DateTime.UtcNow,
                Price = 100M
            };

            db.Screenings.Add(screening);
            db.SaveChanges();

            for (int i = 1; i <= 5; i++)
            {
                db.Seats.Add(new Seat
                {
                    ScreeningId = screening.Id,
                    Row = 1,
                    Number = i,
                    IsBooked = false
                });
            }

            db.Customers.Add(new Customer { FullName = "C1", Email = "c@e" });
            db.SaveChanges();

            var customer = db.Customers.First();
            var seats = db.Seats
                .Where(s => s.ScreeningId == screening.Id)
                .Take(2)
                .ToList();

            var booking = new Booking
            {
                ScreeningId = screening.Id,
                CustomerId = customer.Id,
                Seats = seats.Count,
                TotalPrice = seats.Count * screening.Price,
                BookingTime = DateTime.UtcNow
            };

            db.Bookings.Add(booking);
            db.SaveChanges();

            foreach (var seat in seats)
            {
                seat.IsBooked = true;
                db.BookingSeats.Add(new BookingSeat
                {
                    BookingId = booking.Id,
                    SeatId = seat.Id
                });
            }

            db.SaveChanges();

            db.Seats.Count(s => s.IsBooked).Should().Be(2);
        }
    }
}
