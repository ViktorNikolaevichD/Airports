using AirportsAndFlights.Entities;
using Microsoft.EntityFrameworkCore;

namespace AirportsAndFlights
{
    internal class AppDbContext : DbContext
    {
        public DbSet<Airport> Airports { get; set; }
        public DbSet<Flight> Flights { get; set; }

        public AppDbContext()
        {
            // Проверка базы данных на существование
            Database.EnsureCreated();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Flight>()
                .HasOne(p => p.DepartureAirport)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Flight>()
                .HasOne(p => p.ArrivalAirport)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Строка подключения к базе данных
            optionsBuilder.UseSqlServer("Server=localhost;Database=Airports;Trusted_Connection=True;Encrypt=False;");
        }
    }
}
