using Airports.Entities;
using Microsoft.EntityFrameworkCore;

namespace Airports
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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Строка подключения к базе данных
            optionsBuilder.UseSqlServer("Server=localhost;Database=Airports;Trusted_Connection=True;Encrypt=False;");
        }
    }
}
