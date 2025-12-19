
using Microsoft.EntityFrameworkCore;
using MovieBooking.Api.Models;

namespace MovieBooking.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Movie> Movies => Set<Movie>();
        public DbSet<Theater> Theaters => Set<Theater>();
        public DbSet<Screening> Screenings => Set<Screening>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<Seat> Seats => Set<Seat>();
        public DbSet<BookingSeat> BookingSeats => Set<BookingSeat>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Movie>().Property(p => p.Rating).HasPrecision(3,1);
            modelBuilder.Entity<Screening>().Property(p => p.Price).HasPrecision(10,2);

            modelBuilder.Entity<Screening>()
                .HasOne(s => s.Movie).WithMany(m => m.Screenings).HasForeignKey(s => s.MovieId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Screening>()
                .HasOne(s => s.Theater).WithMany(t => t.Screenings).HasForeignKey(s => s.TheaterId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Screening).WithMany(s => s.Bookings).HasForeignKey(b => b.ScreeningId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Customer).WithMany(c => c.Bookings).HasForeignKey(b => b.CustomerId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Seat>()
                .HasOne(s => s.Screening).WithMany(sc => sc.Seats).HasForeignKey(s => s.ScreeningId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BookingSeat>()
                .HasOne(bs => bs.Booking).WithMany(b => b.BookingSeats).HasForeignKey(bs => bs.BookingId);
            modelBuilder.Entity<BookingSeat>()
                .HasOne(bs => bs.Seat).WithMany(s => s.BookingSeats).HasForeignKey(bs => bs.SeatId);
        }
    }

    public static class SeedData
    {
        public static void Initialize(ApplicationDbContext db)
        {
            if (!db.Movies.Any())
            {
                var movies = new List<Movie>{
                    new Movie{ Title = "Inception", Description = "Dream heist", DurationMinutes = 148, Rating = 8.8M, PosterPath = "/images/posters/inception.png" },
                    new Movie{ Title = "Interstellar", Description = "Space exploration", DurationMinutes = 169, Rating = 8.6M, PosterPath = "/images/posters/interstellar.png" },
                    new Movie{ Title = "The Dark Knight", Description = "Batman vs Joker", DurationMinutes = 152, Rating = 9.0M, PosterPath = "/images/posters/darkknight.png" }
                };
                db.Movies.AddRange(movies);
                var theaters = new List<Theater>{
                    new Theater{ Name = "Pinelands Cinema", Location = "Pinelands" },
                    new Theater{ Name = "Waterfront IMAX", Location = "Cape Town" }
                };
                db.Theaters.AddRange(theaters);
                db.SaveChanges();

                var screenings = new List<Screening>{
                    new Screening{ MovieId = db.Movies.First().Id, TheaterId = db.Theaters.First().Id, StartTime = DateTime.Now.AddHours(2), Price = 120.00M },
                    new Screening{ MovieId = db.Movies.Skip(1).First().Id, TheaterId = db.Theaters.Skip(1).First().Id, StartTime = DateTime.Now.AddHours(4), Price = 150.00M },
                    new Screening{ MovieId = db.Movies.Skip(2).First().Id, TheaterId = db.Theaters.First().Id, StartTime = DateTime.Now.AddHours(6), Price = 130.00M }
                };
                db.Screenings.AddRange(screenings);
                db.Customers.AddRange(new []{
                    new Customer{ FullName = "Rakesh Jakka", Email = "rakesh@example.com" },
                    new Customer{ FullName = "Alex Doe", Email = "alex@example.com" },
                });
                db.SaveChanges();

                // Generate seats 5x10 for each screening
                foreach (var sc in screenings)
                {
                    for (int r = 1; r <= 5; r++)
                    {
                        for (int n = 1; n <= 10; n++)
                        {
                            db.Seats.Add(new Seat { ScreeningId = sc.Id, Row = r, Number = n, IsBooked = false });
                        }
                    }
                }
                db.SaveChanges();

                var cust = db.Customers.First();
                db.Bookings.Add(new Booking{ ScreeningId = db.Screenings.First().Id, CustomerId = cust.Id, Seats = 2, TotalPrice = 240.00M, BookingTime = DateTime.Now });
                db.SaveChanges();
            }
        }
    }
}
